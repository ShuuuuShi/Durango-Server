using Durango.Logic.Skill;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class SkillListNode : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UITweener _makerObject;

	[SerializeField]
	private SkillLearningTag _skillLearningTag;

	public Group Group { get; private set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	public void Set(Group skillGroup)
	{
		Group = skillGroup;
		UpdateData();
	}

	public void UpdateData()
	{
		if (Group.Skill == null)
		{
			_nameLabel.text = Group.Name;
		}
		else
		{
			Node node = Group.Skill.Base.Get();
			_nameLabel.text = node.Name;
		}
		_skillLearningTag.Refresh(Group);
		int learnableCount = Group.GetLearnableCount();
		bool flag = Group.HasNew();
		_makerObject.gameObject.SetActive(learnableCount > 0);
		if (learnableCount == 0)
		{
			return;
		}
		if (flag && !base.Selected)
		{
			float num = Time.time % (_makerObject.duration * 2f);
			if (num > _makerObject.duration)
			{
				_makerObject.tweenFactor = 2f * _makerObject.duration - num;
				_makerObject.PlayReverse();
			}
			else
			{
				_makerObject.tweenFactor = num;
				_makerObject.PlayForward();
			}
		}
		else
		{
			_makerObject.PlayForward();
			_makerObject.ResetToBeginning();
			_makerObject.enabled = false;
		}
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		if (Group != null && base.IsChangeSelected && base.Selected)
		{
			Group.SetRead();
			UpdateData();
		}
	}
}
