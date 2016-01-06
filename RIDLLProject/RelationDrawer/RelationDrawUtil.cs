using UnityEngine;
using UnityEditor;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	public class RelationDrawUtil
	{
		internal static void DrawRotatedTexture( Texture2D texture, Vector2 pivot, Vector2 scale, float rotation, Color color )
		{
			var rect = Util.CenterRect( pivot, scale );
			rotation *= Mathf.Rad2Deg;
			GUIUtility.RotateAroundPivot( rotation, pivot );

			var colorBackup = GUI.color;
			GUI.color = color;
			GUI.DrawTexture( rect, texture, ScaleMode.StretchToFill, true );
			GUI.color = colorBackup;

			GUI.matrix = Matrix4x4.identity;
		}

		internal static void DrawRotatedRect( Rect rect, float rotation, Color color )
		{
			GUIUtility.RotateAroundPivot( rotation * Mathf.Rad2Deg, rect.center );
			EditorGUI.DrawRect( rect, color );
			GUI.matrix = Matrix4x4.identity;
		}

		internal static void DrawRotatedRectGL( Rect rect, float rotation, Color color )
		{
			GLMaterials.defaultMat.SetPass( 0 );    // set the material

			GL.Begin( GL.QUADS );
			GL.Color( color );
			foreach ( var vertex in rect.Vertices() )
			{
				var rotated = vertex.RotateAround( rect.center, rotation );
				GL.Vertex( rotated );
			}
			GL.End();
		}
	}
}
