### Version 1.1.5
* Date 2017.09.02
* Revision cc835a1
* Changes
	* fixed EditorPrefs error in Welcome window for Unity 2017.1

### Version 1.1.4
* Date 2017.03.11
* Revision ccae6f9
* Changes
	* updated S-Quest backend to reflect S-Quest changes

### Version 1.1.3
* Date 2016.12.27
* Revision 8eee8081
* Changes
  * fixed Asset-reference package compile error in Unity 5.4 and up
  * fixed uGUI event backend error due engine-internal changes

### Version 1.1.2
* Date 2016.05.10
* Revision 45fa9fc
* Changes
	* added backend for PlayMaker inter-FSM communication
	* added backends for Dialogue System
	* project view backend can now prune the tree and highlight nodes based on asset types and scene-dependency
	* added backend attributes: Title, Version, Description, Documentation, Icon and Hide
	* backend selection dropdown makes use of the attributes above

### Version 1.1.1
* Date 2016.03.30
* Revision 3823222
* Changes
	* graph layout: increase gravity. disjunct subgraphs will now be closer together
	* tree detection: fixed false positive when there are disjunct subtrees
	* added graph backend packages
		* InventoryMaster
		* S-Quest
		* Asset references and dependency
		* uGUI events
		* project view
		* type hierarchy
	* added Welcome window, where addons can be managed

### Version 1.1.0
* Date 2016.03.10
* Revision b58da13
* Changes
	* moved base directory from RelationsInspector to Plugins/RelationsInspector
	* add API methods GetEntities and GetRelations
	* tooltip placement now takes window borders into consideration
	* minor improvements to example backends

### Version 1.0.5897.2780
* Date 2016.02.24
* Revision 02c0305
* Changes
	* ResetTargets now also accepts a backend type along with the targets
	* added GetAPI1 property to API

### Version 1.0.5862.27937
* Date 2016.01.19
* Revision 760780a
