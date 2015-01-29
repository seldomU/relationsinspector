using UnityEngine;
using RelationsInspector.Backend;
using System.Collections;
using System.Collections.Generic;

public class GameObjectHierarchyBackend : MinimalBackend<GameObject, string>
{
	public override IEnumerable<GameObject> GetRelatedEntities(GameObject entity)
	{
		// include sub-gameObjects
		foreach (Transform t in entity.transform)
			yield return t.gameObject;
	}
}
