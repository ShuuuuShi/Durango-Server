using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Skill;
using Durango.Utils.Extensions;
using Shared.Skill;

public class TameableHelper
{
	private static TameableHelper _instance;

	private bool _isDirtyTameableData = true;

	private readonly HashSet<int> _tameablePetTypes = new HashSet<int>();

	private readonly Dictionary<int, Node> _requiredForTaming = new Dictionary<int, Node>();

	static TameableHelper()
	{
		GameManager.Reset += delegate
		{
			_instance = null;
		};
	}

	private TameableHelper()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += delegate
		{
			_isDirtyTameableData = true;
		};
	}

	public static TameableHelper Instance()
	{
		if (_instance == null)
		{
			_instance = new TameableHelper();
		}
		return _instance;
	}

	private void RefreshTameableData()
	{
		if (!_isDirtyTameableData)
		{
			return;
		}
		_isDirtyTameableData = false;
		_tameablePetTypes.Clear();
		_requiredForTaming.Clear();
		List<Bundle> skills = GameSystem<SkillSystem>.Instance().Skills;
		for (int i = 0; i < skills.Count; i++)
		{
			Bundle bundle = skills[i];
			int j = 0;
			for (int num = KUtility.GetSize(bundle.Sub) + 1; j < num; j++)
			{
				Skill skill = ((j != 0) ? bundle.Sub[j - 1] : bundle.Base);
				if (skill == null)
				{
					continue;
				}
				for (int k = 1; k <= skill.MaxLevel; k++)
				{
					Node node = skill.Get(k);
					if (node.Rewards == null)
					{
						continue;
					}
					Reward[] rewards = node.Rewards;
					foreach (Reward reward in rewards)
					{
						if (reward.Type == RewardType.PetTameable && reward.EntityTypes != null)
						{
							if (k <= skill.Level)
							{
								_tameablePetTypes.AddRange(reward.EntityTypes);
							}
							int[] entityTypes = reward.EntityTypes;
							foreach (int key in entityTypes)
							{
								_requiredForTaming[key] = node;
							}
						}
					}
				}
			}
		}
	}

	public bool IsTameableType(int entityType)
	{
		RefreshTameableData();
		return _tameablePetTypes.Contains(entityType);
	}

	public Node RequiredForTaming(int entityType)
	{
		RefreshTameableData();
		return _requiredForTaming.Get(entityType);
	}
}
