using UnityEngine;
using System.Collections;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class SerializedObjectId
	{
		public int fileId { get; private set; }
		public string guid { get; private set; }
		public int type { get; private set; }

		public SerializedObjectId(int fileId, string guid, int type)
		{
			this.fileId = fileId;
			this.guid = guid;
			this.type = type;
		}

		public override bool Equals(object obj)
		{
			var asId = obj as SerializedObjectId;
			return asId != null && fileId == asId.fileId && guid == asId.guid && type == asId.type;
		}

		public override int GetHashCode()
		{
			int hash = 13;
			hash = (hash * 7) + fileId.GetHashCode();
			hash = (hash * 7) + guid.GetHashCode();
			hash = (hash * 7) + type.GetHashCode();
			return hash;
		}

		public override string ToString()
		{
			return string.Format("fileId {0} guid {1} type {2}", fileId, guid, type);
		}
	}
}
