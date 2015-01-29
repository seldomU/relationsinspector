using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class SerializedGameObject : SerializedObject
	{
		public List<SerializedObject> components {get;private set;}
		public SerializedTransform transform {get;private set;}
		public SerializedObjectId prefab {get;private set;}
		
		public SerializedGameObject(int classId, int fileId): base(classId, fileId){}

		public override void Parse(YamlDocument doc, Dictionary<int, SerializedObject> objectsByFileId)
		{
			var scanner = new SerializedGameObjectRefAnalyser();
			doc.Accept(scanner);

			components = scanner.components.Select( objRef => objectsByFileId[objRef.fileId] ).ToList();
			transform = objectsByFileId[scanner.transform.fileId] as SerializedTransform;
			prefab = scanner.prefab;
			name = scanner.name;
		}

		public override void UpdateOwner(SerializedObject scene)
		{
			foreach (var component in components)
				component.SetOwner(this);

			foreach (var child in transform.children)
				child.gameObject.SetOwner(this);

			// if this GameObject has to parent, make it's owned by the scene
			if (transform.parent == null)
				SetOwner(scene);

		}

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine(GetType().ToString());
			sb.AppendFormat("fileID {0} classId {1} class {2}", fileID, classId, YamlClassIdDictionary.GetClassName(classId));
			sb.AppendLine();

			sb.AppendLine("prefab " + prefab ?? "null");
			sb.AppendLine("transform " + ObjFileId(transform) );
			
			foreach(var c in components)
			{
				sb.AppendLine("component " + c.GetType());
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
