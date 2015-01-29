using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RelationsInspector;
using RelationsInspector.Backend;

namespace RelationsInspector.Backend.Techtree
{
	public class RewardItem : ScriptableObject
	{
		public Reward reward;
		public List<WeightedRewardRef> successors;
	}

	[System.Serializable]
	public class WeightedRewardRef
	{
		public RewardItem rewardItem;
		public int weight;
	}

	public enum Reward { None, Point, Boost, Life }

	namespace RelationsInspector.Backend
	{
		public class RewardBackend : ScriptableObjectBackend<RewardItem, string>
		{
			public override IEnumerable<Tuple<RewardItem, string>> GetRelations(RewardItem rewardItem)
			{
				if (rewardItem.successors == null)
					yield break;

				foreach (var successor in rewardItem.successors)
					yield return new Tuple<RewardItem, string>(successor.rewardItem, "");
			}

			public override void CreateRelation(RewardItem source, RewardItem target, string tag)
			{
				if (source.successors == null)
					source.successors = new List<WeightedRewardRef>();

				source.successors.Add(new WeightedRewardRef() { rewardItem = target, weight = 1 });
				api.AddRelation(source, target, tag);
			}

			public override void DeleteRelation(RewardItem source, RewardItem target, string tag)
			{
				var targetEntries = source.successors.Where(suc => suc.rewardItem = target);

				if (!targetEntries.Any())
				{
					Debug.LogError("RemoveRelation: source is not related to target");
					return;
				}

				source.successors.Remove(targetEntries.First());
				api.RemoveRelation(source, target, tag);
			}
		}
	}
}
