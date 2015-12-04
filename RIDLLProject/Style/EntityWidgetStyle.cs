using System;
using UnityEngine;

namespace RelationsInspector
{
    [Serializable]
    public class EntityWidgetStyle
    {
        public Color backgroundColor;
        public Color highlightColor;
        public Color unexploredColor;
        public int highlightStrength;
        public GUIStyle contentStyle;
        public float widgetRadius;  // for circle widget only
        public Color targetBackgroundColor;
    }
}
