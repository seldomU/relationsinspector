#if RIDEMO
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
	static class DemoRestriction
	{
		static int inputEventCount = 0;
		const int inputEventCountThreshold = 500;
		
		public static void Run()
		{

			switch (Event.current.type)
			{
				case EventType.MouseUp:
				case EventType.MouseDown:
					inputEventCount++;
					break;
			}

			if (inputEventCount >= inputEventCountThreshold)
			{
				inputEventCount = 0;
				bool openStore = EditorUtility.DisplayDialog("Demo", "Thanks for trying the RelationsInspector demo. It is not restricted, but to use the tool permanently, you have to buy the full version.", "To the store", "Not now" ); 
				if(openStore)
					UnityEditorInternal.AssetStore.Open( ProjectSettings.StoreDemoURL );
			}
		}

		public static void OnEnable()
		{
			inputEventCount = GUIUtil.GetPrefsInt( "DemoActions", 0 );
		}

		public static void OnDestroy()
		{
			GUIUtil.SetPrefsInt( "DemoActions", inputEventCount );
		}
	}
}
#endif  //RIDEMO
