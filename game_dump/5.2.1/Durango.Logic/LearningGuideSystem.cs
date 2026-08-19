using System;
using System.Collections.Generic;
using Durango.Logic.LearningGuide;
using Durango.Logic.Skill;
using Durango.Network;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Faction;
using Shared.Skill;
using Yaml;

namespace Durango.Logic;

public class LearningGuideSystem : GameSystem<LearningGuideSystem>
{
	private readonly SkillWithPreviousNodesSet _containsSkillsWithPreviousNodes = new SkillWithPreviousNodesSet();

	private readonly HashSet<Node> _containedSkills = new HashSet<Node>();

	private readonly Dictionary<string, AdviceAchievement> _achievementDict = new Dictionary<string, AdviceAchievement>();

	private Durango.Logic.LearningGuide.Advice _targetAdvice;

	[CanBeNull]
	public Durango.Logic.LearningGuide.Advice TargetAdvice
	{
		get
		{
			return _targetAdvice;
		}
		private set
		{
			_targetAdvice = value;
			RefreshCurrentContainedSkills();
		}
	}

	public bool HasReward { get; private set; }

	public event Action AchievedInfoUpdated;

	public event Action TargetAdviceUpdated;

	private void Start()
	{
		Connections.Frontend.On<AdvisorTargets>(AdvisorTargetsReceived);
		Connections.Frontend.On<TargetTitle>(TargetTitleReceived);
		Singleton<GameManager>.Instance().AddOnReady(delegate
		{
			Connections.Frontend.Send(default(GetAdvisorTargets));
			Connections.Frontend.Send(default(GetTargetTitle));
		});
		Singleton<GameManager>.Instance().MainSceneLoaded += CheckAvailable;
		GameSystem<FactionSystem>.Instance().FactionsUpdated += CheckAvailable;
	}

	public void UpdateAchievementInfo()
	{
		Connections.Frontend.Send(default(GetAdvisorTargets));
	}

	[CanBeNull]
	public AdviceAchievement GetAchievementState(string titleId)
	{
		if (_achievementDict != null)
		{
			return _achievementDict.Get(titleId);
		}
		return null;
	}

	public bool IsSubjectLocked(Durango.Logic.LearningGuide.Advice subject)
	{
		RequiredSkill requiredSkill = subject.RequiredSkill();
		Shared.Skill.Category skill_category = requiredSkill.skill_category;
		if (skill_category == Shared.Skill.Category.Invalid)
		{
			return false;
		}
		return GameSystem<SkillSystem>.Instance().GetCategoryLevel(skill_category) < requiredSkill.level;
	}

	public void SelectCurriculum(Durango.Logic.LearningGuide.Advice advice)
	{
		if (TargetAdvice == advice)
		{
			return;
		}
		if (TargetAdvice != null)
		{
			UIManager.MessageBox.Show(T._("현재 {0} 목표가 진행 중입니다.\n{1:으로} 변경 하시겠습니까?", TargetAdvice.Name, advice.Name), delegate(bool ok)
			{
				if (ok)
				{
					SendSelectTargetTitle(advice.Id);
				}
			});
		}
		else
		{
			SendSelectTargetTitle(advice.Id);
		}
	}

	public void CancelCurriculum(Durango.Logic.LearningGuide.Advice advice)
	{
		if (TargetAdvice != null && TargetAdvice == advice)
		{
			Connections.Frontend.Send(default(CancelTargetTitle)).On<OK>(delegate
			{
				UIManager.SystemMsg(T._("{0} 가이드가 취소되었습니다.", advice.Name));
				ClearTargetAdvice();
			});
		}
	}

	public void ReceiveReward(string titleId)
	{
		Connections.Frontend.Send(new ReceiveAdvisorReward
		{
			TitleId = titleId
		}).On<OK>(delegate
		{
			ClearTargetAdvice();
			UpdateAchievementInfo();
		});
	}

	private static void SendSelectTargetTitle(string titleId)
	{
		SelectTargetTitle msg = default(SelectTargetTitle);
		msg.TitleId = titleId;
		Connections.Frontend.Send(msg);
	}

