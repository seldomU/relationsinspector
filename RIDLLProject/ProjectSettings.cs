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
        public const string layoutCacheDirectoryName = "LayoutCaches";
		public const string expectedRIBasePath = @"Assets\RelationsInspector\Editor";

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
					resourcesPath = FindRIDirectoryPath(resourcesDirectoryName);
				return resourcesPath;
			}
		}

        static string layoutCachesPath;
        public static string LayoutCachesPath
        {
            get
            {
                if (layoutCachesPath == null)
                    layoutCachesPath = FindRIDirectoryPath(layoutCacheDirectoryName);
                return layoutCachesPath;
            }
        }

        // change this: demand that RelationsInspector\Editor exists, it just doesn't have to be in "Assets"
        public static string FindRIDirectoryPath(string directoryName)
        {
            // try the expected path first
            string expectedPath = Path.Combine(expectedRIBasePath, directoryName);
            if (Directory.Exists(expectedPath))
                return expectedPath;

            // search all subdirectories of Assets
            var searchPattern = @"*" + directoryName;
            string path = Directory.GetDirectories("Assets", searchPattern, SearchOption.AllDirectories).FirstOrDefault();
            if (path != null)
                return path;

            Directory.CreateDirectory(expectedPath);
            return expectedPath;
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
