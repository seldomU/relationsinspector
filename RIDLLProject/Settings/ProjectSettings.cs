using System.IO;
using System.Linq;

namespace RelationsInspector
{
    public static class ProjectSettings
	{
		public const string DefaultBackendClassName = "SceneHierarchyBackend";
        public const string AutoBackendClassName = "RIAutoBackend";
		public const string EditorPrefsProjectPrefix = "Relations inspector";

		public const string resourcesDirectoryName = "RelationsInspectorResources";
        public const string layoutCacheDirectoryName = "LayoutCaches";

        const string darkSkinName = "RIWindowDarkSkin.asset";
        const string lightSkinName = "RIWindowLightSkin.asset";
        const string SettingsFileName = "Settings.asset";

        public const string expectedRIBasePath = @"Assets\RelationsInspector\Editor";
        public const string dllName = "RelationsInspector.dll";
        public const string ProgramVersion = "1.0.0";


        public static string[] obligatoryFileNames = new[]
		{
			"ArrowHead.png", "nextIconDark.png", "nextIconLight.png", "prevIconDark.png", "prevIconLight.png", "settingsIconLight.png", "settingsIconDark.png"
		};

        public static string RIBasePath { get; private set; }
        public static string ResourcesPath { get; private set; }
        public static string LayoutCachesPath { get; private set; }
        public static string SettingsPath { get; private set; }
        public static string LightSkinPath { get; private set; }
        public static string DarkSkinPath { get; private set; }

        static ProjectSettings()
        {
            RIBasePath = FindRIBasePath();
            LayoutCachesPath = GetAbsoluteRIDirectoryPath( layoutCacheDirectoryName );
            ResourcesPath = GetAbsoluteRIDirectoryPath( resourcesDirectoryName );
            SettingsPath = Path.Combine( ResourcesPath, SettingsFileName );
            LightSkinPath = Path.Combine( ResourcesPath, lightSkinName );
            DarkSkinPath = Path.Combine( ResourcesPath, darkSkinName );
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
