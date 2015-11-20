using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;

class BuildPackage
{
    const string projectPath = @"I:\code\RelationsInspector\";

    // log file path
    static string logDir = projectPath + @"BuildLogs\";
    const string logFileName = "buildLog";

    // ri dll build settings
    const string msbuildPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
    static string projectToBuild = projectPath + @"RIDLLProject\RelationsInspectorLib.csproj";
    const string releaseBuildConfig = "/property:Configuration=Release;DefineConstants=\"RELEASE\"";
    const string demoBuildConfig = "/property:Configuration=Release;DefineConstants=\"RELEASE;RIDEMO\"";
    const string buildTarget = @"/target:Rebuild";
    const string relDllPath = @"Assets\RelationsInspector\Editor\RelationsInspector.dll";
    static string dllPath = projectPath + relDllPath;

    // ri dll zip settings
    const string zipPath = @"C:\Program Files\7-Zip\7z";
    static string sourceCodePath = @"RIDLLProject\*";
    public const string relSourceArchivePath = @"Assets\RelationsInspector\Editor\SourceCodeRI.zip";
    static string absSourceArchivePath = projectPath + relSourceArchivePath;
    const string excludePatterns = "-xr!obj -xr!bin -xr!.vs -x!*.csproj -x!*.sln -x!*.suo -x!*.user -x!DemoRestriction.cs";


    class BuildStep
    {
        public string title;
        public Func<string> action;
        public Func<bool> isCompleted;
        public float maxDuration;
    }

    [MenuItem("Window/Build release")]
    public static void Release()
    {
        Build( false );
    }

    [MenuItem( "Window/Build demo" )]
    public static void Demo()
    {
        Build( true );
    }

    static string PackageName( bool demo )
    {
        return demo ? ExportPackage.demoPackageName : ExportPackage.releasePackageName;
    }

    static string TryCopy( string sourcePath, string targetPath )
    {
        try
        {
            File.Copy( sourcePath, targetPath );
        }
        catch ( Exception e )
        {
            return string.Format( "failed to copy {0} to {1}, exception: {2}", sourcePath, targetPath, e );
        }
        return string.Empty;
    }

    static void Build( bool demo )
    {
        string log = DoBuildPackage( demo );

        // use the dll version as build id
        // if the dll is missing, use the date instead
        string buildId;
        try
        {
            buildId = System.Reflection.AssemblyName.GetAssemblyName( relDllPath ).Version.ToString();
        }
        catch (Exception e)
        {
            log += "\nFailed to retrieve assembly version: " + e.ToString();
            // fall back to date-based naming
            buildId = DateTime.Now.ToString( "yyyy-MM-dd-HH-mm-ss" ) + ".txt";
        }

        // copy the package
        string buildDir = logDir + "Log" + buildId + @"\";
        Directory.CreateDirectory( buildDir );
        log += TryCopy( projectPath + PackageName( demo ), buildDir + PackageName( demo ) );

        // flush the log
        string uniquePath = buildDir + logFileName + buildId + ".txt";
        string path = logDir + logFileName + ".txt";
        string text = log.ToString();
        File.WriteAllText( uniquePath, text);
        File.WriteAllText( path, text );

        // return when the files are written
        WaitFor( () => File.Exists( uniquePath ) && File.Exists( path ), 1000 );
    }

    static string RunSteps( List<BuildStep> steps )
    {
        var log = new StringBuilder();

        foreach ( var step in steps )
        {
            log.AppendLine( "entering step " + step.title );
            try
            {
                log.Append( step.action() );
                if ( !WaitFor( step.isCompleted, step.maxDuration ) )
                {
                    log.AppendLine( step.title + " timed out" );
                    break;
                }
            }
            catch (Exception e)
            {
                log.AppendLine( e.ToString() );
                break;
            }
        }

        return log.ToString();
    }

