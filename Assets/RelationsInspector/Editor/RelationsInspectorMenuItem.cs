using UnityEditor;
using RelationsInspector;

public class RelationsInspectorMenuItem
{
	[MenuItem("Window/RelationsInspector")]
	static void SpawnWindow()
	{
		EditorWindow.GetWindow<RelationsInspectorWindow>("Relations", typeof(SceneView));
	}
}
