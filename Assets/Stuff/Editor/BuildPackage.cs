using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using System.Linq;
using System.Diagnostics;

class BuildPackage
{
    const string projectPath = @"I:\code\RelationsInspector\";

    // root directory of the package
    const string packageRootDir = @"Assets\RelationsInspector";

    // log file path
    static string logDir = projectPath + @"BuildLogs\";
    const string logFileName = "buildLog";

    // ri dll build settings
    const string msbuildPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe";
    static string projectToBuild = projectPath + @"RIDLLProject\RelationsInspectorLib.csproj";
    const string buildTarget = @"/target:Rebuild";
    const string relDllPath = @"Assets\RelationsInspector\Editor\RelationsInspector.dll";
    const string relPDBPath = @"Assets\RelationsInspector\Editor\RelationsInspector.pdb";
    const string relMDBPath = @"Assets\RelationsInspector\Editor\RelationsInspector.dll.mdb";
    static string dllPath = projectPath + relDllPath;

    // ri dll zip settings
    const string zipPath = @"C:\Program Files\7-Zip\7z";
    static string sourceCodePath = @"RIDLLProject\*";
    public const string relSourceArchivePath = @"Assets\RelationsInspector\Editor\SourceCodeRI.zip";
    static string absSourceArchivePath = projectPath + relSourceArchivePath;
    const string excludePatterns = "-xr!obj -xr!bin -xr!.vs -x!*.csproj -x!*.sln -x!*.suo -x!*.user -x!DemoRestriction.cs";

    enum BuildMode { Release, Debug, Demo };

    class BuildSettings
    {
        public string config;
        public string packageName;
        public string[] requiredPaths;
    }

    static Dictionary<BuildMode, BuildSettings> buildSettings = new Dictionary<BuildMode, BuildSettings>()
    {
        {
            BuildMode.Release,
            new BuildSettings()
            {
                config = "/property:Configuration=Release;DefineConstants=\"RELEASE\"",
                packageName = "RelationsInspector.unitypackage",
                requiredPaths  = new[] {relDllPath, relSourceArchivePath }
            }
        },
        {
            BuildMode.Debug,
            new BuildSettings()
            {
                config = "/property:Configuration=Debug;DefineConstants=\"DEBUG\"",
                packageName = "RelationsInspectorDebug.unitypackage",
                requiredPaths  = new[] { relDllPath }
            }
        },
        {
            BuildMode.Demo,
            new BuildSettings()
            {
                config = "/property:Configuration=Release;DefineConstants=\"RELEASE;RIDEMO\"",
                packageName = "RelationsInspectorDemo.unitypackage",
                requiredPaths  = new[] { relDllPath }
            }
        }
    };

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
        Build( BuildMode.Release );
    }

    [MenuItem( "Window/Build demo" )]
    public static void Demo()
    {
        Build( BuildMode.Demo );
    }

    [MenuItem( "Window/Build debug" )]
    public static void Debug()
    {
        Build( BuildMode.Debug );
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

    static void Build( BuildMode mode )
    {
        string log = DoBuildPackage( mode );

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
        string packageName = buildSettings[ mode ].packageName;
        log += TryCopy( projectPath + packageName, buildDir + packageName );

        // flush the log
        string uniquePath = buildDir + logFileName + buildId + ".txt";
        string path = logDir + logFileName + ".txt";
        string text = log.ToString();
        File.WriteAllText( uniquePath, text);
        File.WriteAllText( path, text );

        // return when the files are written
        WaitFor( null, () => File.Exists( uniquePath ) && File.Exists( path ), 1000 );
    }

    static string RunSteps( List<BuildStep> steps )
    {
        var log = new StringBuilder();

        foreach ( var step in steps )
        {
            log.AppendLine( "entering step " + step.title );
            try
            {
                var timer = Stopwatch.StartNew();
                log.AppendLine( "step message: " + step.action() );

                if ( !WaitFor( timer, step.isCompleted, step.maxDuration ) )
                {
                    log.AppendLine( "timed out after " + timer.ElapsedMilliseconds + "ms" );
                    break;
                }
                log.AppendLine( "step completed after " + timer.ElapsedMilliseconds + " ms" );
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
            title = "Deleting file " + path,
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

    static BuildStep DeleteDirectoryStep( string path )
    {
        return new BuildStep()
        {
            title = "Deleting directory " + path,
            action = () =>
            {
                if ( Directory.Exists( path ) )
                {
                    Directory.Delete( path, true );
                }

                return "";
            },
            isCompleted = () => !Directory.Exists( path ),
            maxDuration = 1000
        };
    }

    static string DoBuildPackage( BuildMode mode )
    {
        var steps = new List<BuildStep>();
        
        // delete the dll
        steps.Add( DeleteFileStep( dllPath ) );
        
        // delete the source archive
        steps.Add( DeleteFileStep( absSourceArchivePath ) );

        // delete skin, settings, layout caches
        steps.Add( DeleteFileStep( RelationsInspector.ProjectSettings.LightSkinPath ) );
        steps.Add( DeleteFileStep( RelationsInspector.ProjectSettings.DarkSkinPath ) );
        steps.Add( DeleteFileStep( RelationsInspector.ProjectSettings.SettingsPath ) );
        steps.Add( DeleteDirectoryStep( RelationsInspector.ProjectSettings.LayoutCachesPath ) );

        // build the dll
        string config = buildSettings[ mode ].config;
        var buildArgs = new[] { projectToBuild, buildTarget, config };
        RunSysCmd( msbuildPath, buildArgs );
        steps.Add( new BuildStep() {
            title = "compiling RI DLL config " + config,
            action = () => RunSysCmd( msbuildPath, buildArgs, 2000 ),
            isCompleted = () => File.Exists( dllPath ),
            maxDuration = 3000
        } );

        // archive the source
        if ( mode == BuildMode.Release )
        {
            steps.Add( new BuildStep() {
                title = "archiving RI source to file " + relSourceArchivePath,
                action = () => RunSysCmd( zipPath, new[] { "a", "-tzip", absSourceArchivePath, sourceCodePath, excludePatterns } ),
                isCompleted = () => File.Exists( absSourceArchivePath ),
                maxDuration = 2000
            } );
        }

        // remove debug symbols
        if ( mode != BuildMode.Debug )
        {
            steps.Add( DeleteFileStep( projectPath + relPDBPath ) );
            steps.Add( DeleteFileStep( projectPath + relMDBPath ) );
        }

        // refresh the asset db
        steps.Add( new BuildStep()
        {
            title = "refreshing asset db",
            action = () => { AssetDatabase.Refresh(); return ""; },
            isCompleted = () => buildSettings[mode].requiredPaths.All( path => AssetExists( path ) ),
            maxDuration = 3000
        } );

        // build the package
        string packageName = buildSettings[ mode ].packageName;
        steps.Add( new BuildStep()
        {
            title = "exporting asset package to " + projectPath + packageName,
            action = () => 
            {
                AssetDatabase.ExportPackage( packageRootDir, packageName, ExportPackageOptions.Recurse );
                return string.Empty;
            },
            isCompleted = () => File.Exists(packageName),
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
    public static bool WaitFor( Stopwatch timer, Func<bool> condition, float maxTime )
    {
        if ( timer == null )
            timer = Stopwatch.StartNew();

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
