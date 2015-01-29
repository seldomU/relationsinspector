using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace RelationsInspector.Backend.ReferenceUtil
{
	public static class ReferenceUtil
	{
		// returns all references to T-objects in the fields of obj. recursively, without going into any found T-objects
		public static IEnumerable<T> GetReferences<T>(object obj) where T : class
		{
			if (obj == null)
				return Enumerable.Empty<T>();

			var visited = new HashSet<object>();
			var references = new List<T>();
			AddReferences<T>(obj, references, visited);
			return references;
		}

		// add all references to T-objects to the hashset, skiping all objects in the visited hashset
		public static void AddReferences<T>(object obj, List<T> references, HashSet<object> visited) where T : class
		{
			var fields = GetAllFields(obj.GetType());
			foreach (var field in fields)
				AddReferences<T>(obj, field, references, visited);
		}

		// add all references to T-object in the field to the hashset, skiping all objects in the visited hashset
		public static void AddReferences<T>(object obj, FieldInfo field, List<T> references, HashSet<object> visited) where T : class
		{
			if (field.GetType().IsValueType)
				return;

			var fieldValue = field.GetValue(obj);
			if (fieldValue == null)
				return;

			bool recurse = !visited.Contains(fieldValue);
			visited.Add(fieldValue);

			var asT = fieldValue as T;
			if (asT != null)
			{
				references.Add(asT);
				return;
			}

			if (!recurse)
				return;

			var asIList = fieldValue as IList;
			if (asIList != null)
			{
				AddReferences<T>(asIList, references, visited);
				return;
			}

			AddReferences<T>(fieldValue, references, visited);
		}

		// add all references to T-object in the list to the hashset, skiping all objects in the visited hashset
		public static void AddReferences<T>(IList list, List<T> references, HashSet<object> visited) where T : class
		{
			foreach (var item in list)
			{
				if (visited.Contains(item))
					continue;

				visited.Add(item);

				var asT = item as T;
				if (asT != null)
				{
					references.Add(asT);
					continue;
				}

				AddReferences(item, references, visited);
			}
		}

		static IEnumerable<FieldInfo> GetAllFields(System.Type t)
		{
			if (t == null)
				return Enumerable.Empty<FieldInfo>();

			BindingFlags flags = BindingFlags.Public | BindingFlags.Instance;
			return t.GetFields(flags).Concat(GetAllFields(t.BaseType));
		}
	}
}
