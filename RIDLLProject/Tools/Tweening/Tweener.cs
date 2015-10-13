using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RelationsInspector.Tweening
{
	public static class Tweener
	{
		public static TweenCollection gen = new TweenCollection();	// generic collection

		static Tweener()
		{
			EditorApplication.update += Update;
		}

		static void Update()
		{
			gen.Update((float)EditorApplication.timeSinceStartup);
		}
	}
}
