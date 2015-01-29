/*
using NUnit.Framework;
using System.Linq;
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using RelationsInspector;

[TestFixture]
public class ReflectionTests
{
	[Test]
	public void MyFistTest()
    {
		var implementations = GetType().Assembly.GetTypes().Where( t => ReflectionUtil.ImplementsGenericInterface(t, typeof(IGraphBackend<,>) ) );

		var dict = new Dictionary<Type,Type>();
		foreach(var type in implementations)
			dict[type] = ReflectionUtil.GetGenericInterface(type, typeof(IGraphBackend<,>));


		
		Debug.Log("implementations");
		foreach (var pair in dict)
			Debug.Log(pair.Key +" "+ pair.Value);

		Assert.IsNotEmpty(implementations);
        // assert (verify) that healthAmount was updated.
		//Assert.AreEqual(40f, health.healthAmount);
    }

	[Test]
	public void WidgetCastGeneric()
	{
		bool isDrawer = DeserializedConfig.IsEntityDrawerType(typeof(ObjectEntityWidget<>), typeof(UnityEngine.Object));
		Assert.IsTrue(isDrawer);
	}

	[Test]
	public void WidgetCastNonGeneric()
	{
		//bool isDrawer = DeserializedConfig.IsDrawerType(typeof(FixedObjectVertexWidget), typeof(UnityEngine.Object));
		//Assert.IsTrue(isDrawer);
	}

	[Test]
	public void APItest()
	{		 
		RelationsInspectorAPI api = EditorWindow.GetWindow<RelationsInspectorWindow>();
		api.ClearWindow();
	}
}
*/
