using System.IO;
using System.Linq;

namespace RelationsInspector
{
	public static class ProjectSettings
	{
		// identify files outside the dll by name
		public const string DefaultBackendClassName = "SceneHierarchyBackend";
		public const string AutoBackendClassName = "RIAutoBackend";
		public const string AutoBackendAttributeName = "AutoBackendAttribute"; // has to live in the game dll
		public const string BackendInstallPath = "Assets/Editor/GraphBackends/";

		public const string EditorPrefsProjectPrefix = "Relations inspector";

		const string resourcesDirectoryName = "RelationsInspectorResources";
		const string packagesDirectoryName = "Packages";
		const string layoutCacheDirectoryName = "LayoutCaches";
		const string welcomeWindowFolderName = "WelcomeWindow";

		const string darkSkinName = "RIWindowDarkSkin.asset";
		const string lightSkinName = "RIWindowLightSkin.asset";
		const string SettingsFileName = "Settings.asset";

		internal const string DocURL = @"https://github.com/seldomU/RIBackendUtil/wiki/RelationsInspector-Manual";
		internal const string DiscussionURL = @"http://forum.unity3d.com/threads/relationsinspector-reveal-structures-in-your-project-demo.382792/";
		internal const string YoutubeURL = @"https://www.youtube.com/channel/UC8LKk1BHomtq-ElMTGNYzJw/videos";
		internal const string TwitterURL = @"https://twitter.com/seldomU";
		internal const string StoreDemoURL = @"content/53147";

		public static string[] obligatoryFileNames = new[]
		{
			"ArrowHead.png",
			"nextIconDark.png",
			"nextIconLight.png",
			"prevIconDark.png",
			"prevIconLight.png",
			"settingsIconLight.png",
			"settingsIconDark.png"
		};

		public static string RIBasePath { get; private set; }
		public static string ResourcesPath { get; private set; }
		public static string WelcomeWindowResourcePath { get; private set; }
		public static string PackagesPath { get; private set; }
		public static string LayoutCachesPath { get; private set; }
		public static string SettingsPath { get; private set; }
		public static string LightSkinPath { get; private set; }
		public static string DarkSkinPath { get; private set; }

		static ProjectSettings()
		{
			RIBasePath = GetRIBasePath();
			LayoutCachesPath = GetAbsoluteRIDirectoryPath( layoutCacheDirectoryName );
			ResourcesPath = GetAbsoluteRIDirectoryPath( resourcesDirectoryName );
			PackagesPath = GetAbsoluteRIDirectoryPath( packagesDirectoryName );
			WelcomeWindowResourcePath = Path.Combine( ResourcesPath, welcomeWindowFolderName );
			SettingsPath = Path.Combine( ResourcesPath, SettingsFileName );
			LightSkinPath = Path.Combine( ResourcesPath, lightSkinName );
			DarkSkinPath = Path.Combine( ResourcesPath, darkSkinName );
		}

		static string GetRIBasePath()
		{
			string dllAbsPath = System.Reflection.Assembly.GetAssembly( typeof( RelationsInspectorWindow ) ).Location;
			string dllAssetPath = Util.AbsolutePathToAssetPath( dllAbsPath );
			return Path.GetDirectoryName( dllAssetPath );
		}

		static string GetAbsoluteRIDirectoryPath( string relativeDirPath )
		{
			string absoluteDirPath = Path.Combine( RIBasePath, relativeDirPath );

			if ( !Directory.Exists( absoluteDirPath ) )
				Directory.CreateDirectory( absoluteDirPath );

			return absoluteDirPath;
		}

		internal static string CheckDependentFiles()
		{
			// make sure the resources folder was found
			if ( string.IsNullOrEmpty( ResourcesPath ) )
				return "Could not find directory " + resourcesDirectoryName;

			// make sure it contains all required files
			foreach ( var fileName in obligatoryFileNames )
				if ( !File.Exists( Path.Combine( ResourcesPath, fileName ) ) )
					return "Could not find resource file " + fileName;

			// all good
			return string.Empty;
		}


	}
}
