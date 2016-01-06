using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
	struct RIState
	{
		public HashSet<object> targets;
		public Type backendType;

		public RIState( IEnumerable<object> targets, Type backendType )
		{
			this.targets = targets.ToHashSet();
			this.backendType = backendType;
		}

		public override string ToString()
		{
			return targets.ToDelimitedString() + " " + backendType.Name;
		}

		public override bool Equals( object obj )
		{
			if ( !( obj is RIState ) )
				return false;

			var other = (RIState) obj;
			return targets.SetEquals( other.targets ) && backendType == other.backendType;
		}

		public override int GetHashCode()
		{
			int hash = 23;
			hash = hash * 17 + backendType.GetHashCode();
			foreach ( var t in targets )
				hash = hash * 17 + t.GetHashCode();
			return hash;
		}
	}

	class RIStateHistory
	{
		List<RIState> stateHistory = new List<RIState>();
		int pointer;

		public void RegisterState( IEnumerable<object> targets, Type backendType )
		{
			if ( targets == null || !targets.Any() )
				return;

			// remove anything after pointer
			if ( stateHistory.Count > pointer + 1 )
				stateHistory.RemoveRange( pointer + 1, stateHistory.Count - 1 - pointer );

			var state = new RIState( targets, backendType );
			stateHistory.RemoveWhere( x => x.Equals( state ) );
			stateHistory.Add( state );
			pointer = stateHistory.Count - 1;
		}

		public void RegisterBackendChange( Type backendType )
		{
			if ( !stateHistory.Any() )
				return;

			RegisterState( stateHistory[ pointer ].targets, backendType );
		}

		public bool HasPrev()
		{
			return pointer > 0;
		}

		public RIState GetPreviousState()
		{
			pointer--;
			return GetTargetsAtPointer();
		}

		public bool HasNext()
		{
			return pointer < stateHistory.Count - 1;
		}

		public RIState GetNextState()
		{
			pointer++;
			return GetTargetsAtPointer();
		}

		RIState GetTargetsAtPointer()
		{
			if ( !stateHistory.Any() )
				throw new Exception( "There is no state history" );

			pointer = Mathf.Clamp( pointer, 0, stateHistory.Count - 1 );
			return stateHistory[ pointer ];
		}

		public void OnGUI( Action<object[], Type> setTargets )
		{
			bool backButtonEnabled = pointer > 0;
			bool nextButtonEnabled = pointer < stateHistory.Count - 1;
			bool contextEnabaled = GUI.enabled;

			GUI.enabled = backButtonEnabled;
			if ( GUILayout.Button( new GUIContent( SkinManager.GetSkin().prevIcon, "Back to previous graph" ), EditorStyles.toolbarButton ) )
			{
				var state = GetPreviousState();
				setTargets( state.targets.ToArray(), state.backendType );
			}

			GUI.enabled = nextButtonEnabled;
			if ( GUILayout.Button( new GUIContent( SkinManager.GetSkin().nextIcon, "Forward to next graph" ), EditorStyles.toolbarButton ) )
			{
				var state = GetNextState();
				setTargets( state.targets.ToArray(), state.backendType );
			}

			GUI.enabled = contextEnabaled;
		}

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			foreach ( var state in stateHistory )
			{
				sb.AppendLine( state.ToString() );
			}
			sb.AppendLine( "pointer at pos" + pointer );
			return sb.ToString();
		}
	}
}
