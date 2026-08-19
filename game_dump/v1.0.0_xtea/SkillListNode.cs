using SkillData;
using UnityEngine;

public class SkillListNode : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UITweener _makerObject;

	public SkillListWidget.SkillGroup Group { get; private set; }

	public void Set(SkillListWidget.SkillGroup skillGroup)
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
			Skill @base = Group.Skill.Base;
			SkillNode skillNode = @base.Get();
			_nameLabel.text = skillNode.Name;
		}
		int learnableCount = Group.GetLearnableCount();
		bool flag = Group.HasNew();
		((Component)_makerObject).gameObject.SetActive(learnableCount > 0);
		if (learnableCount == 0)
		{
			return;
		}
		if (flag && !base.Select)
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
			((Behaviour)_makerObject).enabled = false;
		}
	}

	protected override void OnSelected(bool isSelect)
	{
		base.OnSelected(isSelect);
		if (Group != null)
		{
			Group.Readed();
			UpdateData();
		}
	}
}
