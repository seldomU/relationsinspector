using UnityEditor;
using RelationsInspector;

public class RelationsInspectorMenuItem
{
	[MenuItem("Window/Relations Inspector")]
	static void SpawnWindow()
	{
		EditorWindow.GetWindow<RelationsInspectorWindow>("Relations", typeof(SceneView));
	}
}
