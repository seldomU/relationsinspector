#if RIDEMO
using UnityEngine;
using UnityEditor;

namespace RelationsInspector
{
	static class DemoRestriction
	{
		static int inputEventCount = 0;
		const int inputEventCountThreshold = 75;

		static float demoMessageTime;
		const float demoGUIBlockDuration = 5;	// in seconds
		
		public static bool IsActive( System.Action<GUIContent> showMessage )
		{
			if (demoMessageTime > Time.realtimeSinceStartup)
				return true;
			
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
				demoMessageTime = Time.realtimeSinceStartup + demoGUIBlockDuration;
				showMessage.Invoke(new GUIContent("Demo naptime. Check back shortly."));
			}

			return false;
		}
	}
}
#endif  //RIDEMO