    static BuildStep DeleteFileStep( string path )
    {
        return new BuildStep()
        {
            title = "Deleting " + path,
            action = () => 
            {
                if ( File.Exists( path ) )
                {
                    File.SetAttributes( path, FileAttributes.Normal );
                    File.Delete( path );
                }
                return "";
            },
            isCompleted = () => !File.Exists( path ),
            maxDuration = 1000
        };
    }

    static string DoBuildPackage( bool demo )
    {
        var steps = new List<BuildStep>();
        
        // delete the dll
        steps.Add( DeleteFileStep( dllPath ) );
        
        // delete the source archive
        steps.Add( DeleteFileStep( absSourceArchivePath ) );

        // build the dll
        var config = demo ? demoBuildConfig : releaseBuildConfig;
        var buildArgs = new[] { projectToBuild, buildTarget, config };
        RunSysCmd( msbuildPath, buildArgs );
        steps.Add( new BuildStep() {
            title = "compiling RI DLL config " + config,
            action = () => RunSysCmd( msbuildPath, buildArgs, 2000 ),
            isCompleted = () => File.Exists( dllPath ),
            maxDuration = 3000
        } );

        // archive the source
        if ( !demo ) {

            steps.Add( new BuildStep() {
                title = "archiving RI source to file " + relSourceArchivePath,
                action = () => RunSysCmd( zipPath, new[] { "a", "-tzip", absSourceArchivePath, sourceCodePath, excludePatterns } ),
                isCompleted = () => File.Exists( absSourceArchivePath ),
                maxDuration = 2000
            } );
        }

        // refresh the asset db
        string[] requiredAssetPaths = demo ? new[] { relDllPath } : new[] { relDllPath, relSourceArchivePath };
        steps.Add( new BuildStep()
        {
            title = "refreshing asset db",
            action = () => { AssetDatabase.Refresh(); return ""; },
            isCompleted = () => requiredAssetPaths.All( path => AssetExists( path ) ),
            maxDuration = 3000
        } );

        // build the package
        string packagePath = projectPath + PackageName( demo );
        steps.Add( new BuildStep()
        {
            title = "exporting asset package to " + packagePath,
            action = () => { ExportPackage.CreatePackage( demo ); return ""; },
            isCompleted = () => File.Exists(packagePath),
            maxDuration = 4000
        } );

        // remove the source archive (not really necessary, since its deleted at the start of the process
        steps.Add( DeleteFileStep( absSourceArchivePath ) );
        return RunSteps( steps );
    }

    public static string RunSysCmd( string cmdPath, string[] args, int timeout = 0 )
    {
        var log = new StringBuilder();
        log.AppendLine( "running sys cmd: " + cmdPath + " with args " + args);

        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = cmdPath;
        process.StartInfo.Arguments = string.Join( " ", args );
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        //* Set your output and error (asynchronous) handlers
        process.OutputDataReceived += ( s, e ) => log.AppendLine( e.Data );
        process.ErrorDataReceived += ( s, e ) => log.AppendLine( e.Data ); 
        //* Start process and handlers
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        if ( timeout == 0 )
        {
            process.WaitForExit();
            log.AppendLine( "finished sys cmd " + cmdPath );
        }
        else if ( !process.WaitForExit( timeout ) )
        {
            process.Kill();
            log.AppendLine( "sys cmd killed due to timeout: " + cmdPath );
        }
        else
        {
            log.AppendLine( "finished sys cmd " + cmdPath );
        }
        return log.ToString();
    }

    // stalls execution until condition is true or maxTime has passed
    // returns true if condition is true
    public static bool WaitFor( Func<bool> condition, float maxTime )
    {
        var timer = System.Diagnostics.Stopwatch.StartNew();
        while ( !condition() )
        {
            if ( timer.ElapsedMilliseconds > maxTime )
                return false;

            System.Threading.Thread.Sleep( 100 );
        }
        return true;
    }

    static bool AssetExists( string path )
    {
        return null != AssetDatabase.LoadAssetAtPath( path, typeof( UnityEngine.Object ) );
    }
}
