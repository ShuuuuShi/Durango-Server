using System;
using Durango.Logic.Item;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Item;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PetMilestoneSelectedInfoWidget : MonoBehaviour
{
	[CanBeNull]
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _textBg;

	[SerializeField]
	private UILabel _skillLabel;

	[SerializeField]
	private UISprite _skillSprite;

	[SerializeField]
	private GameObject _skillInfoObject;

	private string _tagId;

	private Messages.PetActiveSkill? _skill;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_skillInfoObject.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject obj)
		{
			Messages.PetActiveSkill? skill = _skill;
			if (skill.HasValue)
			{
				Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(_skill.Value.SkillId, _skill.Value.Rank);
				if (petActiveSkill != null)
				{
					WidgetTooltipControl widgetTooltipControl = UIManager.Popup.FindTooltip<WidgetTooltipControl>();
					widgetTooltipControl.AutoPosition = false;
					widgetTooltipControl.Set($"<em>{petActiveSkill.Name}</em>", petActiveSkill.Description, 400);
					widgetTooltipControl.Show();
					widgetTooltipControl.SetPosition(obj.GetComponent<UIWidget>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), Vector3.up * 20f);
				}
			}
		});
	}

	public void SetTitle(string title)
	{
		if (_titleLabel != null)
		{
			_titleLabel.text = title;
		}
	}

	public void Set(string tagId)
	{
		_skill = null;
		if (!(_tagId == tagId))
		{
			_tagId = tagId;
			Yaml.Tag tag = ((tagId != null) ? SingletonDict<string, Yaml.Tag>.Get(tagId) : null);
			if (tag == null)
			{
				SetUnknown();
				return;
			}
			_textLabel.text = tag.Name;
			_textLabel.color = TagData.GetGradeColor(tag.Grade);
			_textBg.UpdateAnchors();
			_textBg.gameObject.SetActive(value: true);
			_textLabel.gameObject.SetActive(value: true);
			_skillInfoObject.gameObject.SetActive(value: false);
		}
	}

	public void Set(Messages.PetActiveSkill skill)
	{
		_tagId = null;
		if (!_skill.HasValue || !(_skill.Value.SkillId == skill.SkillId) || _skill.Value.Rank != skill.Rank)
		{
			_skill = skill;
			Yaml.PetActiveSkill petActiveSkill = PetActiveSkills.Get(skill.SkillId, skill.Rank);
			if (petActiveSkill == null)
			{
				SetUnknown();
				return;
			}
			_skillLabel.text = petActiveSkill.Name;
			_skillSprite.spriteName = petActiveSkill.Icon;
			_textLabel.gameObject.SetActive(value: false);
			_skillInfoObject.gameObject.SetActive(value: true);
		}
	}

	public void SetClear()
	{
		_tagId = null;
		_skill = null;
		_textLabel.gameObject.SetActive(value: false);
		_textBg.gameObject.SetActive(value: false);
		_skillInfoObject.gameObject.SetActive(value: false);
	}

	public void SetEmpty()
	{
		_tagId = null;
		_skill = null;
		SetEmptyText();
	}

	public void SetUnknown()
	{
		_tagId = null;
		_skill = null;
		SetUnknownText();
	}

	private void SetUnknownText()
	{
		_textLabel.text = "?";
		_textLabel.color = TagData.GetGradeColor(TagGrade.Negative);
		_textBg.UpdateAnchors();
		_textBg.gameObject.SetActive(value: true);
		_textLabel.gameObject.SetActive(value: true);
		_skillInfoObject.gameObject.SetActive(value: false);
	}

	private void SetEmptyText()
	{
		_textLabel.text = T._("없음");
		_textLabel.color = TagData.GetGradeColor(TagGrade.Negative);
		_textLabel.gameObject.SetActive(value: true);
		_textBg.gameObject.SetActive(value: false);
		_skillInfoObject.gameObject.SetActive(value: false);
	}
}
