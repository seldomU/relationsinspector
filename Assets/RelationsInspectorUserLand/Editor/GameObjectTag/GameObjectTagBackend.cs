using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RelationsInspector.Backend.GameObjectTag
{
    class TagWrap
    {
        public string m_tag;

        public override string ToString()
        {
            return m_tag;
        }
    }

    class GameObjectTagBackend : MinimalBackend<object,string>
    {
        Dictionary<string, TagWrap> tagObjs = new Dictionary<string, TagWrap>();

        public override IEnumerable<object> GetRelatedEntities(object entity)
        {
            var asGameObject = entity as GameObject;

            if (asGameObject == null)
                yield break;

            if (asGameObject.Equals(null))
                yield break;

            if (asGameObject.tag == null)
                yield break;

            string tag = asGameObject.tag;

            if (!tagObjs.ContainsKey(tag))
                tagObjs[tag] = new TagWrap { m_tag = tag };

            yield return tagObjs[tag];
        }
    }
}
