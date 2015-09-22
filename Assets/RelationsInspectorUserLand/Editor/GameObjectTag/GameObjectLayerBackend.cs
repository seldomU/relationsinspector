using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using RelationsInspector.Extensions;

namespace RelationsInspector.Backend.GameObjectLayer
{
    class TagWrap
    {
        public string m_tag;

        public override string ToString()
        {
            return m_tag;
        }
    }

    class GameObjectLayerBackend : MinimalBackend<object, string>
    {
        Dictionary<string, TagWrap> tagObjs = new Dictionary<string, TagWrap>();

        public override IEnumerable<object> Init(IEnumerable<object> targets, RelationsInspectorAPI api)
        {
            if (targets == null)
                return Enumerable.Empty<object>();

            var gameObjects = new HashSet<GameObject>();
            var next = new Queue<GameObject>( targets.OfType<GameObject>() );
            while(next.Any())
            {
                var item = next.Dequeue();
                gameObjects.Add(item);
                foreach (Transform t in item.transform)
                    next.Enqueue(t.gameObject);
            }

            return gameObjects.Cast<object>();
        }
        /*
        		// include sub-gameObjects
		foreach (Transform t in entity.transform)
			yield return t.gameObject;
        */

        public override IEnumerable<object> GetRelatedEntities(object entity)
        {
            var asGameObject = entity as GameObject;
            if (!asGameObject)
                yield break;

            if (asGameObject.renderer == null)
                yield break;

            string tag = asGameObject.renderer.sortingLayerName;

            if (!tagObjs.ContainsKey(tag))
                tagObjs[tag] = new TagWrap { m_tag = tag };

            yield return tagObjs[tag];
        }
    }
}
