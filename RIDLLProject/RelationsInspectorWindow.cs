using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using EditorCoroutines;
using System.Linq;
using RelationsInspector.Extensions;
using System.Reflection;
using System;

namespace RelationsInspector
{
	public class RelationsInspectorWindow : EditorWindow, RelationsInspectorAPI
	{		
		IWorkspace workspace;

		[SerializeField]
		internal RelationInspectorSkin skin;

		[SerializeField]
		bool initialized;

		HashSet<object> targetObjects;

		Rect workspaceRect;

		event System.Action OnUpdate;

		List<Type> allBackends;
		List<Type> validBackends;
		Type selectedBackend;
		const string PrefsKeyDefaultBackend = "RIWindowDefaultBackend";

		static GUIContent clearButtonContent = new GUIContent("Clear", "Removes all window content");
		static GUIContent refreshButtonContent = new GUIContent("Refresh", "Rebuilds the graph from the target objects");

		internal void ExecOnUpdate(System.Action action)
		{
			OnUpdate += action;
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

			DrawToolbar();
			
			// allow the user to draw their own controls and return the remaining rect
			var wsRect = DrawWorkspaceControls();

			if (Event.current.type == EventType.repaint)
				workspaceRect = wsRect;

			// clip at the rect borders
			GUI.BeginGroup(workspaceRect, GUI.skin.box);
			WorkspaceOnGUI(workspaceRect);
			GUI.EndGroup();

			HandleEvent(Event.current);
		}

		void OnSelectionChange()
		{
			if (workspace != null)
				workspace.OnSelectionChange();
		}

		void ResetWindow()
		{			
			selectedBackend = null;
			validBackends = null;
			allBackends = null;
			workspace = null;
			targetObjects = null;
			OnUpdate = null;
		}

		static Type[] GetGenericArguments(Type backend)
		{
			var backendInterface = ReflectionUtil.GetGenericInterface(backend, typeof(IGraphBackend<,>));
			if (backendInterface == null)
				throw new ArgumentException(backend + "does not implement backend");
			return backendInterface.GetGenericArguments();
		}

		static bool IsBackendType(Type candidateType)
		{
			Type backendInterface = ReflectionUtil.GetGenericInterface(candidateType, typeof(IGraphBackend<,>));
			if (backendInterface == null)
				return false;

			if (backendInterface.GetGenericArguments().Any(arg => arg.IsGenericParameter))
				return false;
			return true;
		}

		static List<Type> FindBackends()
		{
			var assemblies = new[] { ReflectionUtil.GetAssemblyByName("Assembly-CSharp-Editor"), typeof(RelationsInspectorWindow).Assembly };
			return assemblies.SelectMany(asm => asm.GetTypes()).Where( IsBackendType ).ToList();
		}

		void InitWindow()
		{
			string dependencyError = ProjectSettings.CheckDependentFiles();
			if (!string.IsNullOrEmpty(dependencyError))
			{
				ShowNotification(new GUIContent(dependencyError));
				return;
			}

			validBackends = allBackends = FindBackends();

			var fallbackBackend = ReflectionUtil.GetAssemblyByName("Assembly-CSharp-Editor").GetType(ProjectSettings.DefaultBackendClassName, false, true);
			if (fallbackBackend == null)
				fallbackBackend = validBackends.FirstOrDefault();

			selectedBackend = GUIUtil.GetPrefsType(PrefsKeyDefaultBackend, fallbackBackend );

			wantsMouseMove = true;	// for dragging edges

			InitWorkspace();
			if (!allBackends.Any())
			{
				ShowNotification( new GUIContent("Could not find any backend.") );
				return;
			}
			else if (selectedBackend == null)
			{
				ShowNotification( new GUIContent("Could not find default backend.") );
				return;
			}

			initialized = true;
		}

		void WorkspaceOnGUI(Rect drawRect)
		{
			if (workspace != null)
				workspace.OnGUI(drawRect);
		}

