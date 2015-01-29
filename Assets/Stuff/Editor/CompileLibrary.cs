using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;

namespace RelationsInspector.Stuff
{
	/*
	public class CompileLibrary
	{
		public const string
			sourceDirName = "Assets/RelationsInspectorLibrary",
			buildTargetDirName = "BuiltDLL",
			dllFileName = "RelationsInspector.dll";


		[MenuItem("Window/Build/Compile dll")]
		static void RunCompile()
		{
			CompileDLL();
		}

		public static bool CompileDLL()
		{
			var compileParams = new CompilerParameters();
			compileParams.OutputAssembly = Path.Combine(buildTargetDirName, dllFileName);	//Path.Combine(//System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), kBuildTargetFilename);
			compileParams.CompilerOptions = "/optimize";
			compileParams.ReferencedAssemblies.Add(Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEngine.dll"));
			compileParams.ReferencedAssemblies.Add(Path.Combine(EditorApplication.applicationContentsPath, "Managed/UnityEditor.dll"));
			var source = getSourceForStandardDLL(sourceDirName);
			var codeProvider = new CSharpCodeProvider(new Dictionary<string, string> { { "CompilerVersion", "v3.0" } });
			var compilerResults = codeProvider.CompileAssemblyFromSource(compileParams, source);
			if (compilerResults.Errors.Count > 0)
			{
				foreach (var error in compilerResults.Errors)
					Debug.LogError(error.ToString());
				return false;
			}
			else
			{
				Debug.Log("build successfull");
				return true;
			}
		}

		static string[] getSourceForStandardDLL(string path)
		{
			return Directory.GetFiles(path, "*.cs", SearchOption.AllDirectories).Select(filePath => File.ReadAllText(filePath)).ToArray();
		}
	}

	public class MakeFreshPackage
	{
		[MenuItem("Window/Build/compile and prepare export")]
		static void CompileAndPrepareForExport()
		{
			if (!CompileLibrary.CompileDLL())
				return;

			// the inspector gets upset when we delete the config, so close it
			var windows = Resources.FindObjectsOfTypeAll<RelationsInspectorWindow>();
			for (int i = 0; i < windows.Length; i++)
				windows[i].Close();
			//EditorWindow.GetWindow<RelationsInspectorWindow>().Close();

			//EditorApplication.LockReloadAssemblies();
			//AssetDatabase.StartAssetEditing();

			AssetDatabase.DeleteAsset( Path.Combine(ProjectSettings.ResourcesPath, ProjectSettings.defaultConfigName + ".asset") );
			//File.Delete( Path.Combine(ProjectSettings.ResourcesPath, ProjectSettings.defaultConfigName + ".asset"));

			// this will force a recompile
			
			IOUtil.FileCopy(CompileLibrary.dllFileName, CompileLibrary.buildTargetDirName, @"Assets\RelationsInspector\Editor");

			//File.Delete(Path.Combine(@"Assets\RelationsInspector\Editor", CompileLibrary.dllFileName));

			//AssetDatabase.StopAssetEditing();
			//AssetDatabase.Refresh();
			//EditorApplication.UnlockReloadAssemblies();
		}
	}

	public class Deploy
	{
		const string assetDirName = "Assets";
		const string userLandDirName = "RelationsInspector";
		const string testProjPath = "I:/GenericGraphEditor/UseCases/SurvivalShooter";

		static string userLandPath { get { return Path.Combine(assetDirName, userLandDirName); } }
		static string testAssetPath { get { return Path.Combine(testProjPath, assetDirName); } }
		static string testUserLandPath { get { return Path.Combine(testAssetPath, userLandDirName); } }
		
		[MenuItem("Window/Build/compile and move to test proj")]
		static void CompileAndMove()
		{
			if (!CompileLibrary.CompileDLL())
				return;

			// copy userland files to target
			IOUtil.DirectoryCopy(userLandPath, testUserLandPath, true);
			IOUtil.FileCopy(CompileLibrary.dllFileName, CompileLibrary.buildTargetDirName, Path.Combine(testUserLandPath, "Editor"));
		}		
	}*/
}
