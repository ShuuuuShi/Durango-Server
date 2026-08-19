using System;
using Shared.Skill;
using SkillData;
using UnityEngine;

public class SkillCategoryInfoWidget : MonoBehaviour
{
	public Action InfoButtonClicked;

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
	private DefaultSelectableButton _detailButton;

	private bool _isButtonVisible;

	private Category _category;

	private void Start()
	{
		DefaultSelectableButton detailButton = _detailButton;
		detailButton.Clicked = (Action)Delegate.Combine(detailButton.Clicked, (Action)delegate
		{
			if (InfoButtonClicked != null)
			{
				InfoButtonClicked();
			}
		});
	}

	private void OnEnable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated += OnUpdateSkills;
	}

	private void OnDisable()
	{
		GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnUpdateSkills;
	}

	private void OnUpdateSkills()
	{
		Refresh();
	}

	public void Set(Category category)
	{
		_category = category;
		Refresh();
	}

	private void Refresh()
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (_category == Category.Invalid)
		{
			_infoWidget.gameObject.SetActive(false);
			_unselectWidget.gameObject.SetActive(true);
			return;
		}
		_titleLabel.text = SkillUtil.CategoryLocalizeName(_category);
		_levelLabel.text = LocalizeUtil.FormatLevel(GameSystem<SkillSystem>.Instance().GetCategoryLevel(_category));
		_descriptionLabel.text = SkillUtil.CategoryLocalizeDescription(_category);
		_descriptionWidget.height = (int)Mathf.Abs(((Component)_descriptionLabel).transform.localPosition.y) * 2 + _descriptionLabel.height;
		int categoryUsedSp = GameSystem<SkillSystem>.Instance().GetCategoryUsedSp(_category);
		_usedSpLabel.text = categoryUsedSp.ToString();
		_progressGauge.Set(_category);
		_infoScroll.Reposition();
		_infoWidget.gameObject.SetActive(true);
		_unselectWidget.gameObject.SetActive(false);
	}

	public void ButtonVisible(bool isVisible)
	{
		if (_isButtonVisible != isVisible)
		{
			_isButtonVisible = isVisible;
			TweenAlpha.Begin(((Component)_buttonContainer).gameObject, 0.2f, (!isVisible) ? 0f : 1f);
			UIPanel uIPanel = _infoScroll.ScrollView.panel;
			if ((Object)(object)uIPanel == (Object)null)
			{
				uIPanel = ((Component)_infoScroll.ScrollView).GetComponent<UIPanel>();
			}
			uIPanel.bottomAnchor.absolute = (isVisible ? _buttonContainer.height : 0);
			uIPanel.UpdateAnchors();
		}
	}
}
