Tasks
=====

###Demo
- documentation
 - link API ref to backend-dev sections 
  - backend-dev
   - customizing the entity widget
	- high level: ShowObjectName()	// use this to draw the name of the object
	- mid level: ShowContent(GUIContent)	// use this if you want a standard box/circle with your custom content
	- low level: Rect Draw()				// use this if you want to draw a custom widget with your custom content
   - how to set targets and backend (from within a backend)
   - multiple relation types
    - indicating the hierarchy depth?
    - override DefaultBackend.GetRelatedEntities if you only need one kind of relation
    - otherwise override DefaultBackend.GetRelations
    - if you want to color-code your relation types, override GetRelationColor
    - if you want to allow the user to add relations of multiple types, add items to the entity context menu
   - custom GUI controls (toolbar?)
   	- legend for the depth colors?
   - target type differs from entity type (use attribute)
   - controlling 
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

- sign up for 24h deals: https://docs.google.com/forms/d/1eL91dQ2uttWV0hyIPjiYizP98BIxQOfrNx95NfJiLqI/viewform