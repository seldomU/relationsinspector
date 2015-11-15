using UnityEngine;
using UnityEditor;
using RelationsInspector;
using System.IO;

public class ExportPackage
{
    // don't include these in builds
    static string[] excludePaths = new[] 
    {
        ProjectSettings.LightSkinPath,
        ProjectSettings.DarkSkinPath,
        ProjectSettings.SettingsPath,
        ProjectSettings.LayoutCachesPath
    };

    const string packageBasePath = @"Assets\RelationsInspector";
    public const string releasePackageName = "RelationsInspector.unitypackage";
    public const string demoPackageName = "RelationsInspectorDemo.unitypackage";

    public static void CreatePackage( bool demo )
    {
        if ( demo )
            DoExportDemoPackage();
        else
            DoExportPackage();
    }

    [ MenuItem("Window/Build/Export package")]
	public static void DoExportPackage()
	{
        RunBuild( releasePackageName );
	}

	[MenuItem("Window/Build/Export demo package")]
	public static void DoExportDemoPackage()
	{
        // no source code archive!
        AssetDatabase.DeleteAsset( BuildPackage.relSourceArchivePath );
        RunBuild( demoPackageName );
    }

    static void RunBuild(string packageName)
    {
        string[] includedFilePaths = new[] { packageBasePath };

        PreparePackageExport(); // remove files not intended for distribution
        AssetDatabase.ExportPackage( includedFilePaths, packageName, ExportPackageOptions.Recurse );
        CleanupPackageExport();

    }

    [MenuItem( "Window/Build/RunPrep" )]    // menuitem for testing
    static void PreparePackageExport()
	{
        // close all window instances (so that we can remove the config)
		var windows = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>();
		for (int i = 0; i < windows.Length; i++)
			windows[i].Close();

		// remove debug symbol files
		AssetDatabase.DeleteAsset(@"Assets\RelationsInspector\Editor\RelationsInspector.dll.mdb");
		AssetDatabase.DeleteAsset(@"Assets\RelationsInspector\Editor\RelationsInspector.pdb");

        
        foreach ( var path in excludePaths )
            AssetDatabase.MoveAsset( path, GetTempPath( path ) );
	}

    [MenuItem( "Window/Build/RunCleanup" )]     // menuitem for testing
    static void CleanupPackageExport()
    {
        foreach ( var path in excludePaths )
            AssetDatabase.MoveAsset( GetTempPath( path ), path );

        AssetDatabase.DeleteAsset( BuildPackage.relSourceArchivePath );
    }

    static string GetTempPath( string path )
    {
        return Path.Combine( @"Assets\Stuff", Path.GetFileName( path ) );
    }
}
