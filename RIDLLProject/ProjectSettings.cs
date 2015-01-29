using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Linq;

namespace RelationsInspector
{
	public static class ProjectSettings
	{
		public const string DefaultBackendClassName = "SceneHierarchyBackend";
		public const string EditorPrefsProjectPrefix = "Relations inspector";

		public const string resourcesDirectoryName = "RelationsInspectorResources";
		public const string expectedResourcesBasePath = @"Assets\RelationsInspector\Editor";

		public static string[] obligatoryFileNames = new[]
	{
		"ArrowHead.png",
	};

		static string resourcesPath;
		public static string ResourcesPath
		{
			get
			{
				if (resourcesPath == null)
					resourcesPath = FindResourcesPath();
				return resourcesPath;
			}
		}

		public static string FindResourcesPath()
		{
			// try the expected path first
			var expectedResourcePath = Path.Combine(expectedResourcesBasePath, resourcesDirectoryName);
			if (Directory.Exists(expectedResourcePath))
				return expectedResourcePath;

			// search all subdirectories of Assets
			var searchPattern = @"*" + resourcesDirectoryName;
			return Directory.GetDirectories("Assets", searchPattern, SearchOption.AllDirectories).FirstOrDefault();
		}

		internal static string CheckDependentFiles()
		{
			// make sure the resources folder was found
			if (string.IsNullOrEmpty(ResourcesPath))
				return "Could not find directory " + resourcesDirectoryName;

			// make sure it contains all required files
			foreach (var fileName in obligatoryFileNames)
				if (!File.Exists(Path.Combine(ResourcesPath, fileName)))
					return "Could not find resource file " + fileName;

			// all good
			return string.Empty;
		}
	}
}
