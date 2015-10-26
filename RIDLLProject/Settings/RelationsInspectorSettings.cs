using UnityEngine;

namespace RelationsInspector
{
    class RelationsInspectorSettings : ScriptableObject
    {
        public TreeRootLocation treeRootLocation;
        public bool showMinimap;
        public MinimapLocation minimapLocation;
        [HideInInspector]
        public GraphLayoutParams layoutParams;
    }
}
