Backend development
-------------------

The Relations inspector considers any type as backend that lives in the editor dll and implements IGraphBackend. The utility classes shipped with the package contain basic implementations of all IGraphBackend members.

###Getting started
First, you need to decide on the type of your entities, relations and how to define a relation in your graph. As an example, we'll develop a GameObject hierarchy backend. It will accept GameObjects dragged from the hierarchy window and show their child GameObjects. In this case it seems straighforward:

- entity type: GameObject
- relation type: can be anything. We only have a single kind of relation (parent->child), so there is no need to distinguish. We pick string, arbitrarily.
- relation definition: an entity B is related to another entity A, if A's transform contains B's transform

With that, we can implement our backend's first version.

``` csharp
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
```

Save that to *Assets\Editor* or any other editor folder, wait for Unity to recompile the dll, and now it should be selectable in the RI. Drag in scene objects and you should see their hierarchy graph.

We derive from *MinimalBackend* (one of the utility classes) instead of *IGraphBackend*, because it contains default implementations of all interface members, and allows us to focus on the key properties: the entity- and relation type become generic params of the backend type, and the relation definition becomes the code of *GetRelatedEntities*.

In the following sections, we'll add more features to the backend.

###Pimping your backend

#### Changing relations through context menus

Lets allow the user to modify the hierarchy by adding and removing relations. We'll do that with context menus for the entity and relation widgets. Add *using* directives for **UnityEditor** and **System.Linq**, and the following code:

``` csharp
	public override void OnEntityContextClick(IEnumerable<GameObject> entities)
	{
		var menu = new GenericMenu();
		menu.AddItem(new GUIContent("Add relation"), false, () => api.InitRelation(entities.ToArray(), null));
		menu.ShowAsContext();
	}

	public override void CreateRelation(GameObject source, GameObject target, string tag)
	{
		target.transform.parent = source.transform;
		api.AddRelation(source, target, tag);
	}

	public override void OnRelationContextClick(GameObject source, GameObject target, string tag)
	{
		var menu = new GenericMenu();
		menu.AddItem(new GUIContent("Remove relation"), false, () => DeleteRelation(source, target, tag) );
		menu.ShowAsContext();
	}

	void DeleteRelation(GameObject source, GameObject target, string tag)
	{
		target.transform.parent = null;
		api.RemoveRelation(source, target, tag);
	}
```

Removing a relation is straightforward. Our deletion function disconnects the GameObject transform and tells the API to remove the relation edge from the graph. 

Adding a relation requires one more step: the user has to select a second entity. That is handled by the **InitRelation** API call. **CreateRelation** is where we finally connect the transform and make the API add an edge to the graph.

#### Adding/Removing entites

Add this to the **OnEntityContextClick** handler:
``` csharp
menu.AddItem(new GUIContent("Delete entity"), false, () => { foreach (var e in entities) DeleteEntity(e); });
```
also add these functions:
```csharp
	public override void CreateEntity(Vector2 position)
	{
		var gameObject = new GameObject();
		api.AddEntity(gameObject, position);
	}

	void DeleteEntity(GameObject entity)
	{
		api.RemoveEntity(entity);
		GameObject.DestroyImmediate(entity);		
	}
```
Now you can use the context menu to remove entities and right click + control to create them.