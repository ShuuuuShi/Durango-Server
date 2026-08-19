using System;
using System.Collections.Generic;
using System.Text;
using SkillData;
using UnityEngine;

public class SkillLevelInfoNode : MonoBehaviour
{
	[Serializable]
	private class StateColorChange
	{
		public UIWidget[] Widget;

		public StateColor Color;
	}

	[Serializable]
	[EnumType(typeof(State))]
	private class StateColor : EnumKeyList
	{
		[SerializeField]
		private List<Color> _values;

		public Color Get(State state)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			int num = IndexOf((int)state);
			return (num != -1) ? _values[num] : Color.white;
		}
	}

	private enum State
	{
		Learnd,
		Current,
		Learnable,
		Sealed
	}

	[SerializeField]
	private UISpriteLabel _titleLabel;

	[SerializeField]
	private UISpriteLabel _spLabel;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private UILabel _conditionLabel;

	[SerializeField]
	private StateColorChange[] _colorChangeWidgets;

	public void Set(SkillNode skill)
	{
		int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(skill.Category);
		State state = ((skill.Level >= skill.Parent.Level) ? ((skill.Level == skill.Parent.Level) ? State.Current : ((skill.CategoryLevel <= categoryLevel) ? State.Learnable : State.Sealed)) : State.Learnd);
		_titleLabel.text = LocalizeSystem.Format((state != State.Sealed) ? "#skill_level_info_available_title" : "#skill_level_info_sealed_title", skill.Level.ToString());
		_spLabel.text = LocalizeSystem.Format("#skill_level_info_sp_label", skill.SkillPoints.ToString());
		int num = ((skill.Rewards != null) ? skill.Rewards.Length : 0);
		if (num > 0)
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < num; i++)
			{
				stringBuilder.AppendLine(skill.Rewards[i].ToReadableText());
			}
			_rewardLabel.text = stringBuilder.ToString().Trim();
		}
		else
		{
			_rewardLabel.text = LocalizeSystem.Get("#skill_level_info_no_reward");
		}
		switch (state)
		{
		case State.Learnd:
			_conditionLabel.text = LocalizeSystem.Get("#skill_level_info_learnd_level");
			break;
		case State.Current:
			_conditionLabel.text = LocalizeSystem.Get("#skill_level_info_current_level");
			break;
		case State.Learnable:
			_conditionLabel.text = LocalizeSystem.Get("#skill_level_info_learnable_level");
			break;
		case State.Sealed:
			_conditionLabel.text = LocalizeSystem.Format("#skill_level_info_sealed_level", SkillUtil.CategoryLocalizeName(skill.Category), skill.CategoryLevel.ToString());
			break;
		}
		SetStateColor(state);
	}

	private void SetStateColor(State state)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _colorChangeWidgets.Length; i++)
		{
			StateColorChange stateColorChange = _colorChangeWidgets[i];
			Color color = stateColorChange.Color.Get(state);
			for (int j = 0; j < stateColorChange.Widget.Length; j++)
			{
				stateColorChange.Widget[j].color = color;
			}
		}
	}
}
