using UnityEngine;
using System.Collections;
using RelationsInspector;
using RelationsInspector.Backend;
using System.Collections.Generic;
using System.Linq;

namespace RelationsInspector.Backend.SocialNetwork
{
	public class SocialNetworkBackend : ScriptableObjectBackend<Person, Feeling>
	{
		public override IEnumerable<Tuple<Person, Feeling>> GetRelated(Person person)
		{
			if (person.acquaintances == null)
				yield break;

			foreach (var acq in person.acquaintances)
				yield return new Tuple<Person, Feeling>(acq.person, acq.feeling);
		}

		public override void CreateRelation(Person source, Person target, Feeling tag)
		{
			if (source.acquaintances == null)
				source.acquaintances = new List<Acquaintance>();

			source.acquaintances.Add(new Acquaintance() { person = target, feeling = tag });
			api.AddRelation(source, target, tag);
		}

		public override void DeleteRelation(Person source, Person target, Feeling tag)
		{
			var targetEntries = source.acquaintances.Where(acq => acq.person == target && acq.feeling == tag);

			if (!targetEntries.Any())
			{
				Debug.LogError("RemoveRelation: source is not related to target");
				return;
			}

			source.acquaintances.Remove(targetEntries.First());
			api.RemoveRelation(source, target, tag);
		}

		// map relation tag value to color
		public override Color GetRelationColor(Feeling feeling)
		{
			switch (feeling)
			{
				case Feeling.Indifference:
					return Color.white;

				case Feeling.Love:
					return Color.red;

				case Feeling.Hate:
					return Color.green;

				case Feeling.ItsComplicated:
				default:
					return Color.magenta;
			}
		}
	}
}
