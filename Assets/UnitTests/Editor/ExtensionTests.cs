using UnityEngine;
using System.Collections;
using NUnit.Framework;
using RelationsInspector.Extensions;
using System.Collections.Generic;
using RelationsInspector;
using RelationsInspector.Backend.DialogGraph;

[TestFixture]
public class ExtensionTests
{
	[Test]
	public void TestRemove()
	{
		var dict = new Dictionary<Object, int>();

		var a = ScriptableObject.CreateInstance<DialogItem>();
		var b = ScriptableObject.CreateInstance<DialogItem>();
		var c = ScriptableObject.CreateInstance<DialogItem>();
		dict[a] = 1;
		dict[b] = 2;
		dict[c] = 3;

		Object.DestroyImmediate(b);

		Debug.Log("before " + dict.ToDelimitedString());
		dict.RemoveWhere(pair => pair.Key == null);
		Debug.Log("after " + dict.ToDelimitedString());
	}
}
