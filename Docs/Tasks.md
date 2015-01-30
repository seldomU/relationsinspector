Tasks
=====

###Demo
- SceneHierarchyBackend
 - component content is the gameobject name
 - rename Backend generic parameters to E and R
- forum post
- screenshots
- gifs (openBroadcastingSoftware, gyfcat)
- video (openBroadcastingSoftware)
	- this is an introduction to the relations inspector window.
	- you can open it by to Window -> Relations inspector
	- the window has a target object, just like the inspector. but it does not adapt its target when you change your object selection. In a minute I'll show you why that is.
	- Here you can pick a backend type, it determines what relations will be displayed. In this case we'll get a graph of all components and child gameObjects owned by the target GameObject.
	- The target object is set by dragging an object into window.
	- Now the window will take the target object (marked in light grey) and apply the relation definition from the backend class
	- drag and drop stuff into it
	- zoom, shift
	- light-grey nodes are the target objects (the ones you dragged in)
	- dark grey nodes are objects that a target is related to, acording to the definition provided by the backend class
- documentation
 - link API ref to backend-dev sections 
  - backend-dev
   - customizing the entity widget
	- high level: ShowObjectName()	// use this to draw the name of the object
	- mid level: ShowContent(GUIContent)	// use this if you want a standard box/circle with your custom content
	- low level: Rect Draw()				// use this if you want to draw a custom widget with your custom content
   - multiple relation types
    - override DefaultBackend.GetRelatedEntities if you only need one kind of relation
    - otherwise override DefaultBackend.GetRelations
    - if you want to color-code your relation types, override GetRelationColor
    - if you want to allow the user to add relations of multiple types, add items to the entity context menu
 - FAQ
  - how do I make the graph update when the data is changed outside the editor window
  - how to restrict relations to a single one
  - how to combine free modification with constraints (single relation per entity, graph has to be a tree)

###Release
- finanzamt
- more examples
 - ingame API usage
- more docs
- more video
- more code
 - let user abort layout process (use coroutine)
- offer backends for other tools
 - https://www.assetstore.unity3d.com/en/#!/content/3535
 - https://www.assetstore.unity3d.com/en/#!/content/3872
 - https://www.assetstore.unity3d.com/en/#!/content/22983
 - http://forum.unity3d.com/threads/script-inspector-3.195218/