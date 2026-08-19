using Shared.Skill;
using Yaml;

namespace SkillData;

public class SkillNode : INewCheckerable
{
	public readonly int CategoryLevel;

	public readonly string Name;

	public readonly string Icon;

	public readonly string Description;

	public readonly int SkillPoints;

	public readonly bool UntrainDisabled;

	public readonly int RenderPriority;

	public readonly string Group;

	private readonly string[] _rewards;

	public Reward[] Rewards;

	private NewCheckerContainer _newChecker;

	public string Id => Parent.Id;

	public string Sub => Parent.SubId;

	public int Level { get; private set; }

	public Skill Parent { get; private set; }

	public SkillState State { get; private set; }

	public bool IsNew { get; set; }

	public Category Category => Parent.Category;

	public NewChecker NewChecker
	{
		get
		{
			if (_newChecker == null)
			{
				_newChecker = new NewCheckerContainer();
			}
			return _newChecker;
		}
	}

	public SkillNode(Yaml.Skill s, Skill parent, int level)
	{
		CategoryLevel = s.category_level;
		Name = s.name;
		Icon = s.icon;
		if (string.IsNullOrEmpty(Icon))
		{
			Icon = "icon_question";
		}
		Description = s.description;
		SkillPoints = s.skill_point;
		UntrainDisabled = s.untrain_disabled;
		RenderPriority = s.render_priority;
		Group = s.subcategory;
		_rewards = s.rewards;
		Parent = parent;
		Level = level;
	}

	public void InitRewards(RewardYaml yml)
	{
		int num = ((_rewards != null) ? _rewards.Length : 0);
		Rewards = new Reward[num];
		for (int i = 0; i < num; i++)
		{
			string key = _rewards[i];
			Yaml.Reward data = yml.Get(key);
			Rewards[i] = new Reward(key, data);
		}
	}

	public bool TryGetReward(string id, out Reward result)
	{
		int i = 0;
		for (int num = ((Rewards != null) ? Rewards.Length : 0); i < num; i++)
		{
			if (id == Rewards[i].Id)
			{
				result = Rewards[i];
				return true;
			}
		}
		result = null;
		return false;
	}

	public int RewardIndexOf(string id)
	{
		int result = -1;
		int i = 0;
		for (int num = ((Rewards != null) ? Rewards.Length : 0); i < num; i++)
		{
			if (id == Rewards[i].Id)
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public void UpdateState()
	{
		SkillState state = State;
		if (Level <= Parent.Level)
		{
			State = SkillState.Learned;
		}
		else
		{
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(Category);
			if (categoryLevel >= CategoryLevel)
			{
				if (Level == Parent.Level + 1 && (Parent.Parent.Base == Parent || Parent.Parent.Base.Level > 0))
				{
					State = ((GameSystem<SkillSystem>.Instance().RemainSkillPoint < SkillPoints) ? SkillState.NotEnoughSp : SkillState.Learnable);
				}
				else
				{
					State = SkillState.NoHaveParent;
				}
			}
			else
			{
				State = SkillState.NotEnoughLv;
			}
		}
		if (state != State && state != 0 && State == SkillState.Learnable)
		{
			IsNew = true;
		}
	}
}
