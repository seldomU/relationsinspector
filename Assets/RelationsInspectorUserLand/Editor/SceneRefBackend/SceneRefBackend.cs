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
	
	class VisualNode
	{
		public string label { get; private set; }
		public VisualNode(string label) { this.label = label; }
	}

	[RelationsInspector(typeof(Object))]
	class SceneRefBackend : MinimalBackend<VisualNode, string>
	{
		static Color targetNodeColor = Color.green;
		
		// linking objects to the ones that depend on them
		Dictionary<VisualNode, HashSet<VisualNode>> dependencyGraph;

		// nodes representing the target object. we want to mark them visually
		HashSet<VisualNode> targetNodes;

		// root directory for scene asset search
		string sceneDirPath = Application.dataPath;
		
		public override IEnumerable<VisualNode> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
		{
			this.api = api;
			if (targets == null || !targets.Any())
				yield break;

			var targetObjs = new HashSet<Object>( BackendUtil.Convert<Object>(targets) );
			if (!targetObjs.Any())
				yield break;

			// get all scene files
			var sceneFilePaths = Directory.GetFiles(sceneDirPath, "*.unity", SearchOption.AllDirectories);

			dependencyGraph = new Dictionary<VisualNode, HashSet<VisualNode>>();
			foreach (var path in sceneFilePaths)
			{
				// get the dependency graph in terms of objects
				var objDependencyGraph = ObjectDependencyUtil.GetDependencyGraph(path, targetObjs);

				// convert it to a VisualNode graph
				string fileName = System.IO.Path.GetFileName(path);
				var visualGraph = Convert(objDependencyGraph, obj => GetVisualNodeName(obj, targetObjs, fileName));

				// merge it with the other scene's graphs
				ObjectDependencyUtil.AddGraph<VisualNode>(dependencyGraph, visualGraph);
			}

			// find the nodes being referenced
			var referencedNodes = dependencyGraph.Values.SelectMany(set=>set);

			// find the target nodes. they are ones not referencing anything
			targetNodes = new HashSet<VisualNode>( referencedNodes.Except(dependencyGraph.Keys) );

			// find the scene root nodes in the graph
			var rootNodes = dependencyGraph.Keys.Except(referencedNodes);
			foreach (var rootNode in rootNodes)
				yield return rootNode;
		}

		// get an object label. for target objects, we add the scene's name
		static string GetVisualNodeName(Object obj, HashSet<Object> targetObjs, string sceneName)
		{
			return targetObjs.Contains(obj) ? obj.name + "\nscene " + sceneName : obj.name;
		}

		// turn object graph into VisualNode graph (mapping obj -> name)
		static Dictionary<VisualNode, HashSet<VisualNode>> Convert(Dictionary<Object, HashSet<Object>> objGraph, System.Func<Object,string> getObjectName)
		{
			// get all graph objects
			var allObjs = new HashSet<Object>( objGraph.Keys).Union( objGraph.Values.SelectMany(set => set) );

			// map them to VisualNodes
			var objToNode = allObjs.ToDictionary(obj => obj, obj => new VisualNode( getObjectName(obj) ) );
			
			// convert graph dictionary
			return objGraph.ToDictionary( pair => objToNode[pair.Key], pair => new HashSet<VisualNode>( pair.Value.Select( obj => objToNode[obj]) ) );
		}

		public override IEnumerable<VisualNode> GetRelatedEntities(VisualNode entity)
		{
			if (dependencyGraph.ContainsKey(entity))
				return dependencyGraph[entity];

			return Enumerable.Empty<VisualNode>();
		}

		public override GUIContent GetContent(VisualNode entity)
		{
			return new GUIContent(entity.label, entity.label);
		}

		// draw nodes representing scene root GameObject in a special color
		public override Rect DrawContent(VisualNode entity, EntityDrawContext drawContext)
		{
			if (!targetNodes.Contains(entity))
				return DrawUtil.DrawContent(GetContent(entity), drawContext);

			var colorBackup = drawContext.style.backgroundColor;
			drawContext.style.backgroundColor = targetNodeColor;
			var rect = DrawUtil.DrawContent(GetContent(entity), drawContext);
			drawContext.style.backgroundColor = colorBackup;
			return rect;
		}
	}
}
