using System;
using Durango.Logic;
using Durango.Logic.Skill;
using Newtonsoft.Json;
using Shared.Skill;

namespace Yaml;

public class PlayerAction
{
	[JsonIgnore]
	public string Id;

	[JsonProperty(PropertyName = "meta", Required = Required.Always)]
	public PlayerActionMeta Meta;

	[JsonProperty(PropertyName = "attack_info")]
	public PlayerActionAttackInfo[] AttackInfo;

	private bool _parentSkillFound;

	private Node _parentSkill;

	public Node GetParentSkill()
	{
		if (!_parentSkillFound)
		{
			_parentSkillFound = true;
			string id = Id;
			_parentSkill = GameSystem<SkillSystem>.Instance().FindSkill(delegate(Node s)
			{
				if (s.Rewards == null)
				{
					return false;
				}
				Durango.Logic.Skill.Reward[] rewards = s.Rewards;
				foreach (Durango.Logic.Skill.Reward reward in rewards)
				{
					if (reward.Type == RewardType.Action && reward.ActionIds != null && Array.IndexOf(reward.ActionIds, id) != -1)
					{
						return true;
					}
				}
				return false;
			});
		}
		return _parentSkill;
	}
}
