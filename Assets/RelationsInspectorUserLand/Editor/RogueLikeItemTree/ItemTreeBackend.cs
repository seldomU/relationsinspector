using UnityEngine;
using UnityEditor;
using System.Collections;
using RelationsInspector;
using RelationsInspector.Backend;
using System.Collections.Generic;

namespace RelationsInspector.Backend.ItemTree
{
	public class ItemTreeBackend : ScriptableObjectBackend<InventoryItemType, string>
	{
		static Dictionary<Sprite, Texture2D> textures = new Dictionary<Sprite, Texture2D>();

		public override IEnumerable<Tuple<InventoryItemType, string>> GetRelations(InventoryItemType entity)
		{
			if (entity.children == null)
				entity.children = new List<InventoryItemType>();

			foreach (var other in entity.children)
				yield return new Tuple<InventoryItemType, string>(other, string.Empty);
		}

		public override void CreateRelation(InventoryItemType source, InventoryItemType target, string tag)
		{
			if (source.children == null)
				source.children = new List<InventoryItemType>();

			source.children.Add(target);
			EditorUtility.SetDirty(source);
			api.AddRelation(source, target, tag);
		}

		public override void DeleteRelation(InventoryItemType source, InventoryItemType target, string tag)
		{
			source.children.Remove(target);
			EditorUtility.SetDirty(source);
			api.RemoveRelation(source, target, tag);
		}

		public override Rect DrawContent(InventoryItemType entity, EntityDrawContext drawContext)
		{
			float backup = drawContext.style.widgetRadius;
			drawContext.style.widgetRadius = 12;
			var rect = DrawUtil.DrawContent(new GUIContent(entity.name, GetTexture(entity.icon)), drawContext);
			drawContext.style.widgetRadius = backup;
			return rect;
		}

		public override string GetTooltip(InventoryItemType entity)
		{
			return string.Empty;
		}

		static Texture2D GetTexture(Sprite sprite)
		{
			if (sprite == null)
				return null;

			if(!textures.ContainsKey(sprite))
				textures[sprite] = textureFromSprite(sprite);
			return textures[sprite];
		}

		public static Texture2D textureFromSprite(Sprite sprite)
		{
			if (sprite.rect.width != sprite.texture.width)
			{
				Texture2D newText = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
				Color[] newColors = sprite.texture.GetPixels((int)sprite.textureRect.x,
				(int)sprite.textureRect.y,
				(int)sprite.textureRect.width,
				(int)sprite.textureRect.height);
				newText.SetPixels(newColors);
				newText.Apply();
				return newText;
			}
			else
				return sprite.texture;
		}
	}
}
