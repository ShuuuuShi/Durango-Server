using System;
using System.Text;
using Durango.Logic;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class SkillNodeInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _skillNameLabel;

	[SerializeField]
	private UIWidget _center;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UISpriteLabel _rewardLabel;

	[SerializeField]
	private UISpriteLabel _conditionLabel;

	[SerializeField]
	private UIWidget _bottom;

	[SerializeField]
	private SelectableButton _learnButton;

	private Node _skill;

	public bool IsShow { get; private set; }

	public SelectableButton LearnButton => _learnButton;

	public event Action<Node> OnLearnSkill;

	public event Action<Node> OnUntrainSkill;

	private void Start()
	{
		SelectableButton learnButton = _learnButton;
		learnButton.Clicked = (Action)Delegate.Combine(learnButton.Clicked, new Action(OnClickLearnButton));
		if (!IsShow)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnClickLearnButton()
	{
		if (_skill.Level > _skill.Parent.Level)
		{
			if (this.OnLearnSkill != null)
			{
				this.OnLearnSkill(_skill);
			}
		}
		else if (this.OnUntrainSkill != null)
		{
			this.OnUntrainSkill(_skill);
		}
	}

	public void Show(Node skill)
	{
		IsShow = true;
		base.gameObject.SetActive(value: true);
		_skill = skill;
		UpdateData();
	}

	public void Hide()
	{
		if (IsShow)
		{
			IsShow = false;
			base.gameObject.SetActive(value: false);
		}
	}

	public void ShowLoadingRingToLearnButton()
	{
		_learnButton.Disabled = true;
		_learnButton.ShowLoadingRing(show: true);
	}

	public void HideLoadingRingToLearnButton()
	{
		_learnButton.Disabled = false;
		_learnButton.ShowLoadingRing(show: false);
	}

	private void SetDescription()
	{
		_descriptionLabel.text = _skill.Description;
		ResizeLabelContainer(_descriptionLabel);
	}

	private void SetReward()
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		bool flag = _skill.State == Durango.Logic.Skill.State.Learned;
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(_skill.Rewards); i < size; i++)
		{
			Durango.Logic.Skill.Reward reward = _skill.Rewards[i];
			if (flag)
			{
				reward.ToReadableLinkText(value, _skill.State);
			}
			else
			{
				reward.ToReadableText(value, _skill.State);
			}
			num++;
		}
		if (num > 0)
		{
			_rewardLabel.transform.parent.gameObject.SetActive(value: true);
			_rewardLabel.text = value.ToString().Trim();
			ResizeLabelContainer(_rewardLabel);
		}
		else
		{
			_rewardLabel.transform.parent.gameObject.SetActive(value: false);
		}
	}

	private void SetBottom()
	{
		if (_skill.Level <= _skill.Parent.Level)
		{
			bool flag = GameSystem<SkillSystem>.Instance().UntrainedCount < Yaml.Util.Singleton<Constants>.Instance.SkillUntrain.MaxCount;
			if (_skill.Level == _skill.Parent.Level && !_skill.UntrainDisabled && flag)
			{
				_learnButton.Text = T._("습득 취소");
				_learnButton.Disabled = false;
			}
			else
			{
				_learnButton.Text = T._("습득함");
				_learnButton.Disabled = true;
			}
		}
		else
		{
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_skill.Category);
			if (_skill.SkillPoints > 0)
			{
				_learnButton.Text = string.Format("[c][FFFFFF][icon=icon_sp][-][/c] {0} [c][FFFFFF][icon=bg_line_height][-][/c] {1}", _skill.SkillPoints, T._("랭크 업"));
				_learnButton.Disabled = GameSystem<SkillSystem>.Instance().RemainSkillPoint < _skill.SkillPoints;
			}
			else
			{
				_learnButton.Text = T._("자동 습득");
				_learnButton.Disabled = false;
			}
			_learnButton.Disabled |= _skill.Level > _skill.Parent.Level + 1 || _skill.CategoryLevel > categoryLevel || (_skill.Parent.Bundle.Base != _skill.Parent && _skill.Parent.Bundle.Base.Level == 0);
		}
		_learnButton.ShowLoadingRing(show: false);
		UIUtility.UpdateAnchors(_bottom.transform);
	}

	private void SetCondition()
	{
		string text = null;
		Color color = Color.white;
		if (_skill.Level > _skill.Parent.Level)
		{
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_skill.Category);
			bool flag = _skill.CategoryLevel <= categoryLevel;
			bool flag2 = false;
			int num = 0;
			if (flag)
			{
				num = GameSystem<SkillSystem>.Instance().RemainSkillPoint;
				if (_skill.SkillPoints > num)
				{
					flag2 = true;
				}
			}
			color = ((!flag) ? new Color(0.73f, 0.2f, 0.22f) : new Color(1f, 0.85f, 0.36f));
			if (flag)
			{
				int num2 = _skill.Parent.Level + 1;
				if (_skill.Level > num2)
				{
					text = T._("[icon=icon_make_alert] {0} 필요", _skill.Parent.Get(num2).Name);
				}
				else if (_skill.Parent.Bundle.Base != _skill.Parent && _skill.Parent.Bundle.Base.Level == 0 && _skill.Level == 1)
				{
					text = T._("[icon=icon_make_alert] {0} 필요", _skill.Parent.Bundle.Base.Get().Name);
				}
				else if (flag2)
				{
					text = T._("[icon=icon_make_alert] SP {0} 부족", _skill.SkillPoints - num);
				}
			}
			else
			{
				text = T._("[icon=icon_make_alert] {0} 계열 {1} 필요", Util.CategoryLocalizeName(_skill.Category), LocalizeUtil.FormatLevel(_skill.CategoryLevel));
			}
		}
		_center.bottomAnchor.absolute = _bottom.height;
		_center.UpdateAnchors();
		UIWidget component = _conditionLabel.transform.parent.GetComponent<UIWidget>();
		UIPanel component2 = _scrollView.ScrollView.GetComponent<UIPanel>();
		if (string.IsNullOrEmpty(text))
		{
			component.gameObject.SetActive(value: false);
			component2.bottomAnchor.absolute = 0;
		}
		else
		{
			component.gameObject.SetActive(value: true);
			component2.bottomAnchor.absolute = component.height;
			_conditionLabel.text = text;
			_conditionLabel.color = color;
		}
		component2.UpdateAnchors();
	}

	private void ResizeLabelContainer(UILabel label)
	{
		Vector3 position = label.GetPosition(0f, 1f);
		UIWidget component = label.transform.parent.GetComponent<UIWidget>();
		Vector3 vector = position - component.localCorners[1];
		int num = (int)Mathf.Abs(vector.y);
		label.transform.parent.GetComponent<UIWidget>().height = label.height + num * 2;
		label.SetPosition(component.localCorners[1] + vector, 0f, 1f);
		UIUtility.UpdateAnchors(label.transform.parent);
	}

	public void UpdateData()
	{
		if (_skill != null)
		{
			_skillNameLabel.text = _skill.Name;
			SetDescription();
			SetReward();
			SetBottom();
			SetCondition();
			_scrollView.Reposition(resetPosition: false, tween: false);
		}
	}
}
