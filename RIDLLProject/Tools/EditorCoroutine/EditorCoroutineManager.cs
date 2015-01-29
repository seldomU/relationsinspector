using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorCoroutineManager
{
	#region singleton
	private static EditorCoroutineManager instance;
	public static EditorCoroutineManager Singleton
	{
		get
		{
			if (instance == null)
				instance = new EditorCoroutineManager();
			return instance;
		}
	}
	#endregion

	HashSet<EditorCoroutine> coroutines, incommingCoroutines;

	private EditorCoroutineManager()
	{
		EditorApplication.update += OnEditorUpdate;
		coroutines = new HashSet<EditorCoroutine>();
		incommingCoroutines = new HashSet<EditorCoroutine>();
	}

	void OnEditorUpdate()
	{
		// add incomming coroutines to the set
		coroutines.UnionWith( incommingCoroutines );
		incommingCoroutines.Clear();

		// update the set
		foreach (var cr in coroutines)
			cr.Update();

		// remove the ones that are finished
		coroutines.RemoveWhere( cr => cr.Finished );
	}

	public EditorCoroutine StartCoroutine(IEnumerator enumerator)
	{
		var coroutine = new EditorCoroutine(enumerator);
		incommingCoroutines.Add(coroutine);
		return coroutine;
	}

	public void StopCoroutines()
	{
		coroutines.Clear();
		incommingCoroutines.Clear();
	}

	public void StopCoroutine(EditorCoroutine coroutine)
	{
		coroutines.Remove( coroutine );
		incommingCoroutines.Remove( coroutine );
	}
}
