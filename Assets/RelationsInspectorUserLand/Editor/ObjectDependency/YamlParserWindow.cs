using UnityEngine;
using UnityEditor;
using System.Collections;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class YamlParserWindow : EditorWindow
	{
		public string yaml;
		Vector2 scrollPos;

		void OnGUI()
	{
		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		{
			if (GUILayout.Button("parse & log"))
			{				
				var docs = YAMLUtil.ParseYaml(yaml);
				foreach (var doc in docs)
				{
					Debug.Log("doc " + doc.RootNode.Tag + " class " + doc.ClassName() + " is GO? " + doc.IsGameObject() );
					YAMLUtil.LogNode(doc.RootNode);
				}
			}

			if (GUILayout.Button("parse & analyse"))
			{
				AnalyseYAML(yaml);
			}

			if (GUILayout.Button("graph"))
			{
				var docs = YAMLUtil.ParseYaml( yaml );
				var objs = SerializedObjectParser.Parse(docs);

				var riAPI = GetWindow<RelationsInspectorWindow>() as RelationsInspectorAPI;
				riAPI.ResetTargets( objs.Where(obj => obj  is SerializedGameObject).ToArray() );
			}

			yaml = EditorGUILayout.TextArea(yaml);
		}
		EditorGUILayout.EndScrollView();
	}

		void AnalyseYAML(string text)
		{
			var docs = YAMLUtil.ParseYaml(text);

			var objs = SerializedObjectParser.Parse(docs);

			foreach (var obj in objs)
			{
				Debug.Log("obj: " + obj.ToString());
			}
		}


		[MenuItem("Window/YamlParser")]
		static void Spawn()
		{
			GetWindow<YamlParserWindow>();
		}
	}
}
