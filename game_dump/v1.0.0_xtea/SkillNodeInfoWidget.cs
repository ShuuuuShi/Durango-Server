using System;
using System.Text;
using L10N;
using SkillData;
using UnityEngine;

public class SkillNodeInfoWidget : MonoBehaviour
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private UILabel _skillNameLabel;

	[SerializeField]
	private UILabel _skillRankLabel;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UILabel _rewardLabel;

	[SerializeField]
	private UISpriteLabel _conditionLabel;

	[SerializeField]
	private DefaultSelectableButton _learnButton;

	private AnimationWidget _animWidget;

	private SkillNode _skill;

	private int _scrollViewBottomOffset;

	private string _nameFormat;

	private string _rankFormat;

	private string _descriptionFormat;

	private string _rewardFormat;

	private DefaultSelectableButton.ButtonStyle _learnButtonStyle;

	private string _rankUpButtonFormat;

	private bool _isInit;

	public AnimationWidget AnimWidget => (!((Object)(object)_animWidget == (Object)null)) ? _animWidget : (_animWidget = ((Component)this).GetComponent<AnimationWidget>());

	public bool IsShow { get; private set; }

	public event Action<SkillNode> OnLearnSkill;

	public event Action<SkillNode> OnUntrainSkill;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIPanel component = ((Component)_scrollView.ScrollView).GetComponent<UIPanel>();
			_scrollViewBottomOffset = component.bottomAnchor.absolute;
			_nameFormat = _skillNameLabel.text;
			_rankFormat = _skillRankLabel.text;
			_descriptionFormat = _descriptionLabel.text;
			_rewardFormat = _rewardLabel.text;
			_rankUpButtonFormat = _learnButton.Text;
			_learnButtonStyle = _learnButton.Style;
		}
	}

	private void Start()
	{
		DefaultSelectableButton learnButton = _learnButton;
		learnButton.Clicked = (Action)Delegate.Combine(learnButton.Clicked, new Action(OnClickLearnButton));
		if (!IsShow)
		{
			AnimWidget.SetAlpha(0f, useTween: false);
		}
	}

	private void OnClickLearnButton()
	{
		if (Selectable.Current.Disable)
		{
			return;
		}
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

	public void Show(SkillNode skill)
	{
		IsShow = true;
		((Component)this).gameObject.SetActive(true);
		AnimWidget.Delay = 0.2f;
		AnimWidget.Alpha = 1f;
		_skill = skill;
		UpdateData();
	}

	public void Hide(bool instant)
	{
		if (IsShow)
		{
			IsShow = false;
			if (instant)
			{
				AnimWidget.SetAlpha(0f, useTween: false);
				return;
			}
			AnimWidget.Delay = 0f;
			AnimWidget.Alpha = 0f;
		}
	}

	private void SetDescription()
	{
		_descriptionLabel.text = string.Format(_descriptionFormat, _skill.Description);
		ResizeLabelContainer(_descriptionLabel);
	}

	private void SetReward()
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		for (int i = 0; i < _skill.Rewards.Length; i++)
		{
			stringBuilder.AppendLine(_skill.Rewards[i].ToReadableText());
			num++;
		}
		if (num > 0)
		{
			((Component)((Component)_rewardLabel).transform.parent).gameObject.SetActive(true);
			_rewardLabel.text = string.Format(_rewardFormat, stringBuilder.ToString().Trim());
			ResizeLabelContainer(_rewardLabel);
		}
		else
		{
			((Component)((Component)_rewardLabel).transform.parent).gameObject.SetActive(false);
		}
	}

	private void SetCondition()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)((Component)_conditionLabel).transform.parent).GetComponent<UIWidget>();
		if (_skill == null)
		{
			((Component)component).gameObject.SetActive(false);
		}
		else
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
						text = T._("[icon=icon_make_alert] {0} {1:lv:} 필요", _skill.Parent.Get(num2).Name, num2);
					}
					else if (_skill.Parent.Parent.Base != _skill.Parent && _skill.Parent.Parent.Base.Level == 0 && _skill.Level == 1)
					{
						text = T._("[icon=icon_make_alert] {0} {1:lv:} 필요", _skill.Parent.Parent.Base.Get().Name, 1);
					}
					else if (flag2)
					{
						text = T._("[icon=icon_make_alert] SP {0} 부족", _skill.SkillPoints - num);
					}
				}
				else
				{
					text = T._("[icon=icon_make_alert] {0} 계열 {1} 필요", SkillUtil.CategoryLocalizeName(_skill.Category), LocalizeUtil.FormatLevel(_skill.CategoryLevel));
				}
			}
			if (string.IsNullOrEmpty(text))
			{
				((Component)component).gameObject.SetActive(false);
			}
			else
			{
				((Component)component).gameObject.SetActive(true);
				_conditionLabel.text = text;
				_conditionLabel.Label.color = color;
			}
		}
		UIPanel component2 = ((Component)_scrollView.ScrollView).GetComponent<UIPanel>();
		if (((Component)component).gameObject.activeSelf)
		{
			component2.bottomAnchor.absolute = _scrollViewBottomOffset + component.height;
		}
		else
		{
			component2.bottomAnchor.absolute = _scrollViewBottomOffset;
		}
		Vector2 viewSize = component2.GetViewSize();
		component2.UpdateAnchors();
		Vector2 viewSize2 = component2.GetViewSize();
		if (viewSize != viewSize2)
		{
			_scrollView.PanelResized();
		}
	}

	private void ResizeLabelContainer(UILabel label)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = label.GetPosition(0f, 1f);
		UIWidget component = ((Component)((Component)label).transform.parent).GetComponent<UIWidget>();
		Vector3 val = position - component.localCorners[1];
		int num = (int)Mathf.Abs(val.y);
		((Component)((Component)label).transform.parent).GetComponent<UIWidget>().height = label.height + num * 2;
		label.SetPosition(component.localCorners[1] + val, 0f, 1f);
		UIUtility.UpdateAnchors(((Component)label).transform.parent);
	}

	public void UpdateData()
	{
		Init();
		if (_skill == null)
		{
			return;
		}
		_skillNameLabel.text = string.Format(_nameFormat, _skill.Name);
		_skillRankLabel.text = string.Format(_rankFormat, _skill.Level);
		UIUtility.UpdateAnchors(((Component)_learnButton).transform);
		SetDescription();
		SetReward();
		SetCondition();
		_scrollView.Reposition();
		if (_skill.Level <= _skill.Parent.Level)
		{
			if (_skill.Level == _skill.Parent.Level && GameSystem<SkillSystem>.Instance().Untrainable)
			{
				_learnButton.Text = T._("습득 취소");
				_learnButton.SetStyle(_learnButtonStyle);
				_learnButton.Disable = _skill.UntrainDisabled;
			}
			else
			{
				_learnButton.Text = T._("습득함");
				_learnButton.SetStyle(DefaultSelectableButton.ButtonStyle.Gray);
				_learnButton.Disable = true;
			}
			return;
		}
		int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_skill.Category);
		if (_skill.SkillPoints > 0)
		{
			_learnButton.Text = string.Format(_rankUpButtonFormat, _skill.SkillPoints, T._("랭크 업"));
			_learnButton.SetStyle(_learnButtonStyle);
			_learnButton.Disable = GameSystem<SkillSystem>.Instance().RemainSkillPoint < _skill.SkillPoints;
		}
		else
		{
			_learnButton.Text = T._("자동 습득");
			_learnButton.SetStyle(_learnButtonStyle);
			_learnButton.Disable = false;
		}
		_learnButton.Disable |= _skill.Level > _skill.Parent.Level + 1 || _skill.CategoryLevel > categoryLevel;
		_learnButton.Disable |= _skill.Parent.Parent.Base != _skill.Parent && _skill.Parent.Parent.Base.Level == 0;
	}
}
