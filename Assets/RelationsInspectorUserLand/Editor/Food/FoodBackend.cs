using UnityEngine;
using System.Collections;
using RelationsInspector;
using RelationsInspector.Backend;
using System.Collections.Generic;

namespace RelationsInspector.Backend.Food
{
	public class FoodBackend : ScriptableObjectBackend<FoodItem, string>
	{
		public override IEnumerable<Relation<FoodItem, string>> GetRelations(FoodItem entity)
		{
			if (entity.dependentFoods == null)
				entity.dependentFoods = new List<FoodItem>();

			foreach (var other in entity.dependentFoods)
				yield return new Relation<FoodItem, string>(entity, other, string.Empty);
		}

		public override void CreateRelation(FoodItem source, FoodItem target, string tag)
		{
			if (source.dependentFoods == null)
				source.dependentFoods = new List<FoodItem>();

			source.dependentFoods.Add(target);
			api.AddRelation(source, target, tag);
		}

		public override void DeleteRelation(FoodItem source, FoodItem target, string tag)
		{
			source.dependentFoods.Remove(target);
			api.RemoveRelation(source, target, tag);
		}

		public override Rect DrawContent(FoodItem entity, EntityDrawContext drawContext)
		{
			float backup = drawContext.style.widgetRadius;
			drawContext.style.widgetRadius = 16;
			var rect = DrawUtil.DrawContent(new GUIContent(entity.name, entity.icon), drawContext);
			drawContext.style.widgetRadius = backup;
			return rect;
		}
	}
}
