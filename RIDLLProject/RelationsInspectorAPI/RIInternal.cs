using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using RelationsInspector.Extensions;
using Object = UnityEngine.Object;

namespace RelationsInspector
{
    class RIInternal
    {        
        HashSet<object> targetObjects;
        IWorkspace workspace;
        Action<Action> Exec;
        Action<GUIContent> ShowNotification;

        List<Type> allBackendTypes;
        List<Type> validBackendTypes;
        Type selectedBackendType;
        RelationsInspectorWindow window;
        RIStateHistory targetHistory;

        const string PrefsKeyDefaultBackend = "RIWindowDefaultBackend";

        static GUIContent clearButtonContent = new GUIContent("Clear", "Removes all window content");
        static GUIContent refreshButtonContent = new GUIContent("Refresh", "Rebuilds the graph from the target objects");

        internal RIInternal(Action<Action> Exec, Action<GUIContent> ShowNotification, RelationsInspectorWindow window)
        {
            this.Exec = Exec;
            this.ShowNotification = ShowNotification;
            this.window = window;
            
            validBackendTypes = allBackendTypes = BackendUtil.GetNonGenericBackendTypes();

            targetHistory = new RIStateHistory();

            var fallbackBackendType = TypeUtil
                .GetAssemblyByName("Assembly-CSharp-Editor")
                .GetType(ProjectSettings.DefaultBackendClassName, false, true);

            if (fallbackBackendType == null)
                fallbackBackendType = validBackendTypes.FirstOrDefault();

            selectedBackendType = GUIUtil.GetPrefsType(PrefsKeyDefaultBackend) ?? fallbackBackendType;

            if (!allBackendTypes.Any())
            {
                ShowNotification(new GUIContent("Could not find any backend."));
                return;
            }
            else if (selectedBackendType == null)
            {
                ShowNotification(new GUIContent("Could not find default backend."));
                return;
            }
        }

        internal void Init()
        {
            InitWorkspace();
        }

        // draw a fresh view of the graph
        public void Repaint()
        {
            Exec(Util.IdleAction);  // Update repaints after any action, so idle is enough to cause a repaint
        }

        // manipulate the graph through targets
        public void ResetTargets(object[] targets)
        {
            targetHistory.RegisterState(targets, selectedBackendType);
            Exec(() => SetTargetObjects(targets));
        }

        // if a graph exists, add targets. else create a new one from the targets
        public void AddTargets(object[] targets)
        {
            Exec(() => AddTargetObjects(targets));
        }

        // manipulate the graph directly
        public void AddEntity(object entity, Vector2 position)
        {
            if (workspace != null)
                Exec(() => workspace.AddEntity(entity, position));
        }

        public void RemoveEntity(object entity)
        {
            if (workspace != null)
                Exec(() => workspace.RemoveEntity(entity));
        }

        public void InitRelation(object[] sourceEntities, object tag)
        {
            if (workspace != null)
                Exec(() => workspace.CreateEdge(sourceEntities, tag));
        }

        public void AddRelation(object sourceEntity, object targetEntity, object tag)
        {
            if (workspace != null)
                Exec(() => workspace.AddEdge(sourceEntity, targetEntity, tag));
        }

        public void RemoveRelation(object sourceEntity, object targetEntity, object tag)
        {
            if (workspace != null)
                Exec(() => workspace.RemoveEdge(sourceEntity, targetEntity, tag));
        }

        // enforce backend selection
        public void SetBackend(Type backendType)
        {
            if (!BackendUtil.IsBackendType(backendType))
                throw new ArgumentException(backendType + " is not a valid backend type.");

            targetHistory.RegisterBackendChange(backendType);
            Exec(() => OnSelectBackend(backendType));
        }

        public void SelectEntityNodes(System.Predicate<object> doSelect)
        {
            if (workspace != null)
                Exec(() => workspace.SelectEntityNodes(doSelect));
        }

        public void SendEvent(Event e)
        {
            if (workspace != null)
                workspace.OnEvent(e);
        }


        void OnSelectBackend(Type backendType)
        {
            selectedBackendType = backendType;
            InitWorkspace();
        }

        bool InitWorkspace()
        {
            if (workspace != null)
                workspace.OnDestroy();

            if (selectedBackendType == null)
            {
                workspace = null;
                return false;
            }

            workspace = CreateWorkspace();
            return (workspace != null);
        }



        IWorkspace CreateWorkspace()
        {
            var backendArguments = BackendUtil.GetGenericArguments(selectedBackendType);
            Type entityType = backendArguments[0];
            Type relationTagType = backendArguments[1];

            var genericWorkspaceType = typeof(Workspace<,>).MakeGenericType(backendArguments);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var targetArray = TypeUtil.MakeObjectsAssignable( targetObjects, entityType );
            var ctorArguments = new object[]
            {
                selectedBackendType,
                targetArray,
                (Func<RelationsInspectorAPI>)window.GetAPI,
                (Action)window.Repaint,
                (Action<Action>)window.ExecOnUpdate
            };
            return (IWorkspace)System.Activator.CreateInstance(genericWorkspaceType, flags, null, ctorArguments, null);
        }

