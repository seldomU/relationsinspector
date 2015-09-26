using UnityEngine;
using UnityEditor;
using System.Linq;
using RelationsInspector.Extensions;

// make internal classes accessible to unit tests
#if DEBUG
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp-Editor")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp")]
#endif


namespace RelationsInspector
{
	public class RelationsInspectorWindow : EditorWindow
    {
        RIInternal internalAPI;
        APIv1Implementation api1impl;
        APIv2Implementation api2impl;

		[SerializeField]
		internal RelationInspectorSkin skin;

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
			if (DemoRestriction.IsActive( ShowNotification ))
				return;
#endif// RIDEMO

			skin = SkinManager.GetSkin();	// every frame, so it adapts when the user changes her unity skin

			internalAPI.DrawToolbar();
			
			// allow the user to draw their own controls and return the remaining rect
			var wsRect = internalAPI.DrawWorkspaceControls();

			if (Event.current.type == EventType.repaint)
				workspaceRect = wsRect;

			// clip at the rect borders
			GUI.BeginGroup(workspaceRect, GUI.skin.box);
			internalAPI.OnWorkspaceGUI(workspaceRect);
			GUI.EndGroup();

			HandleEvent(Event.current);
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

		void HandleEvent(Event ev)
		{
			switch (ev.type)
			{
				case EventType.DragUpdated:
					{
						bool controlHeld = (ev.modifiers & EventModifiers.Control) != 0;
						DragAndDrop.visualMode = controlHeld ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Move;
						break;
					}

				case EventType.DragPerform:
					{
						bool controlHeld = (ev.modifiers & EventModifiers.Control) != 0;
						// if control is held down, add to the existing targets
						// else replace them
						bool doAddObjects = controlHeld;

						var dragObjs = GetDragObjects();

						if (doAddObjects)
							internalAPI.AddTargets(dragObjs);
						else
							internalAPI.ResetTargets(dragObjs);

						DragAndDrop.AcceptDrag();
						ev.Use();
						break;
					}

				case EventType.Repaint:
					if (DragAndDrop.visualMode == DragAndDropVisualMode.Generic || DragAndDrop.visualMode == DragAndDropVisualMode.Move)
					{
						var bgColor = skin.windowColor;
						Util.FadeRect( position.ResetOrigin(), bgColor );
					}
					break;
			}
		}

		static Object[] GetDragObjects()
		{
			Object[] objs = DragAndDrop.objectReferences;
			if (objs != null)
				return objs.ToArray();	// copy the array

			return new Object[0];
		}
	}
}

