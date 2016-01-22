#if RIDEMO
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
	static class DemoRestriction
	{
		static int inputEventCount = 0;
		const int inputEventCountThreshold = 75;
		
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
					Application.OpenURL( @"https://www.assetstore.unity3d.com/en/#!/content/158589" );
			}
		}
	}
}
#endif  //RIDEMO
