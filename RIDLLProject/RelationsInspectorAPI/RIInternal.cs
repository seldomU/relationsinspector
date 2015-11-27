using System;
using System.Collections.Generic;
using System.Linq;
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

        static readonly GUIContent clearButtonContent = new GUIContent("Clear", "Removes all window content");
        static readonly GUIContent rebuildButtonContent = new GUIContent("Rebuild", "Rebuilds the graph from the target objects");

        internal RIInternal(Action<Action> Exec, Action<GUIContent> ShowNotification, RelationsInspectorWindow window)
        {
            this.Exec = Exec;
            this.ShowNotification = ShowNotification;
            this.window = window;
            
            // all closed backend types are eligible
            validBackendTypes = allBackendTypes = BackendUtil.backendTypes.Where( t=>!t.IsOpen() ).ToList();

            targetHistory = new RIStateHistory();

            var fallbackBackendType = TypeUtil
                .GetAssemblyByName("Assembly-CSharp-Editor")
                .GetType(ProjectSettings.DefaultBackendClassName, false, true);

            if (fallbackBackendType == null)
                fallbackBackendType = validBackendTypes.FirstOrDefault();

            selectedBackendType = GUIUtil.GetPrefsBackendType(PrefsKeyDefaultBackend) ?? fallbackBackendType;

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
        
        // rebuild the graph from the current targets
        public void Rebuild()
        {
            InitWorkspace();
        }

        // relayout the current graph
        public void Relayout()
        {
            if ( workspace != null )
                workspace.Relayout();
        }

        // manipulate the graph through targets
        public void ResetTargets(object[] targets)
        {
            targetHistory.RegisterState(targets, selectedBackendType);
            Exec(() => SetTargetObjects(targets));
        }

        // if a graph exists, add targets. else create a new one from the targets
        public void AddTargets(object[] targets, Vector2 pos)
        {
            Exec(() => AddTargetObjects(targets, pos));
        }

        // manipulate the graph directly
        public void AddEntity(object entity, Vector2 position)
        {
            if ( workspace == null )
                return;

            var assignableEntity = MakeAssignableEntities( new[] { entity }, selectedBackendType ).FirstOrDefault();
            Exec( () => workspace.AddEntity( assignableEntity, position ) );
        }

        public void RemoveEntity(object entity)
        {
            if ( workspace == null )
                return;

            var assignableEntity = MakeAssignableEntities( new[] { entity }, selectedBackendType ).FirstOrDefault();
            Exec(() => workspace.RemoveEntity(entity));
        }

        internal void ExpandEntity( object entity )
        {
            if ( workspace != null )
                Exec( () => workspace.ExpandEntity( entity ) );
        }

        internal void FoldEntity( object entity )
        {
            if ( workspace != null )
                Exec( () => workspace.FoldEntity( entity ) );
        }

        public void InitRelation(object[] sourceEntities, object tag)
        {
            if (workspace != null)
                Exec(() => workspace.CreateRelation(sourceEntities, tag));
        }

        public void AddRelation(object sourceEntity, object targetEntity, object tag)
        {
            if (workspace != null)
                Exec(() => workspace.AddRelation( sourceEntity, targetEntity, tag));
        }

        public void RemoveRelation(object sourceEntity, object targetEntity, object tag)
        {
            if (workspace != null)
                Exec(() => workspace.RemoveRelation( sourceEntity, targetEntity, tag));
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
            Type entityType = BackendUtil.BackendAttrType(selectedBackendType) ?? backendArguments[0];
            Type relationTagType = backendArguments[1];

            var genericWorkspaceType = typeof(Workspace<,>).MakeGenericType(backendArguments);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            var assignableTargets = MakeAssignableEntities( targetObjects, selectedBackendType );
            object[] targetArray = assignableTargets.Any() ? assignableTargets.ToArray() : null; // make sure to pass null, NOT an empty array
            var ctorArguments = new object[]
            {
                selectedBackendType,
                targetArray,
                window.GetAPI(),
                (Action)window.Repaint,
                (Action<Action>)window.ExecOnUpdate
            };
            return (IWorkspace)Activator.CreateInstance(genericWorkspaceType, flags, null, ctorArguments, null);
        }

        internal void AddTargetObjects( object[] targetsToAdd )
        {
            AddTargetObjects( targetsToAdd, Vector2.zero );
        }

        internal void AddTargetObjects(object[] targetsToAdd, Vector2 pos)
        {
            if ( targetObjects == null )
            {
                SetTargetObjects( targetsToAdd );
                return;
            }

            targetObjects.UnionWith(targetsToAdd);


            var entitiesToAdd = MakeAssignableEntities( targetsToAdd, selectedBackendType );
            workspace.AddTargets( entitiesToAdd.ToArray(), pos );
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
            validBackendTypes = GetValidBackendTypes( targetObjects, allBackendTypes ).ToList();

            if (!validBackendTypes.Contains(selectedBackendType))
                selectedBackendType = BackendUtil.GetMostSpecificBackendType(validBackendTypes);
        }

        static IEnumerable<Type> GetValidBackendTypes( IEnumerable<object> targetEntities, IEnumerable<Type> backendTypes )
        {
            if ( targetEntities == null || !targetEntities.Any() )
                return backendTypes;

            var entityTypes = TypeUtil.GetValidEntityTypes( targetEntities );

            var autoBackendTypes = BackendUtil.CreateAutoBackendTypes( entityTypes );

            var matchingBackendTypes = backendTypes
                .Where( t => !t.IsGenericType )
                .Where( backendType =>
                        BackendUtil.IsEntityTypeAssignableFromAny( backendType, entityTypes ) ||
                        BackendUtil.BackendAttributeFitsAny( backendType, entityTypes ) );

            return autoBackendTypes.Concat( matchingBackendTypes );
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
            if (GUILayout.Button(rebuildButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth(false)))
                InitWorkspace();
            GUI.enabled = true;

            // backend selector
            int selectedBackendId = selectedBackendType != null ? validBackendTypes.IndexOf(selectedBackendType) : -1;
            float popupWidth = selectedBackendType == null ? 150 : EditorStyles.toolbarPopup.CalcSize(new GUIContent(selectedBackendType.Name)).x + 5;
            EditorGUI.BeginChangeCheck();
            selectedBackendId = EditorGUILayout.Popup(selectedBackendId, validBackendTypes.Select( TypeName ).ToArray(), EditorStyles.toolbarPopup, GUILayout.Width(popupWidth));
            if (EditorGUI.EndChangeCheck())
            {
                var newSelection = validBackendTypes[selectedBackendId];
                // don't save constructed types (because we can't deserialize them yet)
                if ( !newSelection.GetGenericArguments().Any() )
                    GUIUtil.SetPrefsBackendType(PrefsKeyDefaultBackend, newSelection);
                SetBackend(newSelection);
            }

            GUILayout.FlexibleSpace();

            // workspace toolbar
            if (workspace != null)
                workspace.OnToolbarGUI();

            GUILayout.FlexibleSpace();

            // setttings menu
            if ( GUILayout.Button( new GUIContent(SkinManager.GetSkin().settingsIcon,"Settings"), EditorStyles.toolbarButton, GUILayout.Width( 25 ) ) )
                SettingsMenu.Create();

            EditorGUILayout.EndHorizontal();
        }

        string TypeName( Type t )
        {
            if ( !t.IsGenericType )
                return t.Name;
            return t.Name.Remove( t.Name.IndexOf( '`' ) ) + " of " + t.GetGenericArguments()[ 0 ].Name;
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
                            AddTargets(dragObjs, Event.current.mousePosition);
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

        static IEnumerable<object> MakeAssignableEntities( IEnumerable<object> objs, Type selectedBackendType )
        {
            if ( objs == null )
                return Enumerable.Empty<object>();

            var entityType = BackendUtil.GetEntityType( selectedBackendType );
            return objs.Select( o => TypeUtil.MakeAssignable( o, entityType) );
        }
    }
}
