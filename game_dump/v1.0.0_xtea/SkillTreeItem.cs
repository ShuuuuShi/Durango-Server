using System;
using L10N;
using SkillData;
using UnityEngine;

public class SkillTreeItem : SelectableWidget
{
	[Serializable]
	private struct Option
	{
		public ColorSet Learned;

		public ColorSet Learnable;

		public ColorSet LearnableAnimation;

		public ColorSet NotEnoughSp;

		public ColorSet NotEnoughLv;

		public ColorSet NoHaveParent;
	}

	[Serializable]
	private struct ColorSet
	{
		public Color Inner;

		public Color Border;

		public Color Icon;

		public Color Sp;

		public Color SpBg;

		public Color Name;

		public Color Faction;
	}

	[SerializeField]
	private UISpriteLabel _nameLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UISprite _factionSprite;

	[SerializeField]
	private UILabel _spLabel;

	[SerializeField]
	private UISprite _spLabelBg;

	[SerializeField]
	private GameObject _selectorObject;

	[SerializeField]
	private Option _option;

	[SerializeField]
	private UIWidget _inner;

	[SerializeField]
	private UIWidget _border;

	[SerializeField]
	private float _animationPeriod;

	private string _skillSpFormat;

	private string _skillLevelFormat;

	public SkillNode Skill { get; private set; }

	public int Depth { get; set; }

	public void Set(SkillNode skill)
	{
		Skill = skill;
	}

	public void UpdateData()
	{
		if (Skill != null)
		{
			UpdateData(Skill);
		}
	}

	private void UpdateData(SkillNode skill)
	{
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_020c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.text = ((skill.State != SkillState.Learnable) ? skill.Name : $"[skill_up_arrow] {skill.Name}");
		_iconSprite.spriteName = skill.Icon;
		if (_skillSpFormat == null)
		{
			_skillSpFormat = _spLabel.text;
		}
		((Component)_spLabel).gameObject.SetActive(true);
		((Component)_spLabelBg).gameObject.SetActive(true);
		_spLabel.text = ((skill.SkillPoints <= 0) ? T._("AUTO") : string.Format(_skillSpFormat, skill.SkillPoints));
		ColorSet colorSet = _option.NotEnoughLv;
		switch (skill.State)
		{
		case SkillState.Learnable:
			colorSet = _option.Learnable;
			break;
		case SkillState.Learned:
			colorSet = _option.Learned;
			break;
		case SkillState.NotEnoughSp:
			colorSet = _option.NotEnoughSp;
			break;
		case SkillState.NoHaveParent:
			colorSet = _option.NoHaveParent;
			break;
		case SkillState.NotEnoughLv:
			colorSet = _option.NotEnoughLv;
			break;
		}
		bool flag = skill.State == SkillState.Learnable;
		ColorSet learnableAnimation = _option.LearnableAnimation;
		SetColor(_inner, colorSet.Inner, learnableAnimation.Inner, flag);
		SetColor(_border, colorSet.Border, learnableAnimation.Border, flag);
		SetColor(_nameLabel.Label, colorSet.Name, learnableAnimation.Name, flag);
		SetColor(_spLabel, colorSet.Sp, learnableAnimation.Sp, flag);
		SetColor(_spLabelBg, colorSet.SpBg, learnableAnimation.SpBg, flag);
		SetColor(_iconSprite, colorSet.Icon, learnableAnimation.Icon, flag);
		SetColor(_factionSprite, colorSet.Faction, learnableAnimation.Faction, flag);
		if (flag)
		{
			TweenColor.Begin(((Component)_spLabel).gameObject, _animationPeriod, learnableAnimation.Sp).style = UITweener.Style.PingPong;
		}
		else
		{
			((Component)(object)_spLabel).SetEnable<TweenColor>(enable: false);
		}
	}

	private void SetColor(UIWidget widget, Color to, Color from, bool isAnim)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		widget.color = to;
		if (isAnim && to != from)
		{
			TweenColor.Begin(((Component)widget).gameObject, _animationPeriod, from).style = UITweener.Style.PingPong;
		}
		else
		{
			((Component)(object)widget).SetEnable<TweenColor>(enable: false);
		}
	}

	protected override void OnSelected(bool isSelect)
	{
		base.OnSelected(isSelect);
		_selectorObject.gameObject.SetActive(isSelect);
	}
}
