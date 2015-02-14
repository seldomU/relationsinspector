using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend.SceneRefBackend
{
	using ObjNodeGraph = Dictionary<SceneObjectNode, HashSet<SceneObjectNode>>;

	[RelationsInspector(typeof(Object))]
	class SceneRefBackend : MinimalBackend<SceneObjectNode, string>
	{
		static Color targetNodeColor = new Color(0.29f, 0.53f, 0.28f);

		Object[] currentTargets;
		
		// linking objects to the ones they reference
		ObjNodeGraph referenceGraph;

		// nodes representing the target object. we want to mark them visually
		HashSet<SceneObjectNode> targetNodes;

		// root directory for scene asset search
		string sceneDirPath = Application.dataPath;
		
		public override IEnumerable<SceneObjectNode> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;
			if (targets == null || !targets.Any())
				yield break;

			var targetObjs = new HashSet<Object>( BackendUtil.Convert<Object>(targets) );
			if (!targetObjs.Any())
				yield break;

			currentTargets = targetObjs.ToArray();

			// get all scene files
			var sceneFilePaths = Directory.GetFiles(sceneDirPath, "*.unity", SearchOption.AllDirectories);

			referenceGraph = new ObjNodeGraph();
			foreach (var path in sceneFilePaths)
			{
				// get the reference graph
				var graph = ObjectDependencyUtil.GetReferenceGraph(path, targetObjs);

				// merge it with the other scene's graphs
				ObjectDependencyUtil.AddGraph<SceneObjectNode>(referenceGraph, graph);
			}

			// find the nodes being referenced
			var referencedNodes = referenceGraph.Values.SelectMany(set=>set);

			// find the target nodes. they are ones not referencing anything
			targetNodes = new HashSet<SceneObjectNode>( referencedNodes.Except(referenceGraph.Keys) );

			// find the scene root nodes in the graph
			var rootNodes = referenceGraph.Keys.Except(referencedNodes);
			foreach (var rootNode in rootNodes)
				yield return rootNode;
		}

		public override IEnumerable<SceneObjectNode> GetRelatedEntities(SceneObjectNode entity)
		{
			if (referenceGraph.ContainsKey(entity))
				return referenceGraph[entity];

			return Enumerable.Empty<SceneObjectNode>();
		}

		public override GUIContent GetContent(SceneObjectNode entity)
		{
			return new GUIContent(entity.label, entity.label);
		}

		// draw nodes representing scene root GameObject in a special color
		public override Rect DrawContent(SceneObjectNode entity, EntityDrawContext drawContext)
		{
			if (!targetNodes.Contains(entity))
				return DrawUtil.DrawContent(GetContent(entity), drawContext);

			var colorBackup = drawContext.style.backgroundColor;
			drawContext.style.backgroundColor = targetNodeColor;
			var rect = DrawUtil.DrawContent(GetContent(entity), drawContext);
			drawContext.style.backgroundColor = colorBackup;
			return rect;
		}

		public override Rect OnGUI()
		{
			return BackendUtil.GetMaxRect();
		}
	}
}
