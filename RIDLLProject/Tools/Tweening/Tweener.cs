using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;

namespace RelationsInspector.Tween
{
	public interface ITween
	{
		void Update(float time);
		bool IsExpired(float time);
		void Finish();
	}
	
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
