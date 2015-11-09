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
        public LayoutTweenParameters layoutTweenParameters;
        [HideInInspector]
        public GraphLayoutParameters graphLayoutParameters;
        public bool logToConsole;
    }
}
