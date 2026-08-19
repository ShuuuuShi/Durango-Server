using System.Linq;
using Durango.Logic.Notification;
using JetBrains.Annotations;
using Shared.Skill;
using Yaml;

namespace Durango.Logic.Skill;

public class Node : INotificationable
{
	public readonly int CategoryLevel;

	public readonly string Name;

	public readonly string Icon;

	public readonly string Description;

	public readonly int SkillPoints;

	public readonly bool UntrainDisabled;

	public readonly int RenderPriority;

	public readonly string Group;

	public Reward[] Rewards;

	private readonly string[] _rewards;

	private Container _notification;

	public string Id => Parent.Id;

	public string Sub => Parent.SubId;

	public int Level { get; private set; }

	[NotNull]
	public Skill Parent { get; private set; }

	public State State { get; private set; }

	public bool IsNew { get; set; }

	public Shared.Skill.Category Category => Parent.Category;

	public Durango.Logic.Notification.Notification Notification
	{
		get
		{
			if (_notification == null)
			{
				_notification = new Container();
			}
			return _notification;
		}
	}

	public Node(Yaml.Skill s, Skill parent, int level)
	{
		CategoryLevel = s.CategoryLevel;
		Name = s.Name;
		Icon = s.Icon;
		if (string.IsNullOrEmpty(Icon))
		{
			Icon = "icon_question";
		}
		Description = s.Description;
		SkillPoints = s.SkillPoint;
		UntrainDisabled = s.UntrainDisabled;
		RenderPriority = s.RenderPriority;
		Group = s.Subcategory;
		_rewards = s.Rewards;
		Parent = parent;
		Level = level;
	}

	public void InitRewards(RewardYaml yml)
	{
		int size = KUtility.GetSize(_rewards);
		Rewards = new Reward[size];
		bool flag = false;
		for (int i = 0; i < size; i++)
		{
			string key = _rewards[i];
			Yaml.Reward reward = yml.Get(key);
			if (reward != null)
			{
				Rewards[i] = new Reward(key, reward);
			}
			else
			{
				flag = true;
			}
		}
		if (flag)
		{
			Rewards = Rewards.Where((Reward x) => x != null).ToArray();
		}
	}

	public bool TryGetReward(string id, out Reward result)
	{
		int i = 0;
		for (int size = KUtility.GetSize(Rewards); i < size; i++)
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
		for (int size = KUtility.GetSize(Rewards); i < size; i++)
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
		State state = State;
		if (Level <= Parent.Level)
		{
			State = State.Learned;
		}
		else
		{
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(Category);
			if (categoryLevel >= CategoryLevel)
			{
				if (Level == Parent.Level + 1 && (Parent.Bundle.Base == Parent || (Parent.Bundle.Base != null && Parent.Bundle.Base.Level > 0)))
				{
					State = ((GameSystem<SkillSystem>.Instance().RemainSkillPoint < SkillPoints) ? State.NotEnoughSp : State.Learnable);
				}
				else
				{
					State = State.NoHaveParent;
				}
			}
			else
			{
				State = State.NotEnoughLv;
			}
		}
		if (state != State && state != 0 && State == State.Learnable)
		{
			IsNew = true;
		}
	}
}
