/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector;

public class RadialTestBuilder : DefaultBackend<string, string>
{
	class Item
	{
		public int[] successorIds;
		public Item(int[] successorIds) {this.successorIds = successorIds; }
	}

	string[] strings = new[]
	{
		"root",
		"1st ring 1",
		"1st ring 2",
		"1st ring 3",
		"2nd ring 1",
		"2nd ring 2",
		"2nd ring 3",
		"2nd ring 4",
		"2nd ring 5",
	};

	// test data: string nodes and their successors
	Item[] items = new[]{
		new Item( new[]{1,2,3} ),
		new Item( new[]{4} ),
		new Item( new int[]{5,6}),
		new Item( new int[]{7,8}),
		new Item( new int[]{}),
		new Item( new int[]{}),
		new Item( new int[]{}),
		new Item( new int[]{}),
		new Item( new int[]{}),
	};

	// implementing IGraphConstruct
	public string GetSeed()
	{
		return strings[ 0 ];
	}

	// implementing IGraphConstruct
	public override IEnumerable<string> GetRelatedEntities(string str)
	{
		int strId = System.Array.IndexOf<string>(strings, str);
		return items[strId].successorIds.Select(id => strings[id]);
	}
}
*/
