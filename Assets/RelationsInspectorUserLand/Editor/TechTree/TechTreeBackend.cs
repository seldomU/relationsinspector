using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using RelationsInspector.Backend;
using RelationsInspector;

namespace RelationsInspector.Backend.Techtree
{
	public class TechTreeBackend : ScriptableObjectBackend<Tech, string>
	{
		public override IEnumerable<Relation<Tech, string>> GetRelations(Tech entity)
		{
			if (entity.dependentTechs == null)
				yield break;

			foreach (var tech in entity.dependentTechs)
				yield return new Relation<Tech, string>(entity, tech, string.Empty);
		}

		// returns true if the relation was created
		public override void CreateRelation(Tech source, Tech target, string tag)
		{
			if (source.dependentTechs == null)
				source.dependentTechs = new List<Tech>();

			source.dependentTechs.Add(target);
			api.AddRelation(source, target, tag);
		}

		// returns true if the relation was removed
		public override void DeleteRelation(Tech source, Tech target, string tag)
		{
			source.dependentTechs.Remove(target);
			api.RemoveRelation(source, target, tag);
		}
	}
}
