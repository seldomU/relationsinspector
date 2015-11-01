using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using RelationsInspector;
using RelationsInspector.Backend;
using UnityEditor;

namespace Assets.UnitTests.Editor
{
    [TestFixture]
    class BackendTypeTests
    {

        class V2Backend : IGraphBackend2<string, string>
        {
            // initialize the backend object
            public IEnumerable<string> Init(IEnumerable<object> targets, RelationsInspectorAPI api) { yield break; }
            public IEnumerable<Relation<string, string>> GetRelations(string entity) { yield break; }
            public void CreateEntity(Vector2 position) { }
            public void CreateRelation(string source, string target, string tag) { }
            public void OnEntitySelectionChange(string[] selection) { }
            public void OnUnitySelectionChange() { }
            public Rect OnGUI() { return new Rect(); }

            public string GetEntityTooltip(string entity) { return string.Empty; }
            public string GetTagTooltip(string tag) { return string.Empty;  }
            public Rect DrawContent(string entity, EntityDrawContext drawContext) { return new Rect(); }

            public void OnEntityContextClick(IEnumerable<string> entities, GenericMenu menu) { }
            public void OnRelationContextClick(Relation<string,string> relation, GenericMenu menu ) { }
            public Color GetRelationColor(string relationTagValue) { return Color.white; }
            public void OnEvent(Event e) { }
        }

        class V1Backend : MinimalBackend<UnityEngine.Object,string>{ }

        [Test]
        public void V1Decorator()
        {
            var internalBackend = RelationsInspector.BackendUtil.CreateBackendDecorator(typeof(V1Backend));
            Debug.Log("internalBackend is " + internalBackend);
        }

        [Test]
        public void V2Decorator()
        {
            var internalBackend = RelationsInspector.BackendUtil.CreateBackendDecorator(typeof(V2Backend));
            //Assert.That( internalBackend ==)
            //.GetInterfaces().Where(i => i.IsGenericType && backEndInterfaces.Contains(i.GetGenericTypeDefinition()))
            Debug.Log("internalBackend is " + internalBackend);
        }




        [Test]
        public void TypeHierarchyTest()
        {
            var backendType = typeof(RelationsInspector.Backend.TypeHierarchy.TypeInheritanceBackend);
            var internalBackend = RelationsInspector.BackendUtil.CreateBackendDecorator(backendType);
            //RelationsInspector.GraphPosSerialization.LoadGraphLayout()
        }
    }
}
