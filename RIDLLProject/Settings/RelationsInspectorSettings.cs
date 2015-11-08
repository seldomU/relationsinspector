using UnityEngine;

namespace RelationsInspector
{
    class RelationsInspectorSettings : ScriptableObject
    {
        public int maxGraphNodes;
        public TreeRootLocation treeRootLocation;
        public bool showMinimap;
        public MinimapLocation minimapLocation;
        [HideInInspector]
        public GraphLayoutParams layoutParams;
        public bool logToConsole;
    }
}
