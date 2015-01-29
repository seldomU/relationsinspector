using UnityEngine;
using System.Collections;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;

namespace RelationsInspector.Backend.ObjectDependency
{
	// goes down the document tree and collects all object references
	public class SerializedTransformRefAnalyser : IYamlVisitor
	{
		public List<SerializedObjectId> children = new List<SerializedObjectId>();
		public SerializedObjectId gameObject;
		public SerializedObjectId parent;


		public SerializedTransformRefAnalyser()
		{
			children = new List<SerializedObjectId>();
		}

		public void Visit(YamlDocument document)
		{
			YAMLUtil.ProcessVariables(document, ReadVariable);
		}

		void ReadVariable(string varName, YamlNode varValue)
		{
			switch (varName)
			{
				case "m_GameObject":
					gameObject = YAMLUtil.GetObjectRef(varValue);
					break;
				case "m_Father":
					parent = YAMLUtil.GetObjectRef(varValue);
					break;
				case "m_Children":
					ReadChildren(varValue);
					break;
			}
		}


		void ReadChildren(YamlNode node)
		{
			/*
			example... (list might be empty)
			m_Children:
				- {fileID: 1795780353}
			*/

			var asSeq = node as YamlSequenceNode;
			if (asSeq == null)
				return;

			foreach (var seqEntry in asSeq.Children)
			{
				var asMapping = seqEntry as YamlMappingNode;
				if (asMapping == null)
					continue;

				var reference = YAMLUtil.GetObjectRef(asMapping);
				if (reference == null)
					continue;

				children.Add(reference);
			}
		}

		// document visitor handles everything, no need for the others
		public void Visit(YamlMappingNode mapping){}
		public void Visit(YamlScalarNode scalar){}
		public void Visit(YamlSequenceNode sequence){}
		public void Visit(YamlStream stream){}
	}
}
