using UnityEngine;
using System.Collections;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Backend.ObjectDependency
{
	// goes down the document tree and collects all object references
	public class SerializedGameObjectRefAnalyser : IYamlVisitor
	{
		public List<SerializedObjectId> components { get; private set; }
		public SerializedObjectId transform { get; private set; }
		public SerializedObjectId prefab { get; private set; }
		public string name { get; private set; }

		//public List<SerializedObjectId> references = new List<SerializedObjectId>();
		public SerializedGameObjectRefAnalyser()
		{
			components = new List<SerializedObjectId>();
		}
		
		public void Visit(YamlDocument document)
		{
			YAMLUtil.ProcessVariables(document, ReadVariable);
		}

		void ReadVariable(string varName, YamlNode varValue)
		{
			switch (varName)
			{
				case "m_Component":
					ReadComponents(varValue);
					break;
				case "m_PrefabParentObject":
					prefab = YAMLUtil.GetObjectRef(varValue as YamlMappingNode);
					break;
				case "m_Name":
					name = YAMLUtil.GetScalarString(varValue);
					break;
				default:
					break;
			}
		}

		void ReadComponents(YamlNode node)
		{
			/*
				example...
				m_Component:
				  - 4: {fileID: 1795780353}
			*/
			// we expect a sequence of single-entry mappings that map an id to a reference mapping
			var asSeq = node as YamlSequenceNode;
			if (asSeq == null)
				return;

			foreach (var seqEntry in asSeq.Children)
			{
				var asMapping = seqEntry as YamlMappingNode;
				if (asMapping == null)
					continue;

				// mapping contains only a single pair
				var componentEntry = asMapping.Single();

				var reference = YAMLUtil.GetObjectRef(componentEntry.Value as YamlMappingNode);
				if (reference == null)
					continue;

				components.Add(reference);

				if (YAMLUtil.GetInt(componentEntry.Key) == YamlClassIdDictionary.ClassIdTransform)
					transform = reference;
			}

			// find transform
		}

		// document visitor handles everything, no need for the others
		public void Visit(YamlMappingNode mapping){}
		public void Visit(YamlScalarNode scalar){}
		public void Visit(YamlSequenceNode sequence){}
		public void Visit(YamlStream stream){}
	}
}
