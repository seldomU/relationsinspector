using UnityEngine;
using UnityEditor;
using RelationsInspector.Extensions;

// make internal classes accessible to unit tests
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Assembly-CSharp-Editor" )]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo( "Assembly-CSharp" )]
#endif


namespace RelationsInspector
{
    public class RelationsInspectorWindow : EditorWindow
    {
        RIInternal internalAPI;
        APIv1Implementation api1impl;
        APIv2Implementation api2impl;

		[SerializeField]
		bool initialized;

		Rect workspaceRect;

		event System.Action OnUpdate;

		internal void ExecOnUpdate(System.Action action)
		{
			OnUpdate += action;
		}

        public RelationsInspectorAPI GetAPI()
        {
            return api1impl;
        }

        internal RelationsInspectorAPI2 GetAPI2()
        {
            return api2impl;
        }

		void OnGUI()
		{
			if (!initialized)
				return;

#if RIDEMO
			if (DemoRestriction.IsActive( content => { ShowNotification(content); Repaint();} ))
				return;
#endif// RIDEMO

			internalAPI.DrawToolbar();
			
			// allow the user to draw their own controls and return the remaining rect
			var wsRect = internalAPI.DrawWorkspaceControls();

            if ( Event.current.type == EventType.repaint )
                workspaceRect = wsRect;

            // clip at the rect borders
            GUI.BeginGroup( workspaceRect, GUIStyle.none );
			internalAPI.OnWorkspaceGUI( workspaceRect.ResetOrigin() );
			GUI.EndGroup();

			internalAPI.HandleEvent(Event.current, position);
		}

		void OnSelectionChange()
		{
            internalAPI.OnSelectionChange();
		}

        void OnDestroy()
        {
            internalAPI.OnDestroy();
            AssetDatabase.SaveAssets();
        }

		void InitWindow()
		{
			string dependencyError = ProjectSettings.CheckDependentFiles();
			if (!string.IsNullOrEmpty(dependencyError))
			{
				ShowNotification(new GUIContent(dependencyError));
				return;
			}

            wantsMouseMove = true;	// for dragging edges

            internalAPI = new RIInternal( ExecOnUpdate, ShowNotification, this );
            api1impl = new APIv1Implementation(internalAPI);
            api2impl = new APIv2Implementation(internalAPI);
            internalAPI.Init();

			initialized = true;
            Repaint();
		}

		void Update()
		{
			try
			{
				if (OnUpdate != null)
				{
					// reset OnUpdate BEFORE invoking it (!) so that actions can be added by the invoked code.
					System.Action stuffToExecute = OnUpdate;
					OnUpdate = null;
					stuffToExecute.Invoke();
					Repaint();
				}
			}
			catch(System.Exception e)
			{
				Debug.LogException(e);
			}

            internalAPI.Update();
		}

		void OnEnable()
		{
			InitWindow();
		}
	}
}

