using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;

public class TweenerTest : EditorWindow
{
	RectObj rectObj = new RectObj(new Rect(20, 30, 30, 30));

	void OnEnable(){}

	void OnGUI()
	{
		EditorGUI.DrawRect(rectObj.rect, Color.red);

		if (Event.current.type == EventType.mouseDown)
		{
			// tween the rectObj
			Tweener.gen.MoveRectTo(rectObj, Event.current.mousePosition, 1f);
			Debug.Log("moving");
		}
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

	[MenuItem("Window/tween/circleTest")]
	static void SpawnCircleTest()
	{
		GetWindow<TweenCircleTest>();
	}
}
