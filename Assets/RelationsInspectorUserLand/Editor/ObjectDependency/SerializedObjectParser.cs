using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.RepresentationModel;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class SerializedObjectParser
	{
		public static IEnumerable<SerializedObject> Parse(IEnumerable<YamlDocument> documents, SerializedObject scene = null)
		{
			var fileIdByDoc = new Dictionary<YamlDocument, int>();
			var objectByFileId = new Dictionary<int, SerializedObject>();

			// create objects and initialize reference-map
			foreach (var doc in documents)
			{
				var obj = CreateObject(doc);
				objectByFileId[obj.fileID] = obj;
				fileIdByDoc[doc] = obj.fileID;
			}

			// parse objects
			foreach (var doc in documents)
			{
				var docObj = objectByFileId[fileIdByDoc[doc]];
				docObj.Parse(doc, objectByFileId);
			}

			foreach (var doc in documents)
				objectByFileId[fileIdByDoc[doc]].UpdateOwner(scene);

			return objectByFileId.Values;
		}

		static SerializedObject CreateObject(YamlDocument doc)
		{
			int classId = doc.ClassId();
			int fileId = doc.FileId();
			switch (classId)
			{
				case YamlClassIdDictionary.ClassIdGameObject:
					return new SerializedGameObject(classId, fileId);

				case YamlClassIdDictionary.ClassIdTransform:
					return new SerializedTransform(classId, fileId);

				default:
					return new SerializedObject(classId, fileId);
			}
		}
	}
}
