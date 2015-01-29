using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.ObjectDependency
{
	public class YamlObjectBackend : MinimalBackend<SerializedObject, string>
	{
		public override IEnumerable<SerializedObject> GetRelatedEntities(SerializedObject entity)
		{
			var asGameObject = entity as SerializedGameObject;
			if (asGameObject != null)
			{
				foreach (var c in asGameObject.components)
					yield return c;
				// prefab?
				yield break;
			}

			var asTransform = entity as SerializedTransform;
			if (asTransform != null)
			{
				foreach (var child in asTransform.children)
					yield return child;

				yield return asTransform.gameObject;

				if (asTransform.parent != null)
					yield return asTransform.parent;

				yield break;
			}
		}
	}
}
