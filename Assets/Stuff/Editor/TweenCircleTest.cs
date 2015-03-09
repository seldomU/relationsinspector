/*
using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;

public class TweenCircleTest : EditorWindow
{
	// inspectable variables
	public Vector2 center;
	public float radius = 50;
	public int numTargets = 10;
	public float updateTargetInterval = 0.1f;
	public float tweenDuration = 0.4f;

	float lastTargetSetTime;
	int lastTargetSet;

	RectObj rectObj;
	Vector2[] targets;

	Editor editor;

	void OnEnable()
	{
		rectObj = new RectObj(new Rect(center.x, center.y, 5, 5));
	}

	void Restart()
	{
		targets = new Vector2[numTargets];
		for (int i = 0; i < numTargets; i++)
		{
			float frac = i / (float)numTargets;
			targets[i] = new Vector2(Mathf.Cos(frac * (2 * Mathf.PI)), Mathf.Sin(frac * (2 * Mathf.PI)));
			targets[i] *= radius;
			targets[i] += center;
		}
		rectObj = new RectObj(new Rect(center.x, center.y, 5, 5));

		lastTargetSetTime = 0;
		lastTargetSet = -1;
	}

	void OnGUI()
	{
		EditorGUI.DrawRect(rectObj.rect, Color.red);

		// draw targets
		if (targets != null)
		{
			for (int i = 0; i < targets.Length; i++)
			{
				EditorGUI.DrawRect(new Rect(targets[i].x, targets[i].y, 2, 2), Color.yellow);
			}
		}


		if (Event.current.type == EventType.mouseDown)
		{
			Restart();
			// tween the rectObj
			//Tweener.MoveRectTo(rectObj, Event.current.mousePosition, 1f);
		}
	}

	void Update()
	{
		if (targets != null && lastTargetSet + 1 < targets.Length)
		{
			double time = EditorApplication.timeSinceStartup;
			if (time >= lastTargetSetTime + updateTargetInterval)
			{
				int targetID = lastTargetSet + 1;
				Tweener.gen.MoveRectTo(rectObj, targets[targetID], tweenDuration);

				lastTargetSetTime = (float)time;
				lastTargetSet = targetID;
			}
		}
		Repaint();
	}

	
	[MenuItem("Window/tweenCircleTest")]
	static void Spawn()
	{
		GetWindow<TweenCircleTest>();
	}
}
*/
