
using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;
using RelationsInspector.Tweening;

public class TweenerTest : EditorWindow
{
    public Vector2 center = new Vector2( 100, 100 );
    public float radius = 20;
    public Vector2 extents = new Vector2( 100, 200 );
    public float cornerSize = 0;
    public float angle = 90;

    void OnEnable(){}

	void OnGUI()
	{
        if ( GUILayout.Button( "inspect" ) )
        {
            Selection.activeObject = this;
        }

        if ( GUILayout.Button( "blow up" ) )
        {
            //angle = 0;
            var maxCornerSize = Mathf.Min( extents.x, extents.y ) * 0.5f;
            TransitionCornerSize( maxCornerSize, 1);
        }
        if ( GUILayout.Button( "shrink" ) )
        {
            //angle = 0;
            TransitionCornerSize( 0, 1 );
        }

        DrawRoundedRect( Util.CenterRect( center, extents ), cornerSize, Color.red, Color.black );
        
		if (Event.current.type == EventType.mouseDown)
		{
            var newCenter = Event.current.mousePosition;    //new Vector2( Random.Range( 50, 150 ), Random.Range( 50, 150 ) );
            Tweener.gen.Add( new Tween<Vector2>( x => center = x, 1, TweenUtil.Vector2_2( center, newCenter, TwoValueEasing.Linear ) ) );
            Debug.Log("moving");
		}
	}

    void TransitionCornerSize(float target, float duration)
    {
        Tweener.gen.Add( new Tween<float>( x => cornerSize = x, duration, TweenUtil.Float2( cornerSize, target, TwoValueEasing.Linear ) ) );
        //Tweener.gen.Add( new Tween<float>( x => angle = x, duration, TweenUtil.Float2( angle, 90, TwoValueEasing.Linear ) ) );

    }

    void DrawRoundedRect( Rect rect, float cornerSize, Color innerColor, Color outlineColor )
    {
        float innerXmin = rect.xMin + cornerSize;
        float innerYmin = rect.yMin + cornerSize;
        float innerXmax = rect.xMax - cornerSize;
        float innerYmax = rect.yMax - cornerSize;

        // draw inner rects
        var innerRect1 = new Rect( innerXmin, rect.yMin, rect.width - 2 * cornerSize, rect.height );
        var innerRect2 = new Rect( rect.xMin, innerYmin, rect.width, rect.height - 2 * cornerSize );
        EditorGUI.DrawRect( innerRect1, innerColor );
        EditorGUI.DrawRect( innerRect2, innerColor );

        var arcCenters = new[] {
            new Vector2( innerXmin, innerYmin),
            new Vector2( innerXmin, innerYmax),
            new Vector2( innerXmax, innerYmax),
            new Vector2( innerXmax, innerYmin)
        };

        var arcStarts = new[] {
            Vector3.left,
            Vector3.up,
            Vector3.right,
            Vector3.down
        };

        Handles.color = innerColor;
        for ( int i = 0; i < 4; i++ )
            Handles.DrawSolidArc( arcCenters[ i ], Vector3.forward, arcStarts[ i ], 90, cornerSize );
        Handles.color = Color.white;
    }

    void Update()
	{
		Repaint();
	}

	[MenuItem("Window/tween/test")]
	static void Spawn()
	{
		GetWindow<TweenerTest>();
	}
}

