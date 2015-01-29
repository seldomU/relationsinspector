using UnityEngine;
using UnityEditor;

public class DebugModeShortcuts
{
	[MenuItem("DebugMode/Set debug mode")]
	static void SetDebugMode()
	{
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "DEBUG");
	}

	[MenuItem("DebugMode/Unset debug mode")]
	static void UnsetDebugMode()
	{
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, "");
	}
}
