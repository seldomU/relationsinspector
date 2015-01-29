using UnityEngine;
using System.Collections;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;

namespace RelationsInspector.Backend.ObjectDependency
{
	// goes down the document tree and collects all object references
	public class SerializedObjectRefAnalyser : IYamlVisitor
	{
		public List<SerializedObjectId> references = new List<SerializedObjectId>();
		
		public void Visit(YamlDocument document)
		{
			document.RootNode.Accept(this);
		}

		public void Visit(YamlMappingNode mapping)
		{
			// see if this is a reference
			var objRef = YAMLUtil.GetObjectRef(mapping);
			if (objRef != null)
			{
				references.Add(objRef);
				return;
			}

			// go down further
			foreach (var child in mapping.Children)
			{
				child.Key.Accept(this);
				child.Value.Accept(this);
			}
		}

		public void Visit(YamlScalarNode scalar)
		{
			// nothting
		}

		public void Visit(YamlSequenceNode sequence)
		{
			foreach (var child in sequence.Children)
				child.Accept(this);
		}

		public void Visit(YamlStream stream)
		{
			// nothing
		}
	}
}
