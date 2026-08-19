using System;
using System.Collections.Generic;
using L10N;
using Messages;
using Shared.Skill;
using SkillData;
using StatisticsData;
using UnityEngine;

public class AutoGuideInfoPopup : MonoBehaviour
{
	[SerializeField]
	private UILabel _progressLabel;

	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private ListObjectPool _explainPool;

	[SerializeField]
	private DefaultSelectableButton _continueButton;

	[SerializeField]
	private DefaultSelectableButton _cancelButton;

	private Color _defaultColor;

	private void Awake()
	{
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		DefaultSelectableButton continueButton = _continueButton;
		continueButton.Clicked = (Action)Delegate.Combine(continueButton.Clicked, new Action(ContinueButton_Clicked));
		DefaultSelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(CancelButton_Clicked));
		_explainPool.Init(null);
		UILabel component = _explainPool.BaseObject.GetComponent<UILabel>();
		_defaultColor = component.color;
	}

	private void ContinueButton_Clicked()
	{
		((Component)this).gameObject.SetActive(false);
	}

	private static void CancelButton_Clicked()
	{
		UIManager.MessageBox.Show(T._("목표를 변경하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				Connections.Frontend.Send(default(CancelTargetTitle));
			}
		});
	}

	public void Show()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0261: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).gameObject.SetActive(true);
		_progressLabel.text = T._("나의 목표 달성율 {0}%", GameSystem<AutoGuideSystem>.Instance().Progress);
		StatisticsData.Title targetTitle = GameSystem<AutoGuideSystem>.Instance().TargetTitle;
		SkillSystem skillSystem = GameSystem<SkillSystem>.Instance();
		int num = targetTitle.RequiredSkillCount();
		int num2 = ((targetTitle.CategoryLevels != null) ? targetTitle.CategoryLevels.Length : 0);
		num2 += num;
		_explainPool.Set(num2);
		Vector3 localPosition = _explainPool.BaseObject.transform.localPosition;
		int num3 = 0;
		if (targetTitle.CategoryLevels != null)
		{
			int num4 = targetTitle.CategoryLevels.Length;
			for (int i = 0; i < num4; i++)
			{
				KeyValuePair<Category, int> keyValuePair = targetTitle.CategoryLevels[i];
				int value = keyValuePair.Value;
				int categoryLevel = skillSystem.GetCategoryLevel(keyValuePair.Key);
				bool flag = value <= categoryLevel;
				UILabel uILabel = ((ListObjectPoolBase<GameObject>)_explainPool).Get<UILabel>(num3);
				uILabel.text = T._("{0} 계열 <em>{1:lv:}</em> 이상 (현재 <em>{2}</em>)", LocalizeUtil.Get(keyValuePair.Key), value, categoryLevel);
				uILabel.color = ((!flag) ? _defaultColor : PresetColor.UIYellow);
				Vector3 localPosition2 = localPosition;
				localPosition2.y -= (float)(num3 * 35);
				((Component)uILabel).transform.localPosition = localPosition2;
				Transform child = ((Component)uILabel).transform.GetChild(0);
				((Component)child).gameObject.SetActive(flag);
				num3++;
			}
		}
		for (int j = 0; j < num; j++)
		{
			SkillNode requiredSkill = targetTitle.GetRequiredSkill(j);
			SkillData.Skill skill = skillSystem.FindSkill(requiredSkill.Id, requiredSkill.Sub);
			int level = requiredSkill.Level;
			int num5 = skill?.Level ?? 0;
			bool flag2 = level <= num5;
			UILabel uILabel2 = ((ListObjectPoolBase<GameObject>)_explainPool).Get<UILabel>(num3);
			uILabel2.text = T._("{0} <em>랭크 {1}</em> 이상 (현재 <em>{2}</em>)", requiredSkill.Name, level, num5);
			uILabel2.color = ((!flag2) ? _defaultColor : PresetColor.UIYellow);
			Vector3 localPosition3 = localPosition;
			localPosition3.y -= (float)(num3 * 35);
			((Component)uILabel2).transform.localPosition = localPosition3;
			Transform child2 = ((Component)uILabel2).transform.GetChild(0);
			((Component)child2).gameObject.SetActive(flag2);
			num3++;
		}
		_scrollView.ResetPosition();
	}
}
