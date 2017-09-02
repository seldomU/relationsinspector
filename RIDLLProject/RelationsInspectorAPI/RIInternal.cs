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
		Action<Action> ExecDelayed;
		Action<GUIContent> ShowNotification;

		List<Type> allBackendTypes;
		List<Type> validBackendTypes;
		Type selectedBackendType;
		RelationsInspectorWindow window;
		RIStateHistory targetHistory;

		const string PrefsKeyDefaultBackend = "RIWindowDefaultBackend";

		static readonly GUIContent clearButtonContent = new GUIContent( "Clear", "Removes all window content" );
		static readonly GUIContent rebuildButtonContent = new GUIContent( "Rebuild", "Rebuilds the graph from the target objects" );

		internal RIInternal( Action<Action> ExecDelayed, Action<GUIContent> ShowNotification, RelationsInspectorWindow window )
		{
			this.ExecDelayed = ExecDelayed;
			this.ShowNotification = ShowNotification;
			this.window = window;

			// all closed backend types are eligible
			validBackendTypes = allBackendTypes = BackendTypeUtil.backendTypes.Where( t => !t.IsOpen() && !BackendTypeUtil.DoHide( t ) ).ToList();

			targetHistory = new RIStateHistory();

			var firstPassEditorDll = TypeUtil.GetAssemblyByName( "Assembly-CSharp-Editor-firstpass" );

			Type fallbackBackendType = ( firstPassEditorDll != null ) ?
				firstPassEditorDll.GetType( ProjectSettings.DefaultBackendClassName, false, true )
				:
				null;

			if ( fallbackBackendType == null )
				fallbackBackendType = validBackendTypes.FirstOrDefault();

			selectedBackendType = GUIUtil.GetPrefsBackendType( PrefsKeyDefaultBackend ) ?? fallbackBackendType;

			if ( !allBackendTypes.Any() )
			{
				ShowNotification( new GUIContent( "Could not find any backend." ) );
				return;
			}
			else if ( selectedBackendType == null )
			{
				ShowNotification( new GUIContent( "Could not find default backend." ) );
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
			Exec( Util.IdleAction, false );  // Update repaints after any action, so idle is enough to cause a repaint
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

		public void ResetTargets( object[] targets, bool delayed = true )
		{
			ResetTargets( targets, null, delayed );
		}

		public void ResetTargets( object[] targets, Type backendType, bool delayed = true )
		{
			if ( !UserAcceptsTargetCount( targets.Length ) )
				return;

			if ( backendType != null && !BackendTypeUtil.IsBackendType( backendType ) )
				throw new ArgumentException( backendType + " is not a valid backend type." );

			targetHistory.RegisterState( targets, backendType );
			Exec( () => SetTargetObjects( targets, backendType ), delayed );
		}

		// if numTargets exceeds the node limit, warn the user through a dialog
		// returns true if user wants to proceed
		public bool UserAcceptsTargetCount( int numTargets )
		{
			if ( numTargets <= Settings.Instance.maxGraphNodes )
				return true;

			int choice = EditorUtility.DisplayDialogComplex(
				"Too many targets",
				string.Format( "The number of target objects ({0}) exceeds the graph node limit({1}). Performance might suffer.", numTargets, Settings.Instance.maxGraphNodes ),
				"Continue",
				"Abort",
				"Edit settings" );

			if ( choice == 2 )
				Selection.activeObject = Settings.Instance;

			return ( choice == 0 );
		}

		// if a graph exists, add targets. else create a new one from the targets
		public void AddTargets( object[] targets, Vector2 pos, bool delayed = true )
		{
			Exec( () => AddTargetObjects( targets, pos ), delayed );
		}

		public object[] GetTargets()
		{
			if ( targetObjects == null )
				return new object[ 0 ];

			return targetObjects.ToArray();
		}

		public IEnumerable<object> GetEntities()
		{
			return ( workspace == null ) ? Enumerable.Empty<object>() : workspace.GetEntities();
		}

		public IEnumerable<object> GetRelations()
		{
			return ( workspace == null ) ? Enumerable.Empty<object>() : workspace.GetRelations();
		}

		// manipulate the graph directly
		public void AddEntity( object entity, Vector2 position, bool delayed = true )
		{
			if ( workspace == null )
				return;

			var assignableEntity = MakeAssignableEntities( new[] { entity }, selectedBackendType ).FirstOrDefault();
			Exec( () => workspace.AddEntity( assignableEntity, position ), delayed );
		}

		public void RemoveEntity( object entity, bool delayed = true )
		{
			if ( workspace == null )
				return;

			var assignableEntity = MakeAssignableEntities( new[] { entity }, selectedBackendType ).FirstOrDefault();
			Exec( () => workspace.RemoveEntity( assignableEntity ), delayed );
		}

		internal void ExpandEntity( object entity, bool delayed = true )
		{
			if ( workspace != null )
				Exec( () => workspace.ExpandEntity( entity ), delayed );
		}

		internal void FoldEntity( object entity, bool delayed = true )
		{
			if ( workspace != null )
				Exec( () => workspace.FoldEntity( entity ), delayed );
		}

		public void InitRelation( object[] sourceEntities, bool delayed = true )
		{
			if ( workspace != null )
				Exec( () => workspace.CreateRelation( sourceEntities ), delayed );
		}

		public object[] FindRelations( object entity )
		{
			if ( workspace == null )
				return new object[ 0 ];

			return workspace.FindRelations( entity );
		}

		public void AddRelation( object sourceEntity, object targetEntity, object tag, bool delayed = true )
		{
			if ( workspace != null )
				Exec( () => workspace.AddRelation( sourceEntity, targetEntity, tag ), delayed );
		}

		public void RemoveRelation( object sourceEntity, object targetEntity, object tag, bool delayed = false )
		{
			if ( workspace != null )
				Exec( () => workspace.RemoveRelation( sourceEntity, targetEntity, tag ), delayed );
		}

		// enforce backend selection
		public void SetBackend( Type backendType, bool delayed = true )
		{
			if ( !BackendTypeUtil.IsBackendType( backendType ) )
				throw new ArgumentException( backendType + " is not a valid backend type." );

			targetHistory.RegisterBackendChange( backendType );
			Exec( () => OnSelectBackend( backendType ), delayed );
		}

		public void SelectEntityNodes( System.Predicate<object> doSelect, bool delayed = true )
		{
			if ( workspace != null )
				Exec( () => workspace.SelectEntityNodes( doSelect ), delayed );
		}

		public void SendEvent( Event e )
		{
			if ( workspace != null )
				workspace.OnEvent( e );
		}

		void Exec( Action action, bool delayed = true )
		{
			if ( delayed )
				ExecDelayed( action );
			else
				action.Invoke();
		}

		void OnSelectBackend( Type backendType )
		{
			selectedBackendType = backendType;
			InitWorkspace();
		}

		bool InitWorkspace()
		{
			if ( workspace != null )
				workspace.OnDestroy();

			if ( selectedBackendType == null )
			{
				workspace = null;
				return false;
			}

			workspace = CreateWorkspace();
			return ( workspace != null );
		}

		IWorkspace CreateWorkspace()
		{
			var assignableTargets = MakeAssignableEntities( targetObjects, selectedBackendType );

			object[] targetArray = assignableTargets.Any() ?
				assignableTargets.ToArray() :
				targetObjects != null ?
				new object[] { } :
				null; // make sure to pass null, NOT an empty array

			var ctorArguments = new object[]
			{
				selectedBackendType,
				targetArray,
				(GetAPI)window.GetAPI,
				(Action)window.Repaint,
				(Action<Action>)window.ExecOnUpdate
			};

			var backendArguments = BackendTypeUtil.GetGenericArguments( selectedBackendType );

			return (IWorkspace) Activator.CreateInstance
				(
					typeof( Workspace<,> ).MakeGenericType( backendArguments ),
					System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
					null,
					ctorArguments,
					null
				);
		}

		internal void AddTargetObjects( object[] targetsToAdd )
		{
			AddTargetObjects( targetsToAdd, Vector2.zero );
		}

		internal void AddTargetObjects( object[] targetsToAdd, Vector2 pos )
		{
			if ( targetsToAdd == null )
			{
				Log.Error( "No targets to add." );
				return;
			}

			// filter null targets
			var newTargets = targetsToAdd.Where( x => !Util.IsBadRef( x ) );

			if ( targetObjects == null )
			{
				SetTargetObjects( newTargets );
				return;
			}

			// allow user to abort if the node-count exceeds the limit
			var combined = targetObjects.Union( newTargets ).ToHashSet();
			if ( !UserAcceptsTargetCount( combined.Count() ) )
				return;

			targetObjects = combined;
			var entitiesToAdd = MakeAssignableEntities( newTargets, selectedBackendType );
			workspace.AddTargets( entitiesToAdd.ToArray(), pos );
			UpdateBackend();
		}

		internal void SetTargetObjects( IEnumerable<object> targets, Type backendType = null )
		{
			if ( targets == null )
				targetObjects = null;
			else
				targetObjects = new HashSet<object>( targets.Where( o => !Util.IsBadRef( o ) ) );
			UpdateBackend();

			if(backendType != null )
				selectedBackendType = backendType;

			InitWorkspace();
		}

		void UpdateBackend()
		{
			validBackendTypes = GetValidBackendTypes( targetObjects, allBackendTypes ).ToList();

			if ( !validBackendTypes.Contains( selectedBackendType ) )
				selectedBackendType = BackendTypeUtil.GetMostSpecificBackendType( validBackendTypes );
		}

		static IEnumerable<Type> GetValidBackendTypes( IEnumerable<object> targetEntities, IEnumerable<Type> backendTypes )
		{
			if ( targetEntities == null || !targetEntities.Any() )
				return backendTypes;

			var entityTypes = TypeUtil.GetValidEntityTypes( targetEntities );

			var autoBackendTypes = BackendTypeUtil.CreateAutoBackendTypes( entityTypes );

			var matchingBackendTypes = backendTypes
				.Where( t => !t.IsGenericType )
				.Where( backendType =>
						BackendTypeUtil.IsEntityTypeAssignableFromAny( backendType, entityTypes ) ||
						BackendTypeUtil.BackendAttributeFitsAny( backendType, entityTypes ) );

			return autoBackendTypes.Concat( matchingBackendTypes );
		}

		internal void OnSelectionChange()
		{
			if ( workspace != null )
				workspace.OnSelectionChange();
		}

		internal void OnDestroy()
		{
			if ( workspace != null )
				workspace.OnDestroy();
		}

		internal void OnWorkspaceGUI( Rect drawRect )
		{
			if ( workspace != null )
				workspace.OnGUI( drawRect );
		}

		internal Rect DrawWorkspaceControls()
		{
			if ( workspace != null )
				return workspace.OnControlsGUI();

			return GUILayoutUtility.GetRect( 0, 0, new[] { GUILayout.ExpandWidth( true ), GUILayout.ExpandHeight( true ) } );
		}

		void LoadHistoryState( object[] targets, Type backendType )
		{
			Exec(
			   () =>
			   {
				   // don't use SetBackend or ResetTargets, they would add this change to the history
				   selectedBackendType = backendType;
				   SetTargetObjects( targets );
			   } );
		}

		internal void DrawToolbar()
		{
			EditorGUILayout.BeginHorizontal( EditorStyles.toolbar );

			// target history navigation
			targetHistory.OnGUI( LoadHistoryState );

			// clear
			GUI.enabled = targetObjects != null;
			if ( GUILayout.Button( clearButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
				Exec( () => SetTargetObjects( null ) );
			GUI.enabled = true;

			// re-create the workspace from targets
			GUI.enabled = targetObjects != null && targetObjects.Any();
			if ( GUILayout.Button( rebuildButtonContent, EditorStyles.toolbarButton, GUILayout.ExpandWidth( false ) ) )
				InitWorkspace();
			GUI.enabled = true;

			GUILayout.FlexibleSpace();

			// backend selector
			string backendSelectText = ( selectedBackendType != null ) ?
				BackendTypeUtil.GetTitle( selectedBackendType ) :
				"Select graph type";

			var backendSelectContent = new GUIContent( backendSelectText, null, "Select graph type" );
			var backendSelectButtonRect = GUILayoutUtility.GetRect( backendSelectContent, EditorStyles.toolbarDropDown, GUILayout.ExpandWidth( false ) );

			if ( GUI.Button( backendSelectButtonRect, backendSelectContent, EditorStyles.toolbarDropDown ) )
			{
				var window = EditorWindow.CreateInstance<BackendSelectWindow>();
				window.backendTypes = validBackendTypes.ToArray();
				window.selectedBackendType = selectedBackendType;
				window.OnSelectBackend = ( newSelection ) =>
				{
					// don't save constructed types (because we can't deserialize them yet)
					if ( !newSelection.GetGenericArguments().Any() )
						GUIUtil.SetPrefsBackendType( PrefsKeyDefaultBackend, newSelection );
					SetBackend( newSelection );
				};

				var size = new Vector2( 340, 150 );
				window.minSize = window.maxSize = size;
				window.ShowAsDropDown( GUIUtil.GUIToScreenRect( backendSelectButtonRect ), size );
			}

			GUILayout.FlexibleSpace();

			// workspace toolbar
			if ( workspace != null )
				workspace.OnToolbarGUI();

			GUILayout.FlexibleSpace();

			// setttings menu
			if ( GUILayout.Button( new GUIContent( SkinManager.GetSkin().settingsIcon, "Settings" ), EditorStyles.toolbarButton, GUILayout.Width( 25 ) ) )
				SettingsMenu.Create();

			EditorGUILayout.EndHorizontal();
		}

		internal void Update()
		{
			if ( workspace != null )
				workspace.Update();
		}

		internal void HandleEvent( Event ev, Rect windowPosition )
		{
			switch ( ev.type )
			{
				case EventType.DragUpdated:
					{
						bool controlHeld = ( ev.modifiers & EventModifiers.Control ) != 0;
						DragAndDrop.visualMode = controlHeld ? DragAndDropVisualMode.Generic : DragAndDropVisualMode.Move;
						break;
					}

				case EventType.DragPerform:
					{
						bool controlHeld = ( ev.modifiers & EventModifiers.Control ) != 0;
						// if control is held down, add to the existing targets
						// else replace them
						bool doAddObjects = controlHeld;

						var dragObjs = GetDragObjects();

						if ( doAddObjects )
							AddTargets( dragObjs, Event.current.mousePosition );
						else
							ResetTargets( dragObjs );

						DragAndDrop.AcceptDrag();
						ev.Use();
						break;
					}

				case EventType.Repaint:
					if ( DragAndDrop.visualMode == DragAndDropVisualMode.Generic || DragAndDrop.visualMode == DragAndDropVisualMode.Move )
					{
						var bgColor = SkinManager.GetSkin().windowColor;
						Util.FadeRect( windowPosition.ResetOrigin(), bgColor );
					}
					break;
			}
		}

		static Object[] GetDragObjects()
		{
			Object[] objs = DragAndDrop.objectReferences;
			if ( objs != null )
				return objs.ToArray();  // copy the array

			return new Object[ 0 ];
		}

		static IEnumerable<object> MakeAssignableEntities( IEnumerable<object> objs, Type backendType )
		{
			if ( objs == null )
				yield break;

			Type acceptedType = BackendTypeUtil.BackendAttrType( backendType );
			Type entityType = BackendTypeUtil.GetGenericArguments( backendType )[ 0 ];

			foreach ( var obj in objs )
			{
				if ( acceptedType != null && acceptedType.IsAssignableFrom( obj.GetType() ) )
				{
					yield return obj;
				}
				else
				{
					yield return TypeUtil.MakeAssignable( obj, entityType );
				}
			}
		}
	}
}
