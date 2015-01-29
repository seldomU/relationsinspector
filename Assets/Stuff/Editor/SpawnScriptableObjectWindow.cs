using UnityEngine;
using UnityEditor;
using System.Collections;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

public class SpawnScriptableObjectWindow : EditorWindow
{
	Vector2 scrollPos;

	Assembly gameAssembly, editorAssembly;

	void OnEnable()
	{
		gameAssembly = GetAssemblyByName("Assembly-CSharp");
		editorAssembly = GetAssemblyByName("Assembly-CSharp-Editor");
	}

	void OnGUI()
	{
		scrollPos = GUILayout.BeginScrollView(scrollPos);

		ShowAssemblyTypes(gameAssembly);
		ShowAssemblyTypes(editorAssembly);

		GUILayout.EndScrollView();
	}

	// draw instanciation controls for all scriptable object types of the assembly
	void ShowAssemblyTypes(Assembly assembly)
	{
		if (assembly == null)
			return;

		GUILayout.Space(10);
		GUILayout.Label(assembly.GetName().Name, EditorStyles.boldLabel);

		// find all scriptable object classes in the assembly
		Predicate<Type> IsSOsubclass = t => 
			t.IsSubclassOf(typeof(ScriptableObject)) && 
			!t.IsSubclassOf(typeof(EditorWindow)) &&
			!t.IsAbstract;

		var types = assembly.GetTypes().Where( t => IsSOsubclass(t) );
		var groupedTypes = types.GroupBy(t => t.Namespace);

		foreach (var group in groupedTypes)
		{
			// namespace label
			EditorGUILayout.Space();
			GUILayout.Label(group.Key, EditorStyles.boldLabel);
			
			// type buttons
			foreach (var type in group)
				if (GUILayout.Button(type.Name, GUILayout.ExpandWidth(false)))
					InstanciateAndSaveSO(type);
		}
	}

	void InstanciateAndSaveSO(System.Type t)
	{
		var instance = ScriptableObject.CreateInstance(t);
		AssetDatabase.CreateAsset(instance, CreateAssetPath(t) );
		AssetDatabase.SaveAssets();
	}

	// generate an asset path from the type name and the selected project path
	string CreateAssetPath(System.Type type)
	{
		return AssetDatabase.GenerateUniqueAssetPath( GetSelectionDirectoryPath() + "/New " + type.ToString() + ".asset");
	}

	public static string GetSelectionDirectoryPath()
	{
		string path = AssetDatabase.GetAssetPath(Selection.activeObject);
		if (path == "")
			return "Assets";

		if (System.IO.Path.GetExtension(path) != "")
			path = path.Replace(System.IO.Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");

		return path;
	}

	// map assembly name to a loaded assembly
	Assembly GetAssemblyByName(string name)
	{
		return System.AppDomain.CurrentDomain.GetAssemblies().
			   SingleOrDefault(assembly => assembly.GetName().Name == name);
	}

	[MenuItem("Window/spawn scriptable object")]
	public static void Spawn()
	{
		GetWindow<SpawnScriptableObjectWindow>();
	}
}
