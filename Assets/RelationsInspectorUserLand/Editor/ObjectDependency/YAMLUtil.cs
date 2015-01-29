using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class YAMLUtil
	{

		public static void LogNode(YamlNode node)
		{
			Debug.Log("node " + node.GetType() + " anchor " + node.Anchor);

			if (node is YamlMappingNode)
			{
				Debug.Log("mapping");
				foreach (var child in (node as YamlMappingNode).Children)
				{
					Debug.Log("key");
					LogNode(child.Key);
					Debug.Log("value");
					LogNode(child.Value);
				}
				return;
			}

			if (node is YamlSequenceNode)
			{
				Debug.Log("seq");
				foreach (var child in (node as YamlSequenceNode).Children)
				{
					Debug.Log("item");
					LogNode(child);
				}
				return;
			}

			if (node is YamlScalarNode)
			{
				Debug.Log("scalar value " + (node as YamlScalarNode).Value);
				return;
			}
		}

		// parse yaml string into documents
		public static IEnumerable<YamlDocument> ParseYaml(string text)
		{
			// Setup the input
			var input = new System.IO.StringReader(text);

			// Load the stream
			var yaml = new YamlStream();
			try
			{
				yaml.Load(input);
			}
			catch (System.Exception e)
			{
				Debug.LogException(e);
				return Enumerable.Empty<YamlDocument>();
			}
			return yaml.Documents;
		}

		public static string GetString(YamlNode node)
		{
			return (node as YamlScalarNode).Value;
		}

		public static int GetInt(YamlNode node)
		{
			return int.Parse(GetString(node));
		}

		/*public static IEnumerable<SerializedObjectId> FindObjectReferences(YamlDocument doc)
		{
			return FindObjectReferences(doc.RootNode);
		}

		public static IEnumerable<SerializedObjectId> FindObjectReferences(YamlScalarNode node)
		{
			yield break;
		}

		public static IEnumerable<SerializedObjectId> FindObjectReferences(YamlMappingNode node)
		{
		}*/


		//public static IEnumerable<UnityObjectReference> FindObjectReferences(IEnumerable<YamlDocument> docs)
		//{
		//	var refs = docs.SelectMany(doc => FindObjectReferences(docs));
		//}

		// find all object references in the docment
		public static IEnumerable<UnityObjectReference> FindObjectReferences(YamlDocument doc)
		{
			var root = doc.RootNode;
			string docClassName = doc.ClassName();
			int docFileId = doc.FileId();
			string docObjName = FindObjectName(doc) ?? docClassName;
			foreach (var uoref in FindObjectReferences(root))
			{
				var sceneObj = new YamlSceneObject(docClassName, docObjName, docFileId);
				uoref.path.Add( sceneObj );
				yield return uoref;
			}
		}

		static string FindObjectName(YamlDocument doc)
		{
			var rootMapping = (doc.RootNode as YamlMappingNode).Children;
			var mapEntries = (rootMapping.First().Value as YamlMappingNode).Children;
			foreach (var entry in mapEntries)
			{
				var keyAsScalar = entry.Key as YamlScalarNode;
				if (keyAsScalar != null && keyAsScalar.Value == "m_Name")
					return (entry.Value as YamlScalarNode).Value;
			}
			return null;
		}

		// returns an object reference if the node represents one
		// has to contain a fileID key
		// may contain a guid key
		public static SerializedObjectId GetObjectRef(YamlMappingNode map)
		{
			if (map == null)
				return null;

			var fileIdStr = FindScalarPairValue(map, "fileID");
			var guidStr = FindScalarPairValue(map, "guid");
			var typeStr = FindScalarPairValue(map, "type");

			if (fileIdStr == null)
				return null;

			int fileId = int.Parse(fileIdStr);
			int type = (typeStr == null) ? 0 : int.Parse(typeStr);
			string guid = guidStr ?? string.Empty;

			return new SerializedObjectId(fileId, guid, type);
		}

		public static SerializedObjectId GetObjectRef(YamlNode node)
		{
			return GetObjectRef(node as YamlMappingNode);
		}

		static string FindScalarPairValue(YamlMappingNode map, string keyValue)
		{
			var thePair = map.Where(pair => IsScalarString(pair.Key, keyValue)).FirstOrDefault();
			if (thePair.Equals(default(KeyValuePair<YamlNode, YamlNode>)))
				return null;

			var valueAsScalar = thePair.Value as YamlScalarNode;
			if (valueAsScalar == null)
				return null;

			return valueAsScalar.Value;
		}

		public static IEnumerable<UnityObjectReference> FindObjectReferences(YamlNode node)
		{
			var asScalar = node as YamlScalarNode;
			if (asScalar != null)
				yield break;

			var asSeq = node as YamlSequenceNode;
			if (asSeq != null)
			{
				foreach (var child in asSeq.Children)
				{
					foreach (var ouref in FindObjectReferences(child))
					{
						//ouref.path.Add(new YamlSceneObject() { name = "x", className = "y" });
						yield return ouref;
					}
				}
				yield break;
			}

			var asMap = node as YamlMappingNode;
			if (asMap != null)
			{
				var objRef = GetObjectRef(asMap);
				var uoref = new UnityObjectReference(objRef);
				if (uoref != null)
				{
					yield return uoref;
					yield break;
				}

				foreach (var child in asMap.Children)
				{
					foreach (var childUORef in FindObjectReferences(child.Value))
					{
						//childUORef.path.Add(new YamlSceneObject() { name = "x", className = "y" });
						yield return childUORef;
					}
				}
				yield break;
			}
		}

		public static string GetScalarString(YamlNode node)
		{
			var asScalar = node as YamlScalarNode;
			if (asScalar == null)
				return string.Empty;
			return asScalar.Value;
		}


		static bool IsScalarString(YamlNode node, string value)
		{
			var asScalar = node as YamlScalarNode;
			return asScalar != null && asScalar.Value == value;
		}

		public static void ProcessVariables(YamlDocument document, System.Action<string, YamlNode> action )
		{
			// root node is mapping
			var rootMapping = (document.RootNode as YamlMappingNode).Children;

			// rootMapping is CLASSNAME : mapping of variable names to their values
			var objectVariables = (rootMapping.Single().Value as YamlMappingNode).Children;
			foreach (var entry in objectVariables)
			{
				// invoke action with variable name and value
				action.Invoke( GetString(entry.Key), entry.Value );
			}
		}

		public static IEnumerable<SerializedGameObject> GetSceneGameObjects(string sceneAssetPath)
		{
			var objs = GetSceneObjects(sceneAssetPath);
			return objs
				.Select(obj => obj as SerializedGameObject)
				.Where(x => x != null);
		}

		public static IEnumerable<SerializedObject> GetSceneObjects(string sceneAssetPath)
		{
			if (!System.IO.File.Exists(sceneAssetPath))
				yield break;

			// file -> text
			string text = System.IO.File.ReadAllText(sceneAssetPath);

			// text -> docs
			var docs = YAMLUtil.ParseYaml(text);

			// docs -> objects
			var sceneObj = CreateSceneObject(sceneAssetPath);
			var objs = SerializedObjectParser.Parse(docs, sceneObj);

			foreach (var obj in objs)
				yield return obj;
		}

		public static SerializedObject CreateSceneObject(string path)
		{
			int classId = YamlClassIdDictionary.ClassIdScene;
			int fileId = 0;
			string name = System.IO.Path.GetFileName(path);

			return new SerializedObject(classId, fileId, name);
		}

		public static SerializedObjectId GetObjectId(Object obj)
		{
			string assetPath = UnityEditor.AssetDatabase.GetAssetPath(obj);
			string guid = UnityEditor.AssetDatabase.AssetPathToGUID(assetPath);
			int fileId = FileIdHelper.GetFileId(obj);
			return new SerializedObjectId(fileId, guid, 0);
		}
	}

	public class UnityObjectReference
	{
		public SerializedObjectId objectId;
		public List<YamlSceneObject> path;
		private SerializedObjectId objRef;

		/*public UnityObjectReference(int fileId, string guid)
		{
			this.objectId = new SerializedObjectId(fileId, guid);
			this.path = new List<YamlSceneObject>();
		}*/

		public UnityObjectReference(SerializedObjectId objRef)
		{
			this.objectId = objRef;
			this.path = new List<YamlSceneObject>();
		}

		public override string ToString()
		{
			string combinedPath = path.Aggregate("", (res, obj) => res += obj.name + " " + obj.className);
			return string.Format("{0} path {1}", objectId.ToString(), combinedPath);
		}
	}

	public class YamlSceneObject
	{
		public string name {get; private set;}
		public string className {get; private set;}
		public int anchor { get; private set; }

		public YamlSceneObject(string name, string className, int anchor)
		{
			this.name = name;
			this.className = className;
			this.anchor = anchor;
		}
	}
}
