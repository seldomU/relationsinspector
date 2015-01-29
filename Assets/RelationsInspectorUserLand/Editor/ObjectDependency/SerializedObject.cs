using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class SerializedObject
	{
		public int classId {get; protected set;}
		public int fileID { get; protected set; }
		public string name { get; protected set; }
		public List<SerializedObjectId> refs { get; protected set; }
		public SerializedObject owner { get; private set; }
		
		public SerializedObject(int classId, int fileId, string name=null)
		{
			this.classId = classId;
			this.fileID = fileId;
			this.name = name ?? YamlClassIdDictionary.GetClassName(classId);
			refs = new List<SerializedObjectId>();
		}

		public virtual void Parse(YamlDocument doc, Dictionary<int, SerializedObject> objectsByFileId)
		{
			var scanner = new SerializedObjectRefAnalyser();
			doc.Accept(scanner);
			refs = scanner.references;
		}

		public override string ToString()
		{
			var sb = new System.Text.StringBuilder();
			sb.AppendLine( GetType().ToString() );
			sb.AppendFormat("fileID {0} classId {1} class {2}", fileID, classId, YamlClassIdDictionary.GetClassName(classId));
			sb.AppendLine();
			foreach (var reference in refs)
				sb.AppendLine("ref " + reference.ToString());
			sb.AppendLine("owner " + owner);

			return sb.ToString();
		}

		internal void SetOwner(SerializedObject serializedObject)
		{
			owner = serializedObject;
		}

		public virtual void UpdateOwner(SerializedObject scene)
		{
		}

		public bool ContainsRef(SerializedObjectId objId)
		{
			return refs.Any( r => r.Equals(objId) );
		}

		public bool ContainsGUIDRef(string guid)
		{
			if (string.IsNullOrEmpty(guid))
				throw new System.ArgumentException("guid");

			if (refs == null)
				return false;

			foreach (var r in refs)
				if (guid == r.guid)
					return true;

			return false;
			//return refs.Any( r => r.guid == guid );
		}
	}

	public class ObjectRef
	{
		public int fileID {get; private set;}
		public int giud {get; private set;}
	}
}
