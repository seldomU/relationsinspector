using UnityEngine;
using System.Collections;
using YamlDotNet.RepresentationModel;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class SerializedTransform : SerializedObject
	{
		public SerializedGameObject gameObject { get; private set; }
		public SerializedTransform parent { get; private set; }
		public List<SerializedTransform> children { get; private set; }

		public SerializedTransform(int classId, int fileId): base(classId, fileId) { }

		public override void Parse(YamlDocument doc, Dictionary<int, SerializedObject> objectByFileId)
		{
			var scanner = new SerializedTransformRefAnalyser();
			doc.Accept(scanner);

			gameObject = objectByFileId[scanner.gameObject.fileId] as SerializedGameObject;
			if(scanner.parent.fileId != 0)
				parent = objectByFileId[scanner.parent.fileId] as SerializedTransform;
			children = scanner.children.Select(child => objectByFileId[child.fileId] as SerializedTransform).ToList();
		}

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine(GetType().ToString());
			sb.AppendFormat("fileID {0} classId {1} class {2}", fileID, classId, YamlClassIdDictionary.GetClassName(classId));
			sb.AppendLine();

			sb.AppendLine("gameObject " + ObjFileId(gameObject));
			sb.AppendLine("parent " + ObjFileId(parent));

			foreach (var child in children)
			{
				sb.AppendLine("child " + ObjFileId(child) );
			}
			sb.AppendLine("owner " + owner);
			return sb.ToString();
		}

		string ObjFileId(SerializedObject obj)
		{
			return obj == null ? "null" : obj.fileID.ToString();
		}
	}
}
