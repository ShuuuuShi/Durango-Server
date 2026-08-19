using System;
using Durango.Logic;
using Durango.Logic.Skill;
using Durango.UI.Control;
using Shared.Skill;
using UnityEngine;

namespace Durango.UI;

public class SkillCategoryInfoWidget : MonoBehaviour
{
	[SerializeField]
	private GameObject _unselectWidget;

	[SerializeField]
	private GameObject _infoWidget;

	[SerializeField]
	private KWidgetScrollView _infoScroll;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private SkillLearningTag _skillLearningTag;

	[SerializeField]
	private UIWidget _eventNotice;

	[SerializeField]
	private UILabel _eventPercentLabel;

	[SerializeField]
	private UILabel _eventRemainLabel;

	[SerializeField]
	private SkillCategoryProgressGauge _progressGauge;

	[SerializeField]
	private UILabel _usedSpLabel;

	[SerializeField]
	private UIWidget _descriptionWidget;

	[SerializeField]
	private UILabel _descriptionLabel;

	[SerializeField]
	private UIWidget _buttonContainer;

	[SerializeField]
	private SelectableButton _detailButton;

	private bool _showButtonContainer;

	private Shared.Skill.Category _category;

	public SelectableButton DetailButton => _detailButton;

	public event Action InfoButtonClicked;

	private void Start()
	{
		SelectableButton detailButton = _detailButton;
		detailButton.Clicked = (Action)Delegate.Combine(detailButton.Clicked, (Action)delegate
		{
			if (this.InfoButtonClicked != null)
			{
				this.InfoButtonClicked();
			}
		});
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnUpdateSkills;
		bool flag = UIManager.IsPortraitWidget(base.gameObject);
		_titleWidget.gameObject.SetActive(!flag);
		_infoScroll.Panel.topAnchor.absolute = ((!flag) ? (-_titleWidget.height) : 0);
		_infoScroll.Panel.UpdateAnchors();
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnUpdateSkills;
	}

	private void OnUpdateSkills()
	{
		Refresh();
	}

	public void Set(Shared.Skill.Category category)
	{
		_category = category;
		Refresh();
	}

	private void Refresh()
	{
		if (_category == Shared.Skill.Category.Invalid)
		{
			_infoWidget.gameObject.SetActive(value: false);
			_unselectWidget.gameObject.SetActive(value: true);
			return;
		}
		_titleLabel.text = Util.CategoryLocalizeName(_category);
		Durango.Logic.Skill.Category skillCategory = GameSystem<SkillSystem>.Instance().GetSkillCategory(_category);
		if (skillCategory != null)
		{
			_levelLabel.text = LocalizeUtil.FormatLevel(skillCategory.Level);
			_skillLearningTag.Refresh(skillCategory);
		}
		_descriptionLabel.text = Util.CategoryLocalizeDescription(_category);
		_descriptionWidget.height = (int)Mathf.Abs(_descriptionLabel.transform.localPosition.y) * 2 + _descriptionLabel.height;
		int categoryUsedSp = GameSystem<SkillSystem>.Instance().GetCategoryUsedSp(_category);
		_usedSpLabel.text = categoryUsedSp.ToString();
		_progressGauge.Set(_category);
		RefreshEventNotice();
		_infoScroll.Reposition();
		_infoWidget.gameObject.SetActive(value: true);
		_unselectWidget.gameObject.SetActive(value: false);
	}

	private void RefreshEventNotice()
	{
		float value;
		StatusEffect effect = SkillSystem.FindSkillExpModifier(_category, out value);
		_eventNotice.gameObject.SetActive(effect != null);
		if (effect != null)
		{
			SyncString text2 = ((!(effect.Until > 0.0)) ? ((SyncString)string.Empty) : new SyncString(delegate(out string text, out float period)
			{
				float remainTime = effect.GetRemainTime();
				text = TimedeltaFormatter.Format(remainTime);
				period = TimedeltaFormatter.NextPeriod(remainTime);
			}));
			_eventRemainLabel.SetText(text2);
			_eventPercentLabel.text = $"<em>{value:P0} [icon=skill_up_arrow]</em>";
		}
	}

	public void ShowButtonContainer(bool show)
	{
		if (_showButtonContainer != show)
		{
			_showButtonContainer = show;
			TweenAlpha.Begin(_buttonContainer.gameObject, 0.2f, (!show) ? 0f : 1f);
			RefreshButtonContainerHeight();
		}
	}

	private void RefreshButtonContainerHeight()
	{
		UIPanel uIPanel = _infoScroll.ScrollView.panel;
		if (uIPanel == null)
		{
			uIPanel = _infoScroll.ScrollView.GetComponent<UIPanel>();
		}
		uIPanel.bottomAnchor.absolute = (_showButtonContainer ? _buttonContainer.topAnchor.absolute : 0);
		uIPanel.UpdateAnchors();
	}
}
