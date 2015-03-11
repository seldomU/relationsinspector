using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryItemType : ScriptableObject
{
	public Sprite icon;
	public List<InventoryItemType> children;
}
