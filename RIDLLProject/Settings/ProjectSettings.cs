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
        public const string dllName = "RelationsInspector.dll";
        public const string ProgramVersion = "1.0.0";

        public static string[] obligatoryFileNames = new[]
		{
			"ArrowHead.png",
		};

        public static string RIBasePath { get; private set; }
        public static string ResourcesPath { get; private set; }
        public static string LayoutCachesPath { get; private set; }

        static ProjectSettings()
        {
            RIBasePath = FindRIBasePath();
            ResourcesPath = GetAbsoluteRIDirectoryPath( resourcesDirectoryName );
            LayoutCachesPath = GetAbsoluteRIDirectoryPath( layoutCacheDirectoryName );
        }

        static string FindRIBasePath()
        {
            if ( Directory.Exists( expectedRIBasePath ) )
                return expectedRIBasePath;

            string path = Directory.GetFiles( "Assets", dllName, SearchOption.AllDirectories ).SingleOrDefault();
            if ( !string.IsNullOrEmpty( path ) )
                return Path.GetDirectoryName( path );   // path has to be absolute

            // no dll, no party. The editor should throw a warning and die
            return null;
        }

        static string GetAbsoluteRIDirectoryPath( string relativeDirPath )
        {
            string absoluteDirPath = Path.Combine( RIBasePath, relativeDirPath);

            if( !Directory.Exists(absoluteDirPath) )
                Directory.CreateDirectory( absoluteDirPath );

            return absoluteDirPath;
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
