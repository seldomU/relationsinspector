using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Text;
using RelationsInspector.Extensions;

namespace RelationsInspector
{
    class RIState
    {
        public object[] targets;
        public Type backendType;

        public RIState(IEnumerable<object> targets, Type backendType)
        {
            this.targets = targets.ToArray();
            this.backendType = backendType;
        }

        public override string ToString()
        {
            return targets.ToDelimitedString() + " " + backendType.Name;
        }
    }

    class RIStateHistory
    {
        List<RIState> stateHistory = new List<RIState>();
        int pointer;

        public void RegisterState(IEnumerable<object> targets, Type backendType)
        {
            if (targets == null || !targets.Any())
                return;

            // remove anything after pointer
            if( stateHistory.Count > pointer+1)
            stateHistory.RemoveRange(pointer + 1, stateHistory.Count - 1 - pointer);

            stateHistory.Add( new RIState(targets, backendType) );
            pointer = stateHistory.Count - 1;
        }

        public void RegisterBackendChange(Type backendType)
        {
            if (!stateHistory.Any())
                return;

            RegisterState(stateHistory[pointer].targets, backendType);
        }

        public bool HasPrev()
        {
            return pointer > 0;
        }

        public RIState GetPreviousState ()
        {
            pointer--;
            return GetTargetsAtPointer();
        }

        public bool HasNext()
        {
            return pointer < stateHistory.Count - 1;
        }

        public RIState GetNextState ()
        {
            pointer++;
            return GetTargetsAtPointer();
        }

        RIState GetTargetsAtPointer()
        {
            if (!stateHistory.Any())
                throw new Exception("There is no state history");

            pointer = Mathf.Clamp(pointer, 0, stateHistory.Count - 1);
            return stateHistory[pointer];
        }

        public void OnGUI( Action<object[], Type> setTargets)
        {
            bool backButtonEnabled = pointer > 0;
            bool nextButtonEnabled = pointer < stateHistory.Count - 1;
            bool contextEnabaled = GUI.enabled;

            GUI.enabled = backButtonEnabled;
            if (GUILayout.Button( new GUIContent( SkinManager.GetSkin().prevIcon, "Back to previous graph" ), EditorStyles.toolbarButton))
            {
                var state = GetPreviousState();
                setTargets(state.targets, state.backendType);
            }

            GUI.enabled = nextButtonEnabled;
            if (GUILayout.Button( new GUIContent( SkinManager.GetSkin().nextIcon, "Forward to next graph" ), EditorStyles.toolbarButton))
            {
                var state = GetNextState();
                setTargets(state.targets, state.backendType);
            }

            GUI.enabled = contextEnabaled;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            foreach(var state in stateHistory)
            {
                sb.AppendLine(state.ToString());
            }
            sb.AppendLine("pointer at pos" + pointer);
            return sb.ToString();
        }
    }
}
