using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;
using System.IO;

public class ExportPackage
{
	[MenuItem("Window/Build/Export package")]
	public static void DoExportPackage()
	{
		string[] includedFilePaths = new[] { @"Assets\RelationsInspector", @"Assets\Editor Default Resources" };
		string packagePath = "RelationsInspector.unitypackage";

		PreparePackageExport();	// remove files not intended for distribution
		AssetDatabase.ExportPackage(includedFilePaths, packagePath, ExportPackageOptions.Recurse);
	}

	[MenuItem("Window/Build/Export demo package")]
	public static void DoExportDemoPackage()
	{
		string[] includedFilePaths = new[] { @"Assets\RelationsInspector", @"Assets\Editor Default Resources" };
		string packagePath = "RelationsInspectorDemo.unitypackage";

		PreparePackageExport();	// remove files not intended for distribution
		AssetDatabase.ExportPackage(includedFilePaths, packagePath, ExportPackageOptions.Recurse);
	}

	static void PreparePackageExport()
	{
		// close all window instances (so that we can remove the config)
		var windows = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>();
		for (int i = 0; i < windows.Length; i++)
			windows[i].Close();

		// remove debug symbol files
		AssetDatabase.DeleteAsset(@"Assets\RelationsInspector\Editor\RelationsInspector.dll.mdb");
		AssetDatabase.DeleteAsset(@"Assets\RelationsInspector\Editor\RelationsInspector.pdb");

	}
}
