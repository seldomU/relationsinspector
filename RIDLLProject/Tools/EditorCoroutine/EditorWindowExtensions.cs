using System.Collections;
using UnityEditor;

namespace EditorCoroutines
{
	public static class EditorWindowExtensions
	{
		public static EditorCoroutine StartCoroutine(this EditorWindow window, IEnumerator enumerator )
		{
			return EditorCoroutineManager.Singleton.StartCoroutine(enumerator);
		}
	}
}
