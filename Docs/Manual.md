Relations inspector manual
==========================

The relations inspector is a Unity editor extension that lets the user visualize and edit relations between all kinds of project data. It will be called *RI* in this manual, to avoid confusion with Unity's inspector window.

![screenshots](http://i.imgur.com/k7BooH6.png "some screenshots")

Each kind of relation graph is driven by a backend class. A backend defines what relations to show for a specific object type.

Using the RI
-------------------
Typically, you drag and drop some assets (the target objects) into the RI window. The RI will pick a backend that fits their types and show a graph made by the backend from your target objects. The auto-selected backend may not be the one you want, so the graph might look underwhelming. In the toolbar you'll find a dropdown where you can choose between all backends that fit your target types.

The toolbar also contains:

* a **Clear** button, that will reset the target objects and the graph
* a **Refresh** button, that will generate a new graph from the current target objects
* a **Layout** selector, that lets you arrange the graph in a tree layout, if applicable
* a **Entity widget** selector, that offers two ways of displaying entity nodes

The rest of the window contains the graph and its minimap, as well as  backend-specific GUI controls, if there are any. The graph area responds to the following inputs:

* moving the mouse wheel zooms in and out
* dragging the graph-area while the right mouse button is pressed, shifts the graph
* clicking into the minimap makes the window focus on the clicked location
* left-clicking an entity widget will select it. Pressing control at the same time adds the entity to the existing selection.
* dragging the graph-area while the left mouse button is pressed, selects all entities in the drag area
* right-clicking the graph-area while pressing control creates an entity, if the backend permits
* right clicking on entity- and relationwidgets is handled by each backend. It may open a context menu


If you want to create a graph from scratch: clear the window, select your backend and use its controls to create and modify your data.

<!-- todo: cover the case where the game or a second window is driving the API -->



Developing a backend
--------------------

The RI considers any type as backend that lives in the editor dll and implements IGraphBackend. In the package you can find some utility classes that contain basic implementations of all IGraphBackend members.

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

Save that to *Assets\Editor* or any other editor folder, wait for Unity to recompile the dll, and now it should be selectable in the RI.

We derive from *MinimalBackend* (one of the utility classes) instead of *IGraphBackend*, because it contains default implementations of all interface members, and allows us to focus on the key properties: the entity- and relation type become generic params of the backend type, and the relation definition becomes the code of *GetRelatedEntities*.

In the following sections, we'll add more features to the backend.

###Pimping your backend
modification etc

####Drawing the entity widgets####

- high level: ShowObjectName()	// use this to draw the name of the object
- mid level: ShowContent(GUIContent)	// use this if you want a standard box/circle with your custom content
- low level: Rect Draw()				// use this if you want to draw a custom widget with your custom content

####Multiple relation types####
- override DefaultBackend.GetRelatedEntities if you only need one kind of relation
- otherwise override DefaultBackend.GetRelations
- if you want to color-code your relation types, override GetRelationColor
- if you want to allow the user to add relations of multiple types, add items to the entity context menu


