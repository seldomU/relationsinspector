using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using RelationsInspector;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.BenchmarkTool
{
    [SaveLayout]
    public class BenchmarkBackend : MinimalBackend<Number, NumberRelation>
	{
		public override IEnumerable<Relation<Number, NumberRelation>> GetRelations(Number item)
		{
			return EditorWindow.GetWindow<BenchmarkWindow>().GetRelated(item);
		}
	}
}
