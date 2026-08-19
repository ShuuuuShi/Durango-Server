using System.Collections.Generic;
using Durango.Logic;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Shared.Skill;
using UnityEngine;

namespace Durango.UI;

public class SkillCategoryNode : SelectableWidget
{
	[SerializeField]
	private UILabel _nameLebel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private GameObject _maxLvObject;

	[SerializeField]
	private SkillLearningTag _skillLearningTag;

	[SerializeField]
	private GameObject _maxExpObject;

	[SerializeField]
	private UISprite _gaugeUpper;

	[SerializeField]
	private GameObject _selector;

	[SerializeField]
	private GameObject _researchAlarm;

	[SerializeField]
	private UITweener _newMaker;

	[SerializeField]
	private GameObject _border;

	[SerializeField]
	private GameObject _eventArrow;

	public Durango.Logic.Skill.Category Category { get; private set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	public void Set(Shared.Skill.Category category)
	{
		Category = GameSystem<SkillSystem>.Instance().GetSkillCategory(category);
		UpdateData();
	}

	private void UpdateData()
	{
		if (Category == null)
		{
			return;
		}
		_nameLebel.text = Util.CategoryLocalizeName(Category.Type);
		_iconSprite.spriteName = Util.CategoryIcon(Category.Type);
		UIUtility.ResizeToSquare(_iconSprite);
		if (Category.IsResearching())
		{
			_levelLabel.text = string.Format("{0}[preset=animation_arrow][c][{2}]{1}[-][/c]", LocalizeUtil.FormatLevel(Category.Level), Category.Level + 1, NGUIText.EncodeColor(PresetColor.UIYellow));
			_border.gameObject.SetActive(value: true);
		}
		else
		{
			_levelLabel.text = LocalizeUtil.FormatLevel(Category.Level);
			_border.gameObject.SetActive(value: false);
		}
		int level = GameSystem<StatisticsSystem>.Instance().Level;
		GameSystem<SkillSystem>.Instance().GetCategoryExp(Category.Type, out var current, out var max);
		if (max < 0 || (current >= max && Category.Level >= level))
		{
			_maxExpObject.SetActive(value: true);
			_gaugeUpper.transform.parent.gameObject.SetActive(value: false);
		}
		else
		{
			_maxExpObject.SetActive(value: false);
			_gaugeUpper.transform.parent.gameObject.SetActive(value: true);
			_gaugeUpper.fillAmount = Mathf.Clamp01((float)current / (float)max);
		}
		_researchAlarm.SetActive(Category.IsReadyToResearch());
		_maxLvObject.SetActive(Category.Level > 0 && max < 0);
		_skillLearningTag.Refresh(Category);
		bool flag = false;
		bool flag2 = false;
		List<Bundle> skills = GameSystem<SkillSystem>.Instance().Skills;
		for (int i = 0; i < skills.Count; i++)
		{
			if (skills[i].Category == Category.Type)
			{
				if (!flag2 && skills[i].HasNew())
				{
					flag2 = true;
				}
				if (!flag && skills[i].GetLearnableCount() > 0)
				{
					flag = true;
				}
			}
		}
		_newMaker.gameObject.SetActive(flag);
		if (flag)
		{
			_newMaker.GetComponent<UIRect>().UpdateAnchors();
			if (flag2)
			{
				float num = Time.time % (_newMaker.duration * 2f);
				if (num > _newMaker.duration)
				{
					_newMaker.tweenFactor = 2f * _newMaker.duration - num;
					_newMaker.PlayReverse();
				}
				else
				{
					_newMaker.tweenFactor = num;
					_newMaker.PlayForward();
				}
			}
			else
			{
				_newMaker.PlayForward();
				_newMaker.ResetToBeginning();
				_newMaker.enabled = false;
			}
		}
		Color tint = ((Category.Level != 0) ? Color.white : (Color.white * 0.5f));
		tint.a = 1f;
		SetTint(tint);
		float value;
		bool active = SkillSystem.FindSkillExpModifier(Category.Type, out value) != null;
		_eventArrow.SetActive(active);
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		_selector.SetActive(base.Selected);
	}
}
