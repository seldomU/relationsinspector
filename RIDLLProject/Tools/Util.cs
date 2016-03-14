using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public class Util
	{
		/*public static GUIStyle WindowStyle
		{
			get { return EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).window; }
		}*/
		public static string ProjectPath = GetProjectPath();

		public static Rect rectZero = new Rect( 0, 0, 0, 0 );

		public static System.Action IdleAction = () => { };

		public static Rect CenterRect( Vector2 center, float width, float height )
		{
			return new Rect( center.x - width / 2, center.y - height / 2, width, height );
		}

		public static Rect CenterRect( Vector2 center, Vector2 extents )
		{
			return CenterRect( center, extents.x, extents.y );
		}

		public static void DrawBezier( Rect source, Rect destination )
		{
			var start = new Vector2( source.center.x, source.yMax );
			var end = new Vector2( destination.center.x, destination.yMin );
			var tangentOffset = new Vector2( 0, 25 );
			var startTangent = start + tangentOffset;
			var endTangent = end - tangentOffset;
			Handles.DrawBezier( start, end, startTangent, endTangent, Color.white, null, 4 );
		}

		// doesn't produce a perfect rect, coords can be off by 1
		public static void DrawRectOutline( Rect rect, Color color )
		{
			var backupColor = Handles.color;
			Handles.color = color;

			var corners = rect.Vertices();
			for ( int i = 0; i < corners.Length; i++ )
				Handles.DrawLine( corners[ i ], corners[ ( i + 1 ) % corners.Length ] );

			Handles.color = backupColor;
		}

		public static Rect GetBounds( IEnumerable<Rect> rects )
		{
			if ( !rects.Any() )
				return rectZero;

			float xMin = rects.Select( r => r.xMin ).Min();
			float xMax = rects.Select( r => r.xMax ).Max();
			float yMin = rects.Select( r => r.yMin ).Min();
			float yMax = rects.Select( r => r.yMax ).Max();

			return new Rect( xMin, yMin, xMax - xMin, yMax - yMin );
		}

		public static Rect GetBounds( Rect r1, Rect r2 )
		{
			float xMin = Mathf.Min( r1.xMin, r2.xMin );
			float yMin = Mathf.Min( r1.yMin, r2.yMin );
			float xMax = Mathf.Max( r1.xMax, r2.xMax );
			float yMax = Mathf.Max( r1.yMax, r2.yMax );
			return new Rect( xMin, yMin, xMax - xMin, yMax - yMin );
		}

		public static Rect GetBounds( IEnumerable<Vector2> points )
		{
			if ( !points.Any() )
				return rectZero;

			float xMin = points.Select( p => p.x ).Min();
			float xMax = points.Select( p => p.x ).Max();
			float yMin = points.Select( p => p.y ).Min();
			float yMax = points.Select( p => p.y ).Max();

			return new Rect( xMin, yMin, xMax - xMin, yMax - yMin );
		}

		public static float MinScale( Vector2 sourceExtents, Vector2 targetExtents )
		{
			float xScale = sourceExtents.x == 0 ? 0 : targetExtents.x / sourceExtents.x;
			float yScale = sourceExtents.y == 0 ? 0 : targetExtents.y / sourceExtents.y;
			return xScale < yScale ? xScale : yScale;
		}

		public static void FadeRect( Rect rect, Color color )
		{
			color.a = 0.5f;
			EditorGUI.DrawRect( rect, color );
		}

		public static Vector2 GetPositionOnCircle( float angle )
		{
			return new Vector2( Mathf.Cos( angle ), Mathf.Sin( angle ) );
		}

		public static float GetAngle( Vector2 position )
		{
			return Mathf.PI / 2 - Mathf.Atan2( position.x, position.y ); // Vector2.Angle(position, Vector2.right);	//
		}

		internal static float NormalizeAngle( float angle )
		{
			angle = angle % ( 2 * Mathf.PI );
			if ( angle < 0 )
				angle += 2 * Mathf.PI;
			return angle;
		}

		internal static T LoadAsset<T>( string path ) where T : Object
		{
			return (T) AssetDatabase.LoadAssetAtPath( path, typeof( T ) );
		}

		internal static T LoadGUIDAsset<T>( string guid ) where T : Object
		{
			string path = AssetDatabase.GUIDToAssetPath( guid );
			if ( string.IsNullOrEmpty( path ) )
				return null;

			return LoadAsset<T>( path );
		}

		internal static string AbsolutePathToAssetPath( string path )
		{
			// asset paths use forward slashes as directory seperators
			path = path.Replace( "\\", "/" );
			var assetsPath = Application.dataPath;
			if ( path == null || !path.StartsWith( assetsPath ) )
				throw new System.ArgumentException( "path is " + path + " assetsPath is " + assetsPath );

			return "Assets" + path.Substring( assetsPath.Length );
		}

		// asset path -> a full system path
		internal static string AssetToSystemPath( string assetPath )
		{
			if ( assetPath == null || !assetPath.StartsWith( "Assets" ) )
				throw new System.ArgumentException( "path is " + assetPath );

			string fixedSeperatorPath = assetPath.Replace( "/", System.IO.Path.DirectorySeparatorChar.ToString() );

			return System.IO.Path.Combine( GetProjectPath(), fixedSeperatorPath );
		}

		// returns the path of the directory that contains the assets folder
		internal static string GetProjectPath()
		{
			string fixedSeperatorDataPath = Application.dataPath.Replace( "/", System.IO.Path.DirectorySeparatorChar.ToString() );
			return System.IO.Directory.GetParent( fixedSeperatorDataPath ).FullName;
		}

		internal static void ForceCreateAsset( Object obj, string path )
		{
			System.IO.Directory.CreateDirectory( System.IO.Path.GetDirectoryName( path ) );
			AssetDatabase.CreateAsset( obj, path );
		}

		// load asset at path, if it exists. if not, create it
		internal static T LoadOrCreate<T>( string assetPath, System.Func<T> create ) where T : Object
		{
			// if the asset file exists, load it and return the object
			if ( System.IO.File.Exists( assetPath ) )
				return LoadAsset<T>( assetPath );

			// the asset file does not exist. create the object, and from it create the asset
			var obj = create();
			AssetDatabase.CreateAsset( obj, assetPath );
			AssetDatabase.SaveAssets();
			return obj;
		}

		internal static void DrawCenteredInspector( Editor editor, int width = 350 )
		{
			// vertical space on top
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			// horizontal space left
			GUILayout.FlexibleSpace();

			// content
			GUILayout.BeginVertical( GUI.skin.box, GUILayout.Width( width ) );

			GUILayout.Label( editor.name, EditorStyles.boldLabel );
			editor.OnInspectorGUI();

			GUILayout.EndVertical();

			// horizontal space right
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			// vertical space bottom
			GUILayout.FlexibleSpace();
		}

		public static IEnumerable<int> Shuffle( int numItems )  // fisher-yates
		{
			var moved = new Dictionary<int, int>(); // mapping original to current position
			for ( int i = numItems - 1; i > 0; i-- )
			{
				int atI = moved.ContainsKey( i ) ? moved[ i ] : i;
				int other = Random.Range( 0, i + 1 );
				int atOther = moved.ContainsKey( other ) ? moved[ other ] : other;
				moved[ other ] = atI;

				yield return atOther;
			}
		}

		public static string GetSelectionDirectoryPath()
		{
			string path = AssetDatabase.GetAssetPath( Selection.activeObject );
			if ( path == "" )
				return "Assets";

			if ( System.IO.Path.GetExtension( path ) != "" )
				path = path.Replace( System.IO.Path.GetFileName( AssetDatabase.GetAssetPath( Selection.activeObject ) ), "" );

			return path;
		}

		// find both null-refs and unity missing refs of destroyed objects
		public static bool IsBadRef<T>( T objRef )
		{
			return objRef == null || objRef.Equals( null );
		}
	}
}
