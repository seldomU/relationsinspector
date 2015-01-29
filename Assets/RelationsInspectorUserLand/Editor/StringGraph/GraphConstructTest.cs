/*
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector;

public class GraphConstructTest : DefaultBackend<string,string>
{
	class Item
	{
		public int[] successorIds;
		public Item(int[] successorIds) {this.successorIds = successorIds; }
	}

	string[] strings = new[]
	{
		"first",
		"second, which is wide\n and extra high",
		"third",
		"forth"
	};


	// test data: string nodes and their successors
	Item[] items = new[]{
		new Item( new[]{1,2} ),
		new Item( new[]{3} ),
		new Item( new int[]{}),
		new Item( new int[]{})
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
