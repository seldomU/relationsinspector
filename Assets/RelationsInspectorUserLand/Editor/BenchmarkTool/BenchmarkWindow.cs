using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector;

// test graphs

// todo: 
// add branchingFactor distribution params, so we can have non-uniform distributions
// add tree height distribution param

namespace RelationsInspector.Backend.BenchmarkTool
{
	public class BenchmarkWindow : EditorWindow
	{
		// params bound to gui controls
		// tree controls
		public int treeNumItems;
		public int treeBranchingFactor;
		// DAG controls
		public int dagNumItems;
		public int dagBranchingFactor;

		RelationsInspectorAPI relationsInspectorAPI;
		Dictionary<Number, IList<Tuple<Number, NumberRelation>>> explicitRelations;

		Editor selfEditor;

		void OnEnable()
		{
			relationsInspectorAPI = GetWindow<RelationsInspectorWindow>() as RelationsInspectorAPI;
			selfEditor = Editor.CreateEditor(this);
		}

		void OnGUI()
		{
			// tree test controls
			GUILayout.Label("Params", EditorStyles.boldLabel);
			selfEditor.OnInspectorGUI();

			if (GUILayout.Button("make tree"))
			{
				explicitRelations = MakeTreeRelations(treeNumItems, treeBranchingFactor);
				relationsInspectorAPI.ResetTargets(explicitRelations.Keys.ToArray());
			}

			if (GUILayout.Button("make DAG"))
			{
				explicitRelations = MakeDAGRelations(dagNumItems, dagBranchingFactor);
				relationsInspectorAPI.ResetTargets(explicitRelations.Keys.ToArray());
			}
		}

		static Dictionary<Number, IList<Tuple<Number, NumberRelation>>> MakeDAGRelations(int numItems, int branchingFactor)
		{
			var dict = new Dictionary<Number, IList<Tuple<Number, NumberRelation>>>();
			var items = Enumerable.Range(0, numItems).Select(i => new Number() { value = i }).ToArray();

			for (int i = 0; i < numItems; i++)
			{
				// find _branchingFactor_ other items (may include self)
				int numLinks = Mathf.Min(branchingFactor, numItems);
				var linkIds = Util.Shuffle(numItems).Take(numLinks);
				var linkRelations = linkIds.Select(id => new Tuple<Number, NumberRelation>(items[id], NumberRelation.Unspecified));
				dict[items[i]] = linkRelations.ToList();
			}
			return dict;
		}

		static Dictionary<Number, IList<Tuple<Number, NumberRelation>>> MakeTreeRelations(int numItems, int branchingFactor)
		{
			var dict = new Dictionary<Number, IList<Tuple<Number, NumberRelation>>>();
			var items = Enumerable.Range(0, numItems).Select(i => new Number() { value = i }).ToArray();

			for (int i = 0; i < numItems; i++)
			{
				// take up to _branchingFactor_ items, clamp to bounds
				int firstRelationObjectId = Mathf.Min(i * branchingFactor + 1, numItems);
				int numRelationObjects = Mathf.Min(branchingFactor, numItems - firstRelationObjectId);
				var linkIds = Enumerable.Range(firstRelationObjectId, numRelationObjects);
				var relations = linkIds.Select(id => new Tuple<Number, NumberRelation>(items[id], NumberRelation.Unspecified));

				dict[items[i]] = relations.ToList();
			}
			return dict;
		}

		public IEnumerable<Tuple<Number, NumberRelation>> GetRelated(Number item)
		{
			return explicitRelations[item];
		}

		[MenuItem("Window/number test")]
		static void Spawn()
		{
			GetWindow<BenchmarkWindow>();
		}
	}
}