	public Learning GetSkillLearningState([NotNull] Durango.Logic.Skill.Category category)
	{
		bool flag = false;
		bool flag2 = true;
		foreach (Node containedSkill in _containedSkills)
		{
			if (containedSkill.Category == category.Type)
			{
				flag = true;
				if (containedSkill.State != State.Learned)
				{
					flag2 = false;
					break;
				}
			}
		}
		if (!flag)
		{
			return Learning.None;
		}
		if (flag2)
		{
			return Learning.Learned;
		}
		return Learning.InProgress;
	}

	public Learning GetSkillLearningState([NotNull] Group skillGroup)
	{
		return GetPredicatedSkillsLearningState((Node skill) => skill.Group == skillGroup.Name);
	}

	public Learning GetSkillLearningState([NotNull] Node skill, bool includePreviousNodes = false)
	{
		if ((includePreviousNodes && _containsSkillsWithPreviousNodes.Contains(skill)) || _containedSkills.Contains(skill))
		{
			if (skill.State == State.Learned)
			{
				return Learning.Learned;
			}
			return Learning.InProgress;
		}
		return Learning.None;
	}

	private static void CheckAvailable()
	{
		bool enable = GameSystem<FactionSystem>.Instance().IsFactionEnabled(FactionType.Lama);
		GameSystem<MenuSystem>.Instance().EnableMenu(MenuType.LearningGuide, enable);
	}

	private void RefreshCurrentContainedSkills()
	{
		_containsSkillsWithPreviousNodes.Clear();
		_containedSkills.Clear();
		if (TargetAdvice == null)
		{
			return;
		}
		int num = TargetAdvice.SkillsCount();
		for (int i = 0; i < num; i++)
		{
			Node skill = TargetAdvice.GetSkill(i);
			if (skill != null)
			{
				_containsSkillsWithPreviousNodes.Add(skill);
				_containedSkills.Add(skill);
			}
		}
	}

	private Learning GetPredicatedSkillsLearningState(Predicate<Node> predicate)
	{
		bool flag = false;
		bool flag2 = false;
		foreach (Node containedSkill in _containedSkills)
		{
			if (predicate(containedSkill))
			{
				if (containedSkill.State == State.Learned)
				{
					flag = true;
				}
				else
				{
					flag2 = true;
				}
			}
		}
		if (flag2)
		{
			return Learning.InProgress;
		}
		if (flag)
		{
			return Learning.Learned;
		}
		return Learning.None;
	}

	public bool HasLearnableSkillForCurrentTitle()
	{
		foreach (Node containedSkill in _containedSkills)
		{
			if (containedSkill.Parent.Level < containedSkill.Level)
			{
				if (containedSkill.State == State.Learnable)
				{
					return true;
				}
				if (containedSkill.Parent.HasLearnableNode())
				{
					return true;
				}
			}
		}
		return false;
	}

	private void AdvisorTargetsReceived(AdvisorTargets msg, PacketHeader header)
	{
		bool flag = false;
		foreach (KeyValuePair<string, float> title in msg.Titles)
		{
			AdviceAchievement adviceAchievement = _achievementDict.Get(title.Key);
			bool flag2 = msg.RemainingRewards.Contains(title.Key);
			if (adviceAchievement == null)
			{
				adviceAchievement = new AdviceAchievement();
				_achievementDict.Add(title.Key, adviceAchievement);
			}
			adviceAchievement.Ratio = title.Value;
			adviceAchievement.CanReward = flag2;
			flag = flag || flag2;
		}
		HasReward = flag;
		if (this.AchievedInfoUpdated != null)
		{
			this.AchievedInfoUpdated();
		}
	}

	private void TargetTitleReceived(TargetTitle msg, PacketHeader header)
	{
		TargetAdvice = GameSystem<StatisticsSystem>.Instance().GetAdvice(msg.TitleId);
		if (this.TargetAdviceUpdated != null)
		{
			this.TargetAdviceUpdated();
		}
	}

	private void ClearTargetAdvice()
	{
		TargetAdvice = null;
		if (this.TargetAdviceUpdated != null)
		{
			this.TargetAdviceUpdated();
		}
	}
}
