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
    const string buildTarget = "/target:Rebuild";
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
    /*public static void Release()
    {
        var log = new StringBuilder();

        // compile the dll
        var buildArgs = new[] { projectToBuild, buildTarget, releaseBuildConfig };
        RunSysCmd( msbuildPath, buildArgs, log );

        // zip the ri dll source
        RunSysCmd( zipPath, new[] { "a", "-tzip", absSourceArchivePath, sourceCodePath, excludePatterns }, log );

        System.Func<bool> contentFilesReady = () => File.Exists( absSourceArchivePath ) && File.Exists( dllPath ) ;
        WaitFor( contentFilesReady, 2000 );

        // build the package
        ExportPackage.DoExportPackage();

        // wait for the package
        WaitFor( () => File.Exists( projectPath + ExportPackage.releasePackageName ), 5000 );

        // remove the source archive
        File.Delete( absSourceArchivePath );

        // flush the log
        File.WriteAllText( logFilePath, log.ToString() );
    }

    public static void Demo()
    {
        var log = new StringBuilder();

        var buildArgs = new[] { projectToBuild, buildTarget, demoBuildConfig };
        RunSysCmd( msbuildPath, buildArgs, log );

        // precaution: remove the source zip, if it somehow exists
        File.Delete( absSourceArchivePath );

        System.Func<bool> contentFilesReady = () => !File.Exists( absSourceArchivePath ) && File.Exists( dllPath );
        WaitFor( contentFilesReady, 2000 );

        // build the package
        ExportPackage.DoExportDemoPackage();

        // flush the log
        File.WriteAllText( logFilePath, log.ToString() );
    }*/

        [MenuItem("Window/Build release")]
    public static void Release()
    {
        DoBuildPackage( false );
    }

    [MenuItem( "Window/Build demo" )]
    public static void Demo()
    {
        DoBuildPackage( true );
    }

    static void DoBuildPackage( bool demo )
    {
        //var log = new StringBuilder();

        string log = DoBuildPackage3( demo );

        // flush the log
        //string uniquePath = logDir + logFileName + DateTime.Now.ToString() + ".txt";
        string path = logDir + logFileName + ".txt";
        string text = log.ToString();

        //File.WriteAllText( uniquePath, text);
        File.WriteAllText( path, text );

        // return when the files are written
        WaitFor( () => /*File.Exists( uniquePath ) &&*/ File.Exists( path ), 1000 );
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
            action = () => { File.Delete( path ); return ""; },
            isCompleted = () => !File.Exists( path ),
            maxDuration = 1000
        };
    }

    static string DoBuildPackage3( bool demo )
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
        string packagePath = projectPath + ( demo ? ExportPackage.demoPackageName : ExportPackage.releasePackageName );
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
    /*
    static void DoBuildPackage2( StringBuilder log, bool demo )
    {
        // build dll
        var config = demo ? demoBuildConfig : releaseBuildConfig;
        var buildArgs = new[] { projectToBuild, buildTarget, config };
        log.Append( RunSysCmd( msbuildPath, buildArgs ) );

        //System.Func<bool> riDllBuilt = () => File.Exists( dllPath );
        ;
        if ( !WaitFor( () => File.Exists( dllPath ), 2000 ) )
        {
            log.AppendLine( "ri dll build timed out. aborting." );
            return;
        }

        // relase build: zip the ri dll source
        if ( !demo )
            log.Append( RunSysCmd( zipPath, new[] { "a", "-tzip", absSourceArchivePath, sourceCodePath, excludePatterns } ) );
        else // demo build: make sure the file is not present
            File.Delete( absSourceArchivePath );

        System.Func<bool> sourceArchiveReady = () => File.Exists( absSourceArchivePath );
        if ( demo )
            sourceArchiveReady = () => !sourceArchiveReady();   // for the demo the zip has to be gone
        
        if ( !WaitFor( sourceArchiveReady, 1000 ) )
        {
            log.AppendLine( "adding/removing the source zip timed out. aborting." );
            return;
        }

        // build the package
        ExportPackage.CreatePackage( demo );
        string packagePath = projectPath + (demo ? ExportPackage.demoPackageName : ExportPackage.releasePackageName);
        // wait for the package
        if ( !WaitFor( () => File.Exists( packagePath ), 5000 ) )
        {
            log.AppendLine( "building the package timed out. aborting." );
        }

        // delete the source archive
        File.Delete( absSourceArchivePath );
        System.Func<bool> sourceArchiveDeleted = () => !File.Exists( absSourceArchivePath );
        if ( !WaitFor( sourceArchiveDeleted, 1000 ) )
        {
            log.AppendLine( "failed to delete source archive. aborting." );
            return;
        }
    }*/

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
    public static bool WaitFor( System.Func<bool> condition, float maxTime )
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
        return null != UnityEditor.AssetDatabase.LoadAssetAtPath( path, typeof( UnityEngine.Object ) );
    }
}