		void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			// clear
			GUI.enabled = targetObjects != null;
			if (GUILayout.Button(clearButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				ExecOnUpdate( ClearWindow );
			GUI.enabled = true;

			// re-create the workspace from targets
			GUI.enabled = targetObjects != null && targetObjects.Any();
			if (GUILayout.Button(refreshButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
				InitWorkspace();
			GUI.enabled = true;

			// backend selector
			int selectedBackendId = selectedBackend != null ? validBackends.IndexOf(selectedBackend) : -1;
			float popupWidth = selectedBackend == null ? 150 : EditorStyles.toolbarPopup.CalcSize(new GUIContent(selectedBackend.Name)).x + 5;
			EditorGUI.BeginChangeCheck();
			selectedBackendId = EditorGUILayout.Popup(selectedBackendId, validBackends.Select(t => t.Name).ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(popupWidth));
			if (EditorGUI.EndChangeCheck())
			{
				var newSelection = validBackends[selectedBackendId];
				GUIUtil.SetPrefsType(PrefsKeyDefaultBackend, newSelection);
				ExecOnUpdate(() => OnSelectBackend(newSelection) );
			}

			GUILayout.FlexibleSpace();

			if (workspace != null)
				workspace.OnToolbarGUI();

			EditorGUILayout.EndHorizontal();
		}

		Rect DrawWorkspaceControls()
		{
			if (workspace != null)
				return workspace.OnControlsGUI( );

			return GUILayoutUtility.GetRect(0, 0, new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) });
		}

		void ClearWindow()
		{
			targetObjects = null;
			OnTargetChange();
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

			if (workspace != null)
				workspace.Update();
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
							ExecOnUpdate(() => AddTargets(dragObjs) );
						else
							ExecOnUpdate(() => SetTargets(dragObjs) );

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

		static UnityEngine.Object[] GetDragObjects()
		{
			UnityEngine.Object[] objs = DragAndDrop.objectReferences;
			if (objs != null)
				return objs.ToArray();	// copy the array

			return new UnityEngine.Object[0];
		}

		public void AddTargets(object[] targetsToAdd)
		{
			if (targetObjects == null)
				targetObjects = new HashSet<object>(targetsToAdd);
			else
				targetObjects.UnionWith(targetsToAdd);

			OnTargetChange();
		}

		public void SetTargets(object[] targets)
		{
			targetObjects = new HashSet<object>(targets);
			OnTargetChange();
		}

		void OnTargetChange()
		{
			UpdateBackend();
			InitWorkspace();
		}

		Type MostSpecific(IList<Type> backends)
		{
			if (backends == null || backends.Count() == 0)
				return null;

			var groups = backends.GroupBy(backend => GetGenericArguments(backend).First());
			var entityTypes = groups.Select(group => group.Key).ToHashSet();

			var bestEntityType = ReflectionUtil.GetMostSpecificType(entityTypes);
			var bestEntityTypeGroup = groups.Single(group => group.Key == bestEntityType);
			return bestEntityTypeGroup.First();
		}

		void UpdateBackend()
		{
			if (targetObjects == null || !targetObjects.Any())
				validBackends = allBackends.ToList();
			else
			{
				var targetTypes = ReflectionUtil.GetTypesAssignableFrom(targetObjects.Select(obj => obj.GetType()));
				var backendsWithMatchingEntityTypes = allBackends.Where(backend => EntityTypeIsAssignableFromAny(backend, targetTypes));
				var backendsWithMatchingTargetAttribute = allBackends.Where(backend => BackendAttributeFitsAny(backend, targetTypes));
				validBackends = backendsWithMatchingEntityTypes.Union(backendsWithMatchingTargetAttribute).ToList();
			}

			if (!validBackends.Contains(selectedBackend))
				selectedBackend = MostSpecific(validBackends);
		}

		bool EntityTypeIsAssignableFromAny(Type backendType, IEnumerable<System.Type> types)
		{
			var entityType = GetGenericArguments(backendType).First();
			return entityType != null && types.Where(t => entityType.IsAssignableFrom(t)).Any();
		}

		// true if any of the given types can be passed as a RI attribute type
		bool BackendAttributeFitsAny(Type backendType, IEnumerable<System.Type> types)
		{
			var attributes = (RelationsInspectorAttribute[]) backendType.GetCustomAttributes(typeof(RelationsInspectorAttribute), true);
			return attributes.Any(attr => types.Any(t => attr.type.IsAssignableFrom(t)));
		}

		void OnSelectBackend(Type backendType)
		{
			selectedBackend = backendType;
			InitWorkspace();
		}

		bool InitWorkspace()
		{
			if (selectedBackend == null )
			{
				workspace = null;
				return false;
			}

			workspace = CreateWorkspace();
			return (workspace != null);
		}

		IWorkspace CreateWorkspace()
		{
			var backendArguments = GetGenericArguments(selectedBackend);
			Type entityType = backendArguments[0];
			Type relationTagType = backendArguments[1];

			var genericWorkspaceType = typeof(Workspace<,>).MakeGenericType( entityType, relationTagType );
			var flags = BindingFlags.NonPublic | BindingFlags.Instance;
			var targetArray = targetObjects == null ? null : targetObjects.ToArray();
			var ctorArguments = new object[] { selectedBackend, this, targetArray };
			return (IWorkspace)System.Activator.CreateInstance(genericWorkspaceType, flags, null, ctorArguments, null);
		}

		#region implementing RelationsInspectorAPI

		void RelationsInspectorAPI.ClearWindow()
		{
			ExecOnUpdate( ClearWindow );
		}

		// draw a fresh view of the graph
		void RelationsInspectorAPI.Repaint()
		{
			ExecOnUpdate( Util.Idle );	// Update repaints after any action, so idle is enough to cause a repaint
		}

		// manipulate the graph through targets
		void RelationsInspectorAPI.ResetTargets(object[] targets)
		{
			ExecOnUpdate(() => SetTargets(targets) );
		}

		// if a graph exists, add targets. else create a new one from the targets
		void RelationsInspectorAPI.AddTargets(object[] targets)
		{
			ExecOnUpdate( () => AddTargets(targets) );
		}

		// manipulate the graph directly
		void RelationsInspectorAPI.AddEntity(object entity, Vector2 position)
		{
			if( workspace != null)
				ExecOnUpdate(() => workspace.AddEntity(entity, position) );
		}

		void RelationsInspectorAPI.RemoveEntity(object entity)
		{
			if( workspace != null)
				ExecOnUpdate( () => workspace.RemoveEntity(entity) );
		}

		void RelationsInspectorAPI.InitRelation(object[] sourceEntities, object tag)
		{
			if (workspace != null)
				ExecOnUpdate(() => workspace.CreateEdge(sourceEntities, tag));
		}

		void RelationsInspectorAPI.AddRelation(object sourceEntity, object targetEntity, object tag)
		{
			if (workspace != null)
				ExecOnUpdate(() => workspace.AddEdge(sourceEntity, targetEntity, tag) );
		}

		void RelationsInspectorAPI.RemoveRelation(object sourceEntity, object targetEntity, object tag)
		{
			if (workspace != null)
				ExecOnUpdate(() => workspace.RemoveEdge(sourceEntity, targetEntity, tag) );
		}

		// enforce backend selection
		void RelationsInspectorAPI.SetBackend(Type backendType)
		{
			if (!IsBackendType(backendType))
				throw new ArgumentException(backendType + " is not a valid backend type.");

			ExecOnUpdate(() => OnSelectBackend(backendType) );
		}

		#endregion
	}
}

