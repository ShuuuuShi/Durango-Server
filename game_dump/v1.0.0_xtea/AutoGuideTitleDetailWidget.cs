using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using L10N;
using Shared.Skill;
using SkillData;
using StatisticsData;
using UnityEngine;

public class AutoGuideTitleDetailWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UISprite _achievmentBar;

	[SerializeField]
	private UISprite _achievmentGrunge;

	[SerializeField]
	private UILabel _achievmentPercent;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UILabel _explainLabel;

	[SerializeField]
	private UILabel _conditionsLabel;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private DefaultSelectableButton _confirmButton;

	public event Action ConfirmButtonClicked;

	private void Awake()
	{
		DefaultSelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(ConfirmButton_Clicked));
	}

	private void ConfirmButton_Clicked()
	{
		if (this.ConfirmButtonClicked != null)
		{
			this.ConfirmButtonClicked();
		}
	}

	public void Set([CanBeNull] Title title)
	{
		if (title == null)
		{
			return;
		}
		_titleLabel.text = title.Name;
		float achievementRatio = GameSystem<AutoGuideSystem>.Instance().GetAchievementRatio(title.Id);
		_achievmentBar.fillAmount = achievementRatio;
		_achievmentGrunge.fillAmount = achievementRatio;
		_achievmentPercent.text = $"{(int)(achievementRatio * 100f)}%";
		((Component)_confirmButton).gameObject.SetActive(achievementRatio < 1f);
		UIRect component = ((Component)_scrollView).GetComponent<UIRect>();
		component.bottomAnchor.absolute = ((!(achievementRatio < 1f)) ? 2 : 120);
		_levelLabel.text = T._("<em>{0:lv:}</em>", title.ExptectedLevelOfAchieved);
		_explainLabel.text = title.Description;
		StringBuilder stringBuilder = new StringBuilder();
		if (title.CategoryLevels != null)
		{
			int num = title.CategoryLevels.Length;
			for (int i = 0; i < num; i++)
			{
				KeyValuePair<Category, int> keyValuePair = title.CategoryLevels[i];
				stringBuilder.Append(T._("- {0} 계열 <em>{1:lv:}</em>", LocalizeUtil.Get(keyValuePair.Key), keyValuePair.Value));
				stringBuilder.Append("\n");
			}
		}
		int num2 = title.RequiredSkillCount();
		for (int j = 0; j < num2; j++)
		{
			SkillNode requiredSkill = title.GetRequiredSkill(j);
			if (requiredSkill == null)
			{
				Debug.LogError((object)$"No skill of {title.Name}[{j}]");
				continue;
			}
			stringBuilder.Append(T._("- {0} <em>랭크 {1}</em>", requiredSkill.Name, requiredSkill.Level));
			if (j != num2 - 1)
			{
				stringBuilder.Append("\n");
			}
		}
		_conditionsLabel.text = stringBuilder.ToString();
		_scrollView.ResetPosition();
		((Component)this).BroadcastMessage("UpdateAnchors");
	}
}
