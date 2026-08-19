using System;
using Durango.Logic.Skill;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class SkillTreeItem : SelectableWidget
{
	[Serializable]
	private struct Option
	{
		public ColorSetStruct Learned;

		public ColorSetStruct Learnable;

		public ColorSetStruct LearnableAnimation;

		public ColorSetStruct NotEnoughSp;

		public ColorSetStruct NotEnoughLv;

		public ColorSetStruct NoHaveParent;
	}

	[Serializable]
	private struct ColorSetStruct
	{
		public Color Inner;

		public Color Border;

		public Color Icon;

		public Color Sp;

		public Color SpBg;

		public Color Name;
	}

	[SerializeField]
	private UISpriteLabel _nameLabel;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private SkillLearningTag _skillLearningTag;

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
	private TweenerPlayer _learnedEffect;

	[SerializeField]
	private float _animationPeriod;

	private Durango.Logic.Skill.State _prevState;

	public Node Skill { get; private set; }

	public int Depth { get; set; }

	protected override void OnInit()
	{
		ClickSound = UISound.ClickType.ButtonMedium;
	}

	public void Set(Node skill)
	{
		_prevState = Durango.Logic.Skill.State.None;
		Skill = skill;
	}

	public void UpdateData()
	{
		if (Skill != null)
		{
			UpdateData(Skill);
		}
	}

	private void UpdateData(Node skill)
	{
		_nameLabel.text = ((skill.State != Durango.Logic.Skill.State.Learnable) ? skill.Name : $"[icon=skill_up_arrow] {skill.Name}");
		_iconSprite.spriteName = skill.Icon;
		_spLabel.gameObject.SetActive(value: true);
		_spLabelBg.gameObject.SetActive(value: true);
		_spLabel.text = ((skill.SkillPoints <= 0) ? T._("AUTO") : T._("[sup]SP[/sup] {0}", skill.SkillPoints));
		_skillLearningTag.Refresh(skill);
		ColorSetStruct colorSetStruct = _option.NotEnoughLv;
		switch (skill.State)
		{
		case Durango.Logic.Skill.State.Learnable:
			colorSetStruct = _option.Learnable;
			break;
		case Durango.Logic.Skill.State.Learned:
			colorSetStruct = _option.Learned;
			break;
		case Durango.Logic.Skill.State.NotEnoughSp:
			colorSetStruct = _option.NotEnoughSp;
			break;
		case Durango.Logic.Skill.State.NoHaveParent:
			colorSetStruct = _option.NoHaveParent;
			break;
		case Durango.Logic.Skill.State.NotEnoughLv:
			colorSetStruct = _option.NotEnoughLv;
			break;
		}
		bool flag = skill.State == Durango.Logic.Skill.State.Learnable;
		ColorSetStruct learnableAnimation = _option.LearnableAnimation;
		SetColor(_inner, colorSetStruct.Inner, learnableAnimation.Inner, flag);
		SetColor(_border, colorSetStruct.Border, learnableAnimation.Border, flag);
		SetColor(_nameLabel, colorSetStruct.Name, learnableAnimation.Name, flag);
		SetColor(_spLabel, colorSetStruct.Sp, learnableAnimation.Sp, flag);
		SetColor(_spLabelBg, colorSetStruct.SpBg, learnableAnimation.SpBg, flag);
		SetColor(_iconSprite, colorSetStruct.Icon, learnableAnimation.Icon, flag);
		if (flag)
		{
			TweenColor.Begin(_spLabel.gameObject, _animationPeriod, learnableAnimation.Sp).style = UITweener.Style.PingPong;
		}
		else
		{
			_spLabel.SetEnable<TweenColor>(enable: false);
		}
		if (_prevState != 0 && _prevState != skill.State && skill.State == Durango.Logic.Skill.State.Learned)
		{
			_learnedEffect.Play();
		}
		_prevState = skill.State;
	}

	private void SetColor(UIWidget widget, Color to, Color from, bool isAnim)
	{
		widget.color = to;
		if (isAnim && to != from)
		{
			TweenColor.Begin(widget.gameObject, _animationPeriod, from).style = UITweener.Style.PingPong;
		}
		else
		{
			widget.SetEnable<TweenColor>(enable: false);
		}
	}

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		_selectorObject.gameObject.SetActive(base.Selected);
	}
}