        internal void AddTargetObjects(object[] targetsToAdd)
        {
            if (targetObjects == null)
                targetObjects = new HashSet<object>(targetsToAdd);
            else
                targetObjects.UnionWith(targetsToAdd);

            OnTargetChange();
        }

        internal void SetTargetObjects(object[] targets)
        {
            if (targets == null)
                targetObjects = null;
            else
                targetObjects = new HashSet<object>(targets);
            OnTargetChange();
        }

        void OnTargetChange()
        {
            UpdateBackend();
            InitWorkspace();
        }

        void UpdateBackend()
        {
            if (targetObjects == null || !targetObjects.Any())
                validBackendTypes = allBackendTypes.ToList();
            else
            {
                // find all possible backend entity types that can be derived from the set of target objects
                var targetTypes = TypeUtil.GetTypesAssignableFrom(targetObjects.Select(obj => obj.GetType()));
                var targetComponentTypes = TypeUtil.GetSharedComponentTypes( targetObjects );
                var backendEntityTypes = targetTypes.Union( targetComponentTypes );

                // find all backends that accept any of the possible entity types
                validBackendTypes = allBackendTypes
                    .Where( backendType => BackendUtil.IsEntityTypeAssignableFromAny( backendType, backendEntityTypes ) )
                    .ToList();
            }

            if (!validBackendTypes.Contains(selectedBackendType))
                selectedBackendType = BackendUtil.GetMostSpecificBackend(validBackendTypes);
        }

        internal void OnSelectionChange()
        {
            if (workspace != null)
                workspace.OnSelectionChange();
        }

        internal void OnDestroy()
        {
            if (workspace != null)
                workspace.OnDestroy();
        }

        internal void OnWorkspaceGUI(Rect drawRect)
        {
            if (workspace != null)
                workspace.OnGUI(drawRect);
        }

        internal Rect DrawWorkspaceControls()
        {
            if (workspace != null)
                return workspace.OnControlsGUI();

            return GUILayoutUtility.GetRect(0, 0, new[] { GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true) });
        }

        void LoadHistoryState(object[] targets, Type backendType)
        {
            Exec(
               () =>
               {
                   // don't use SetBackend or ResetTargets, they would add this change to the history
                   selectedBackendType = backendType;
                   SetTargetObjects(targets);
               } );
        }

        internal void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            // target history navigation
            targetHistory.OnGUI( LoadHistoryState );

            // clear
            GUI.enabled = targetObjects != null;
            if (GUILayout.Button(clearButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                Exec(() => SetTargetObjects(null));
            GUI.enabled = true;

            // re-create the workspace from targets
            GUI.enabled = targetObjects != null && targetObjects.Any();
            if (GUILayout.Button(refreshButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                InitWorkspace();
            GUI.enabled = true;

            // backend selector
            int selectedBackendId = selectedBackendType != null ? validBackendTypes.IndexOf(selectedBackendType) : -1;
            float popupWidth = selectedBackendType == null ? 150 : EditorStyles.toolbarPopup.CalcSize(new GUIContent(selectedBackendType.Name)).x + 5;
            EditorGUI.BeginChangeCheck();
            selectedBackendId = EditorGUILayout.Popup(selectedBackendId, validBackendTypes.Select(t => t.Name).ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(popupWidth));
            if (EditorGUI.EndChangeCheck())
            {
                var newSelection = validBackendTypes[selectedBackendId];
                GUIUtil.SetPrefsType(PrefsKeyDefaultBackend, newSelection);
                SetBackend(newSelection);
            }

            GUILayout.FlexibleSpace();

            if (workspace != null)
                workspace.OnToolbarGUI();

            EditorGUILayout.EndHorizontal();
        }

        internal void Update()
        {
            if (workspace != null)
                workspace.Update();
        }

        internal void HandleEvent(Event ev, Rect windowPosition)
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
                            AddTargets(dragObjs);
                        else
                            ResetTargets(dragObjs);

                        DragAndDrop.AcceptDrag();
                        ev.Use();
                        break;
                    }

                case EventType.Repaint:
                    if (DragAndDrop.visualMode == DragAndDropVisualMode.Generic || DragAndDrop.visualMode == DragAndDropVisualMode.Move)
                    {
                        var bgColor = SkinManager.GetSkin().windowColor;
                        Util.FadeRect(windowPosition.ResetOrigin(), bgColor);
                    }
                    break;
            }
        }

        static Object[] GetDragObjects()
        {
            Object[] objs = DragAndDrop.objectReferences;
            if (objs != null)
                return objs.ToArray();  // copy the array

            return new Object[0];
        }
    }
}
