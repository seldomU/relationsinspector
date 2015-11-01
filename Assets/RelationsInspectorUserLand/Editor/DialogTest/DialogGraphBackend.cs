using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.DialogGraph
{
	public class DialogGraphBackend : ScriptableObjectBackend<DialogItem, string>
	{
		public override IEnumerable<Relation<DialogItem, string>> GetRelations(DialogItem entity)
		{
			if (entity.options == null)
				yield break;

			foreach (var option in entity.options)
				yield return new Relation<DialogItem, string>(entity, option.target, option.text);
		}

		public override void CreateRelation(DialogItem sourceItem, DialogItem targetItem, string tag)	//Edge<DialogItem, string> relation)
		{
			var targetOption = new DialogItemOption() { target = targetItem, text = string.Empty };
			if (sourceItem.options == null)
				sourceItem.options = new List<DialogItemOption>();
			sourceItem.options.Add(targetOption);

			api.AddRelation(sourceItem, targetItem, tag);
		}

		public override void DeleteRelation(DialogItem sourceItem, DialogItem targetItem, string tag)
		{
			var targetEntries = sourceItem.options.Where(o => o.target == targetItem);

			if (!targetEntries.Any())
			{
				Debug.LogError("RemoveRelation: source is not linked to target");
				return;
			}

			sourceItem.options.Remove(targetEntries.First());
			api.RemoveRelation(sourceItem, targetItem, tag);
		}
	}
}
