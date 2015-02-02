Relations inspector manual
==========================

The relations inspector is a Unity editor extension that lets the user visualize and edit relations between all kinds of project data. It will be called *RI* in this manual, to avoid confusion with Unity's inspector window.

![screenshots](http://i.imgur.com/1OgxgAF.gif "some screenshots")

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

* dragging objects into the window makes them the new inspection targets. If *control* is held down at the same time, the objects are added to the existing inspection targets.
* moving the mouse wheel zooms in and out
* dragging the graph-area while the right mouse button is pressed, shifts the graph
* clicking into the minimap makes the window focus on the clicked location
* left-clicking an entity widget will select it. Pressing control at the same time adds the entity to the existing selection.
* dragging the graph-area while the left mouse button is pressed, selects all entities in the drag area
* right-clicking the graph-area while pressing control creates an entity, if the backend permits
* right clicking on entity- and relationwidgets is handled by each backend. It may open a context menu


If you want to create a graph from scratch: clear the window, select your backend and use its controls to create and modify your data.
