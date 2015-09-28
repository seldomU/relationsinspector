using System;
using UnityEngine;

namespace RelationsInspector
{
    [Serializable]
    public class EntityWidgetStyle
    {
        public Color backgroundColor;
        public Color highlightColor;
        public float highlightStrength;
        public float contentPadding;
        public GUIStyle contentStyle;
        public float widgetRadius;  // for circle widget only
        public Color targetBackgroundColor;
    }
}
