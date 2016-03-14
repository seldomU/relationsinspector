using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public class WelcomeWindow : EditorWindow
	{
		interface IContent { void Draw(); }

		struct ContentItem
		{
			public string title;
			public System.Func<GUIStyle> getStyle;
			public System.Action onClick;
		}

		static Dictionary<string, GUIStyle> stylesCache = new Dictionary<string, GUIStyle>();

		static ContentItem[] headlineContent = new ContentItem[]
		{
		new ContentItem() { title = "Documentation", getStyle = () => GetButtonStyle("Documentation" ), onClick = () => Application.OpenURL("") },
		new ContentItem() { title = "Discussion", getStyle = () => GetButtonStyle("Discussion" ), onClick = () => Application.OpenURL("") },
		new ContentItem() { title = "Videos", getStyle = () => GetButtonStyle("Youtube" ), onClick = () => Application.OpenURL("") },
		new ContentItem() { title = "Twitter", getStyle = () => GetButtonStyle("Twitter" ), onClick = () => Application.OpenURL("") },
		new ContentItem() { title = "Buy full version", getStyle = () => GetButtonStyle("Buy" ), onClick = () => Application.OpenURL("") }
		};

		const string windowTitle = "Welcome to RelationsInspector";

		static Texture IntegrationIcon = GetTexture( "Integration" );
		static Texture RITitleTextImage = GetTexture( "RITitleText" );

		Vector2 scrollPosition;
		static Vector2 defaultWindowSize = new Vector2( 400, 600 );

		public GUIStyle listHeaderStyle;
		public GUIStyle mainTitleStyle;
		public GUIStyle tooltipStyle;
		public GUIStyle packageTitleStyle;
		public GUIStyle packageDescriptionStyle;
		public GUIStyle packageInstallToggleStyle;
		public GUIStyle versionLabelStyle;

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
		//public float packageContentSpaceLeft = 20;

		RIBackendPackageInfo[] packageInfos;

		void Update()
		{
			Repaint();
		}

		void InitStyles()
		{
			if ( listHeaderStyle == null )
			{
				listHeaderStyle = new GUIStyle( GUI.skin.label );
				listHeaderStyle.fontSize = 17;
				listHeaderStyle.contentOffset = new Vector2( -15, 5 );
				listHeaderStyle.padding.bottom = 15;
			}

			if ( mainTitleStyle == null )
			{
				mainTitleStyle = new GUIStyle( GUI.skin.label );
				mainTitleStyle.fontSize = 26;
				mainTitleStyle.alignment = TextAnchor.MiddleCenter;
			}

			if ( tooltipStyle == null )
			{
				tooltipStyle = new GUIStyle( GUI.skin.label );
				tooltipStyle.alignment = TextAnchor.MiddleCenter;
			}

			if ( packageTitleStyle == null )
			{
				packageTitleStyle = new GUIStyle( GUI.skin.label );
				packageTitleStyle.fontStyle = FontStyle.Bold;
			}

			if ( packageDescriptionStyle == null )
			{
				packageDescriptionStyle = new GUIStyle( GUI.skin.label );
				packageDescriptionStyle.padding.left = 81;
			}

			if ( packageInstallToggleStyle == null )
			{
				packageInstallToggleStyle = new GUIStyle( GUI.skin.toggle );
				packageInstallToggleStyle.margin.left = 40;
				packageInstallToggleStyle.margin.right = 25;
			}

			if ( versionLabelStyle == null )
			{
				versionLabelStyle = new GUIStyle( GUI.skin.label );
				versionLabelStyle.margin.left = 288;
			}
		}

		void OnEnable()
		{
			string[] packageFolderAssetPaths = Directory.GetFiles( Util.AssetToSystemPath( ProjectSettings.PackagesPath ), "*.asset" );
			Debug.Log( ProjectSettings.PackagesPath );
			Debug.Log( Util.AssetToSystemPath( ProjectSettings.PackagesPath ) );
			Debug.Log( packageFolderAssetPaths.ToDelimitedString() );
			packageInfos = packageFolderAssetPaths
				.Select( path => Util.LoadAsset<RIBackendPackageInfo>( Util.AbsolutePathToAssetPath( path ) ) )
				.Where(asset => asset != null ) // might not be of type RIBackendPackageInfo
				.ToArray();
		}

		void OnGUI()
		{
			InitStyles();

			GUILayout.Space( toolbarToHeaderSpace );

			// draw Title
			GUILayout.BeginHorizontal();
			GUILayout.Space( titleSpaceLeft );  //( EditorGUIUtility.currentViewWidth - RITitleTextImage.width ) / 2;
			GUI.DrawTexture( ReserveRect( RITitleTextImage.width, RITitleTextImage.height ), RITitleTextImage );
			GUILayout.EndHorizontal();

			string version = GetType().Assembly.GetName().Version.ToString();
#if RIDEMO
            version += "demo";
#endif
			GUILayout.Label( version, versionLabelStyle );

			GUILayout.Space( headerToListSpace );


			scrollPosition = GUILayout.BeginScrollView( scrollPosition );
			{
				// draw integrations header
				GUILayout.BeginHorizontal();
				GUILayout.Space( integrationIconHorSpacing );
				var iconRect = ReserveRect( new Vector2( listIconSize, listIconSize ) );
				GUILayout.Space( integrationIconHorSpacing );
				GUI.DrawTexture( iconRect, IntegrationIcon );
				//if ( GUI.Button( iconRect, GUIContent.none, GUIStyle.none ) )
				//	item.onClick.Invoke();
				GUILayout.Label( "Integration packs", listHeaderStyle );
				GUILayout.EndHorizontal();

				// integrations
				foreach ( var pInfo in packageInfos )
				{
					DrawPackageContent( pInfo );
				}
			}
			GUILayout.EndScrollView();

			GUILayout.FlexibleSpace();

			// link toolbar
			GUILayout.BeginHorizontal();
			foreach ( var item in headlineContent )
			{
				GUILayout.Space( toolbarItemHorSpacing );
				var rect = ReserveRect( new Vector2( toolbarIconSize, toolbarIconSize ) );
				//GUI.DrawTexture( rect, item.icon );
				if ( GUI.Button( rect, new GUIContent( "", null, item.title ), item.getStyle() ) )
					item.onClick.Invoke();
			}
			GUILayout.EndHorizontal();

			// tooltip
			GUILayout.Label( GUI.tooltip, tooltipStyle );

			GUILayout.Space( toolbarTopSpace );
		}

		void DrawPackageContent( RIBackendPackageInfo pInfo )
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

		#region utility

		string GetFullPackagePath( string packageName )
		{
			string directoryPath = Util.AssetToSystemPath( ProjectSettings.PackagesPath );	//Path.Combine( Application.dataPath, ProjectSettings.PackagesPath );
			string filePath = Path.Combine( directoryPath, packageName + ".unitypackage" );
			Debug.Log( filePath );
			return filePath;
		}

		bool PackageIsInstalled( RIBackendPackageInfo packageInfo )
		{
			string packageInstallPath = Util.AssetToSystemPath( GetPackageInstallPath( packageInfo ) );
			return System.IO.Directory.Exists( packageInstallPath );
		}

		string GetPackageInstallPath( RIBackendPackageInfo packageInfo )
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

		static GUIStyle GetButtonStyle( string name )
		{
			string id = "buttonStyle" + name;

			if ( stylesCache.ContainsKey( id ) )
				return stylesCache[ id ];

			// not cached. create it
			var style = new GUIStyle();
			style.normal.background = GetTexture( name );
			style.hover.background = GetTexture( name + "_hover" );
			//style.onHover.background = GetTexture( "Twitter" ); //name + "_hover" );
			stylesCache[ id ] = style;
			return style;
		}

		#endregion

		//[MenuItem( "Window/RelationsInspector/Welcome" )]
		internal static void Spawn()
		{
			var window = GetWindow<WelcomeWindow>( true, windowTitle, true );
			window.position = new Rect( 100, 100, defaultWindowSize.x, defaultWindowSize.y );
		}

		/*
		[MenuItem( "Window/Create x" )]
		static void CreateX()
		{
			Selection.activeObject = GetWindow<WelcomeWindow>();
			
			//var packageInfo = ScriptableObject.CreateInstance<RIBackendPackageInfo>();
			//AssetDatabase.CreateAsset( packageInfo, "Assets/info.asset" );
			//AssetDatabase.SaveAssets();
			
		}*/
	}
}
