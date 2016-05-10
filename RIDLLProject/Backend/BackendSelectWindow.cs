using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RelationsInspector
{
	class BackendSelectWindow : EditorWindow
	{
		// variables supplied by the window creator
		public Type[] backendTypes;
		public Type selectedBackendType;
		public Action<Type> OnSelectBackend;

		// styling
		const float TitleWidth = 224f;
		const float VersionWidth = 45f;
		const float IconSize = 16;

		GUIStyle scrollBoxStyle;
		GUIStyle backendTitleStyle;
		GUIStyle headerStyle;
		Texture2D helpIcon;

		// state
		Vector2 scrollPos;
		Dictionary<Type, Texture2D> backendIcons;

		void OnEnable()
		{
			helpIcon = EditorGUIUtility.FindTexture( "_Help" );
			scrollBoxStyle = new GUIStyle( GUI.skin.box );
			scrollBoxStyle.margin = new RectOffset( 1, 0, 1, 1 );

			backendTitleStyle = new GUIStyle( GUI.skin.label );
			backendTitleStyle.richText = true;

			headerStyle = new GUIStyle(GUI.skin.label);
			headerStyle.alignment = TextAnchor.MiddleCenter;
			headerStyle.fontSize = 13;
			headerStyle.fontStyle = FontStyle.Bold;
			headerStyle.margin = new RectOffset( 6,6,6,6 );
		}

		void OnGUI()
		{
			if ( backendTypes == null || OnSelectBackend == null )
			{
				Log.Message( "BackendSelectWindow missing argument" );
				Close();
			}

			if ( backendIcons == null )
			{
				backendIcons = backendTypes
					.Select( t => new { type = t, icon = LoadIcon( t ) } )
					//.Where( tuple => tuple.icon != null )
					.ToDictionary( tuple => tuple.type, tuple => tuple.icon );
			}

			GUILayout.BeginHorizontal( scrollBoxStyle );
			GUILayout.Label("Select graph type", headerStyle);
			GUILayout.EndHorizontal();

			// a scrollview with one row for each backend type
			scrollPos = GUILayout.BeginScrollView( scrollPos, scrollBoxStyle );
			{
				foreach ( var t in backendTypes )
				{
					GUILayout.BeginHorizontal();
					{
						DrawTypeRow( t );
					}
					GUILayout.EndHorizontal();
				}
			}
			GUILayout.EndScrollView();
		}

		void DrawTypeRow( Type t )
		{
			// icon
			var iconRect = GUILayoutUtility.GetRect( IconSize, IconSize );
			var icon = backendIcons[ t ];
			if( icon != null )
				GUI.DrawTexture( iconRect, icon, ScaleMode.StretchToFill, true );

			// title (if clicked, return the type and close the window)
			string titleText = BackendTypeUtil.GetTitle( t );
			if ( t == selectedBackendType )
			{
				string color = EditorGUIUtility.isProSkin ? "white" : "grey";
				titleText = string.Format("<color=\"{0}\">{1}</color>", color, titleText);
			}

			var titleContent = new GUIContent( titleText, null, BackendTypeUtil.GetDescription( t ) );
			bool select = GUILayout.Button( titleContent, backendTitleStyle, GUILayout.Width(TitleWidth) );
			if ( select )
			{
				OnSelectBackend( t );
				Close();
			}

			// version
			string version = BackendTypeUtil.GetVersion( t );
			if ( string.IsNullOrEmpty( version ) )
				version = string.Empty;

			GUILayout.Label( version, GUILayout.Width( VersionWidth ) );

			// doc URL (if clicked: open)
			string docURL = BackendTypeUtil.GetDocumentationURL( t );
			GUI.enabled = !string.IsNullOrEmpty( docURL );
			var docContent = new GUIContent( helpIcon, "Open documentation website" );
			bool openDoc = GUILayout.Button( docContent, GUIStyle.none, GUILayout.ExpandWidth( false ) );
			if ( openDoc )
				Application.OpenURL( docURL );
			GUI.enabled = true;
			GUILayout.FlexibleSpace();
		}

		static Texture2D LoadIcon( Type backendType )
		{
			string iconPath = BackendTypeUtil.GetIconPath( backendType );
			if ( string.IsNullOrEmpty( iconPath ) )
				return null;
			return Util.LoadAsset<Texture2D>( iconPath );
		}
	}
}
