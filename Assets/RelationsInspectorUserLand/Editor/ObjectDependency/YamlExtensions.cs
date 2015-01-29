using UnityEngine;
using System.Collections;
using YamlDotNet.RepresentationModel;
using System.Linq;

namespace RelationsInspector.Backend.ObjectDependency
{
	public static class YamlExtensions
	{
		public static bool IsGameObject(this YamlDocument doc)
		{
			return (doc.RootNode.Tag == @"tag:unity3d.com,2011:1");
		}

		public static string ClassName(this YamlDocument doc)
		{
			string classIdStr = doc.RootNode.Tag.Split(':').Last();
			int classId = int.Parse(classIdStr);
			return YamlClassIdDictionary.GetClassName(classId);
		}

		public static int ClassId(this YamlDocument doc)
		{
			string classIdStr = doc.RootNode.Tag.Split(':').Last();
			return int.Parse(classIdStr);
		}

		public static int FileId(this YamlDocument doc)
		{
			return int.Parse(doc.RootNode.Anchor);
		}
	}
}
