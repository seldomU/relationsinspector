using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace RelationsInspector
{
	[InitializeOnLoad]
	public class WelcomeWindow : EditorWindow
	{
		class RIBackendPackageMetaData
		{
			public string title;
			public string description;
			public string packageName;
			public string folderName;
		}

		struct ImageButtonContent
		{
			public string title;
			public string bStTexName;   // button style's base texture name
			public System.Action onClick;
		}

		static RIBackendPackageMetaData[] packageMetadata = new RIBackendPackageMetaData[]
		{
			new RIBackendPackageMetaData()
			{
				title = "PlayMaker FSM communication",
				description =
				"Displays PlayMaker state machines\n" +
				"and the events and variables sent between them.",
				packageName = "PlayMakerFSMCommunication",
				folderName = "PlayMaker"
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Dialogue System",
				description =
				"Displays conversations\n" +
				"and the references between them.",
				packageName = "DialogueSystem",
				folderName = "DialogueSystem"
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Inventory Pro items",
				description = 
				"Displays item crafting relations,\n" +
				"blueprints and more.",
				packageName = "InventoryPro",
				folderName = "InventoryPro"
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "InventoryMaster crafting",
				description =
				"Displays item crafting relations\n" +
				"and blueprints.",
				packageName = "InventoryMaster",
				folderName = "InventoryMaster",
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "S-Quest quest editor",
				description =
				"Displays the dependencies between quests\n" +
				"and lets you create and edit them.",
				packageName = "SQuest",
				folderName = "SQuest",
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Asset reference and dependencies",
				description =
					"A graph showing where an asset is referenced.\n" +
					"A graph showing an asset's dependencies.",
				packageName = "AssetReferences",
				folderName = "AssetReferenceBackend",
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "uGUI Events",
				description = 
					"Shows the scene's uGUI event components\n" +
					"and all their listeners.",
				packageName = "UGUIEvents",
				folderName = "UGUIEvents"
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Project view (Example)",
				description =
					"Shows a tree of all project assets,\n" +
					"like Unity's projectview.",
				packageName = "ProjectView",
				folderName = "ProjectView",
			}
			,
			new RIBackendPackageMetaData()
			{
				title = "Type hierarchy",
				description = 
					"Shows inheritance relations\n" +
					"between types.",
				packageName = "TypeHierarchy",
				folderName = "TypeHierarchy"
			}
		};

		static string[] textureNames = new[]
{
			TNameDoc,
			TNameDoc + Hover,
			TNameDiscussion,
			TNameDiscussion + Hover,
			TNameVideo,
			TNameVideo + Hover,
			TNameTwitter,
			TNameTwitter + Hover,
			TNameBuy,
			TNameBuy + Hover,
			TNameIntegration,
			TNameTitleWhite,
			TNameTitleBlack
		};

		const string TNameDoc = "Documentation";
		const string TNameDiscussion = "Discussion";
		const string TNameVideo = "Youtube";
		const string TNameTwitter = "Twitter";
		const string TNameBuy = "Buy";
		const string TNameIntegration = "Integration";
		const string TNameTitleWhite = "RITitleTextWhite";
		const string TNameTitleBlack = "RITitleTextBlack";
		const string Hover = "_hover";

		static ImageButtonContent[] headlineContent = new ImageButtonContent[]
		{
		new ImageButtonContent() { title = "Documentation", bStTexName = TNameDoc, onClick = () => Application.OpenURL( ProjectSettings.DocURL ) }
		,new ImageButtonContent() { title = "Discussion", bStTexName = TNameDiscussion, onClick = () => Application.OpenURL( ProjectSettings.DiscussionURL ) }
		,new ImageButtonContent() { title = "Videos", bStTexName = TNameVideo, onClick = () => Application.OpenURL(ProjectSettings.YoutubeURL) }
		,new ImageButtonContent() { title = "Twitter", bStTexName = TNameTwitter, onClick = () => Application.OpenURL(ProjectSettings.TwitterURL) }
#if RIDEMO
		,new ImageButtonContent() { title = "Buy full version", bStTexName = TNameBuy, onClick = () => UnityEditorInternal.AssetStore.Open( ProjectSettings.StoreDemoURL ) }
#endif
		};

		const string windowTitle = "Welcome to RelationsInspector";
		static Vector2 windowSize = new Vector2( 400, 500 );

		public float titleSpaceLeft = 45;
		public float toolbarIconSize = 32;
		public float toolbarItemHorSpacing = 40;
		public float listIconSize = 32;
		public float toolbarTopSpace = 10;
		public float toolbarToHeaderSpace = 15;
		public float headerToListSpace = 15;
		public float integrationIconHorSpacing = 30;
		public float packageCheckBoxWidth = 1;
		public float packageRowVerticalSpace = 8;

		public GUIStyle listHeaderStyle;
		public GUIStyle mainTitleStyle;
		public GUIStyle tooltipStyle;
		public GUIStyle packageTitleStyle;
		public GUIStyle packageDescriptionStyle;
		public GUIStyle packageInstallToggleStyle;
		public GUIStyle versionLabelStyle;

		Dictionary<string, GUIStyle> buttonStyles = new Dictionary<string, GUIStyle>();
		Dictionary<string, Texture2D> textureByName = new Dictionary<string, Texture2D>();

		Vector2 scrollPosition;
		double nextRepaintTime;
		bool resourcesLoaded;
		bool stylesInitialized;

		public static void SpawnOnFreshInstall()
		{
			string prefsKey = "RIWelcomeWindow" + typeof( WelcomeWindow ).Assembly.GetName().Version.ToString();
			bool freshlyInstalled = EditorPrefs.GetBool( prefsKey, true );
			if ( freshlyInstalled )
			{
				EditorPrefs.SetBool( prefsKey, false );
				SpawnWindow();
			}
		}

		bool LoadAllTextures()
		{
			foreach ( var name in textureNames )
			{
				if ( textureByName.ContainsKey( name ) )
					continue;

				var texture = GetTexture( name );
				if ( texture == null )
				{
					Debug.Log( "missing texture " + name );
					return false;
				}

				textureByName[ name ] = texture;
			}
			return true;
		}

		void Update()
		{
			// repaint 5 times a second to make tooltip more responsive
			if ( EditorApplication.timeSinceStartup > nextRepaintTime )
			{
				nextRepaintTime = EditorApplication.timeSinceStartup + 0.2f;
				Repaint();
			}
		}

		void InitStyles()
		{
			listHeaderStyle = new GUIStyle( GUI.skin.label );
			listHeaderStyle.fontSize = 17;
			listHeaderStyle.contentOffset = new Vector2( -15, 5 );
			listHeaderStyle.padding.bottom = 15;

			mainTitleStyle = new GUIStyle( GUI.skin.label );
			mainTitleStyle.fontSize = 26;
			mainTitleStyle.alignment = TextAnchor.MiddleCenter;

			tooltipStyle = new GUIStyle( GUI.skin.label );
			tooltipStyle.alignment = TextAnchor.MiddleCenter;

			packageTitleStyle = new GUIStyle( GUI.skin.label );
			packageTitleStyle.fontStyle = FontStyle.Bold;

			packageDescriptionStyle = new GUIStyle( GUI.skin.label );
			packageDescriptionStyle.padding.left = 81;

			packageInstallToggleStyle = new GUIStyle( GUI.skin.toggle );
			packageInstallToggleStyle.margin.left = 40;
			packageInstallToggleStyle.margin.right = 25;

			versionLabelStyle = new GUIStyle( GUI.skin.label );
			versionLabelStyle.margin.left = 288;

			foreach ( var x in headlineContent )
			{
				var style = new GUIStyle();
				style.normal.background = textureByName[ x.bStTexName ];
				style.hover.background = textureByName[ x.bStTexName + Hover ];
				buttonStyles[ x.bStTexName ] = style;
			}
		}
	
		void OnEnable()
		{
			resourcesLoaded = false;
			stylesInitialized = false;
		}

		void OnGUI()
		{
			if ( !resourcesLoaded )
			{
				if ( !LoadAllTextures() )
				{
					GUILayout.Label( "Window resources are missing" );
					return;
				}
				resourcesLoaded = true;
			}

			if ( !stylesInitialized )
			{
				InitStyles();
				stylesInitialized = true;
			}

			GUILayout.Space( toolbarToHeaderSpace );

			// draw Title
			GUILayout.BeginHorizontal();
			var titleTexture = EditorGUIUtility.isProSkin ? textureByName[ TNameTitleWhite ] : textureByName[ TNameTitleBlack ];
			GUILayout.Space( ( position.width - titleTexture.width ) / 2 );
			GUI.DrawTexture( ReserveRect( titleTexture.width, titleTexture.height ), titleTexture );
			GUILayout.EndHorizontal();

			string version = GetType().Assembly.GetName().Version.ToString();
#if RIDEMO
			version += " Demo";
#endif
			GUILayout.Label( version, versionLabelStyle );

			GUILayout.Space( headerToListSpace );

			// draw integrations header
			GUILayout.BeginHorizontal();
			GUILayout.Space( integrationIconHorSpacing );
			var iconRect = ReserveRect( new Vector2( listIconSize, listIconSize ) );
			GUILayout.Space( integrationIconHorSpacing );
			GUI.DrawTexture( iconRect, textureByName[TNameIntegration] );

			GUILayout.Label( "Installable Add-ons", listHeaderStyle );
			GUILayout.EndHorizontal();

			scrollPosition = GUILayout.BeginScrollView( scrollPosition );
			{
				// integrations
				foreach ( var pInfo in packageMetadata )
					DrawPackageContent( pInfo );
			}
			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			// link bar
			GUILayout.Space( toolbarTopSpace );
			GUILayout.BeginHorizontal();
			float linkBarSpacing = ( position.width - ( headlineContent.Length * toolbarIconSize ) ) / ( headlineContent.Length + 1 );
			foreach ( var item in headlineContent )
			{
				GUILayout.Space( linkBarSpacing );
				var rect = ReserveRect( new Vector2( toolbarIconSize, toolbarIconSize ) );
				if ( GUI.Button( rect, new GUIContent( "", null, item.title ), buttonStyles[item.bStTexName] ) )
					item.onClick.Invoke();
			}
			GUILayout.EndHorizontal();

			// tooltip
			GUILayout.Label( GUI.tooltip, tooltipStyle );

			GUILayout.Space( toolbarTopSpace );
		}

		void DrawPackageContent( RIBackendPackageMetaData pInfo )
		{
			GUILayout.Space( packageRowVerticalSpace );
			// title and install/uninstall control
			GUILayout.BeginHorizontal();
			{
				EditorGUI.BeginChangeCheck();
				EditorGUIUtility.labelWidth = packageCheckBoxWidth;
				bool install = GUILayout.Toggle( PackageIsInstalled( pInfo ), GUIContent.none, packageInstallToggleStyle );
				if ( EditorGUI.EndChangeCheck() )
				{
					if ( install )
						AssetDatabase.ImportPackage( GetFullPackagePath( pInfo.packageName ), true );
					else
					{
						string installPath = GetPackageInstallPath( pInfo );
						string msg = string.Format( "Delete the folder {0} and its contents?", installPath );
						if ( EditorUtility.DisplayDialog( "Delete folder", msg, "Yes", "No" ) )
							AssetDatabase.DeleteAsset( installPath );
					}
				}
				GUILayout.Label( pInfo.title, packageTitleStyle );
				GUILayout.FlexibleSpace();
			}
			GUILayout.EndHorizontal();

			// description
			GUILayout.Label( pInfo.description, packageDescriptionStyle );
		}

		internal static void SpawnWindow()
		{
			var window = GetWindow<WelcomeWindow>( true, windowTitle, true );
			window.minSize = window.maxSize = windowSize;
			window.position = new Rect( 100, 100, windowSize.x, windowSize.y );
		}

		#region utility

		string GetFullPackagePath( string packageName )
		{
			string directoryPath = Util.AssetToSystemPath( ProjectSettings.PackagesPath );
			string filePath = Path.Combine( directoryPath, packageName + ".unitypackage" );
			return filePath;
		}

		bool PackageIsInstalled( RIBackendPackageMetaData packageInfo )
		{
			string packageInstallPath = Util.AssetToSystemPath( GetPackageInstallPath( packageInfo ) );
			return System.IO.Directory.Exists( packageInstallPath );
		}

		string GetPackageInstallPath( RIBackendPackageMetaData packageInfo )
		{
			return ProjectSettings.BackendInstallPath + packageInfo.folderName;
		}

		bool AssetExists( string assetPath )
		{
			return !string.IsNullOrEmpty( AssetDatabase.AssetPathToGUID( assetPath ) );
		}

		Rect ReserveRect( float size ) { return ReserveRect( new Vector2( size, size ) ); }
		Rect ReserveRect( float x, float y ) { return ReserveRect( new Vector2( x, y ) ); }

		Rect ReserveRect( Vector2 extents )
		{
			return GUILayoutUtility.GetRect( 0, 0, GUILayout.Width( extents.x ), GUILayout.Height( extents.y ) );
		}

		static Texture2D GetTexture( string name )
		{
			string texturePath = Path.Combine( ProjectSettings.WelcomeWindowResourcePath, name + ".png" );
			return Util.LoadAsset<Texture2D>( texturePath );
		}

#endregion
	}
}
