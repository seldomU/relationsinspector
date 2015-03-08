using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace RelationsInspector.Backend.Food
{
	public class FoodItem : ScriptableObject
	{
		public Texture2D icon;
		public List<FoodItem> dependentFoods;
	}
}
