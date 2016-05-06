using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
	class GUIUtil
	{
		public static string GetPrefsKey( string localKey )
		{
			// create a name that should be a unique EditorPrefs key
			// by prefixing the project name (namespacing)
			return System.IO.Path.Combine( ProjectSettings.EditorPrefsProjectPrefix, localKey );
		}

		public static void SetPrefsInt( string localKey, int value )
		{
			EditorPrefs.SetInt( GetPrefsKey( localKey ), value );
		}

		public static int GetPrefsInt( string localKey )
		{
			return EditorPrefs.GetInt( GetPrefsKey( localKey ) );
		}

		public static int GetPrefsInt( string localKey, int defaultValue )
		{
			string prefsKey = GetPrefsKey( localKey );
			if ( !EditorPrefs.HasKey( prefsKey ) )
				return defaultValue;

			return EditorPrefs.GetInt( prefsKey );
		}

		public static void SetPrefsString( string localKey, string value )
		{
			EditorPrefs.SetString( GetPrefsKey( localKey ), value );
		}

		public static string GetPrefsString( string localKey )
		{
			return EditorPrefs.GetString( GetPrefsKey( localKey ) );
		}

		public static string GetPrefsString( string localKey, string defaultValue )
		{
			string prefsKey = GetPrefsKey( localKey );
			if ( !EditorPrefs.HasKey( prefsKey ) )
				return defaultValue;

			return EditorPrefs.GetString( prefsKey );
		}

		public static void SetPrefsBackendType( string localKey, Type value )
		{
			SetPrefsString( localKey, value.ToString() );
		}

		public static Type GetPrefsBackendType( string localKey )
		{
			string typeName = GetPrefsString( localKey );
			if ( string.IsNullOrEmpty( typeName ) )
				return null;

			return BackendTypeUtil.backendTypes.SingleOrDefault( t => t.ToString() == typeName );  //.Split( '`' )[ 0 ]
		}

		/// Creates a toolbar that is filled from an Enum. (CC-BY-SA, from http://wiki.unity3d.com/index.php?title=EditorGUIExtension)
		public static Enum EnumToolbar( string prefixLabel, Enum selected, GUIStyle style, params GUILayoutOption[] options )
		{
			Func<Enum, GUIContent> getContent = ( item ) => new GUIContent( Enum.GetName( item.GetType(), item ) );
			return EnumToolbar( prefixLabel, selected, getContent, style, options );
		}

		/// Creates a toolbar that is filled from an Enum. (CC-BY-SA, from http://wiki.unity3d.com/index.php?title=EditorGUIExtension)
		public static Enum EnumToolbar( string prefixLabel, Enum selected, Func<Enum, GUIContent> getContent, GUIStyle style, params GUILayoutOption[] options )
		{
			var values = Enum.GetValues( selected.GetType() );
			var contents = values.OfType<Enum>().Select( val => getContent( val ) ).ToArray();

			int selected_index = 0;
			while ( selected_index < values.Length )
			{
				if ( selected.ToString() == values.GetValue( selected_index ).ToString() )
					break;
				selected_index++;
			}

			GUILayout.BeginHorizontal();
			if ( !string.IsNullOrEmpty( prefixLabel ) )
			{
				GUILayout.Label( prefixLabel );
				GUILayout.Space( 10 );
			}

			selected_index = GUILayout.Toolbar( selected_index, contents, style, options );
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			return (Enum) values.GetValue( selected_index );
		}

		public static Rect GUIToScreenRect( Rect rect )
		{
			Vector2 origin = GUIUtility.GUIToScreenPoint( new Vector2( rect.x, rect.y ) );
			rect.x = origin.x;
			rect.y = origin.y;
			return rect;
		}
	}
}
