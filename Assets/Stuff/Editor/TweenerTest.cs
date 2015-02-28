using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;

public class TestWindow : EditorWindow
{
	RectObj rectObj = new RectObj(new Rect(20, 30, 30, 30));

	void OnEnable(){}

	void OnGUI()
	{
		EditorGUI.DrawRect(rectObj.rect, Color.red);

		if (Event.current.type == EventType.mouseDown)
		{
			// tween the rectObj
			Tweener.MoveRectTo(rectObj, Event.current.mousePosition, 1f);
		}
	}

	void Update()
	{
		Repaint();
	}

	[MenuItem("Window/test")]
	static void Spawn()
	{
		GetWindow<TestWindow>();
	}
}
