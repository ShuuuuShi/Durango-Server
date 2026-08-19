using System;
using L10N;
using Shared.Economy;
using Shared.Skill;
using SkillData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class SkillCategoryProgressGauge : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _label;

	[SerializeField]
	private GameObject _expProgress;

	[SerializeField]
	private Selectable _researchBtn;

	[SerializeField]
	private UISpriteLabel _researchBtnLabel;

	[SerializeField]
	private Selectable _researchProgress;

	[SerializeField]
	private UISprite _expUpperSprite;

	[SerializeField]
	private UILabel _expLabel;

	[SerializeField]
	private UISpriteLabel _researchLabel;

	[SerializeField]
	private UILabel _researchTimerLabel;

	private SkillData.SkillCategory _category;

	private float _updateTimer;

	private string _researchBtnFormat;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			Selectable researchBtn = _researchBtn;
			researchBtn.Clicked = (Action)Delegate.Combine(researchBtn.Clicked, new Action(OnClickResearch));
			Selectable researchProgress = _researchProgress;
			researchProgress.Clicked = (Action)Delegate.Combine(researchProgress.Clicked, new Action(OnClickSkipResearch));
			_researchBtnFormat = _researchBtnLabel.text;
		}
	}

	public void Set(Category category)
	{
		Init();
		_category = GameSystem<SkillSystem>.Instance().GetSkillCategory(category);
		if (_category.IsResearching())
		{
			((Component)this).gameObject.SetActive(true);
			_expProgress.gameObject.SetActive(false);
			((Component)_researchBtn).gameObject.SetActive(false);
			((Component)_researchProgress).gameObject.SetActive(true);
			_label.text = string.Format("[FFD85B][icon_skill_start][-] {0}", T._("남은 시간"));
			UpdateResearchingTimer();
			return;
		}
		_label.text = string.Format("[FFD85B][icon_skill_exp][-] {0}", T._("숙련도"));
		GameSystem<SkillSystem>.Instance().GetCategoryExp(_category.Category, out var current, out var max);
		if (max > 0)
		{
			((Component)this).gameObject.SetActive(true);
			_expUpperSprite.fillAmount = (float)current / (float)max;
			if (current < max)
			{
				_expProgress.gameObject.SetActive(true);
				((Component)_researchBtn).gameObject.SetActive(false);
				((Component)_researchProgress).gameObject.SetActive(false);
				_expLabel.text = T._("{0} / {1}", current, max);
				return;
			}
			int level = GameSystem<StatisticsSystem>.Instance().Level;
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_category.Category);
			if (categoryLevel < level)
			{
				_expProgress.gameObject.SetActive(false);
				((Component)_researchBtn).gameObject.SetActive(true);
				((Component)_researchProgress).gameObject.SetActive(false);
				Yaml.SkillCategory skillCategory = SingletonDict<Category, Yaml.SkillCategory>.Get(_category.Category);
				int num = skillCategory.research_times.Get(_category.Level, 0);
				_researchBtnLabel.text = ((num <= 0) ? T._("연구 시작") : string.Format(_researchBtnFormat, T._("연구 시작"), TimerSystem.TimeToString(num)));
			}
			else
			{
				_expProgress.gameObject.SetActive(true);
				((Component)_researchBtn).gameObject.SetActive(false);
				((Component)_researchProgress).gameObject.SetActive(false);
				_expLabel.text = T._("최대치");
			}
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void Update()
	{
		if (_category != null && _category.IsResearching())
		{
			if (_updateTimer > 0f)
			{
				_updateTimer -= Time.deltaTime;
			}
			else
			{
				UpdateResearchingTimer();
			}
		}
	}

	private void UpdateResearchingTimer()
	{
		double researchEnd = _category.ResearchEnd;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		double num = researchEnd - predictedServerTime;
		_researchTimerLabel.text = T._("{0:sec:}", num);
		int num2 = (int)_category.ResearchSkipCost.Get();
		_researchLabel.text = ((num2 <= 0) ? T._("즉시 완료!") : T._("즉시 완료! <gem></gem>{0}", num2));
		_updateTimer = 1f;
	}

	private void OnClickResearch()
	{
		SkillData.SkillCategory researching = GameSystem<SkillSystem>.Instance().GetResearchingCategory();
		if (researching != null)
		{
			int skipCost = (int)researching.ResearchSkipCost.Get();
			GameSystem<InventorySystem>.Instance().PlayerInventory.ShowPayConfirm(skipCost, Currency.Gem, T._("이미 <em>{2}</em> 연구가 진행중 입니다.\n{0:으로} 즉시 완료하고 <em>{3}</em>{3:-을} 연구 시작하시겠습니까?\n현재 보유량 {1}"), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<SkillSystem>.Instance().ResearchSkillCategory(_category.Category, researching.Category, skipCost);
				}
			}, SkillUtil.CategoryLocalizeName(researching.Category), SkillUtil.CategoryLocalizeName(_category.Category));
			return;
		}
		UIManager.MessageBox.Show(T._("{0:을} 연구 시작하시겠습니까?", SkillUtil.CategoryLocalizeName(_category.Category)), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SkillSystem>.Instance().ResearchSkillCategory(_category.Category);
			}
		});
	}

	private void OnClickSkipResearch()
	{
		int amount = (int)_category.ResearchSkipCost.Get();
		GameSystem<InventorySystem>.Instance().PlayerInventory.ShowPayConfirm(amount, Currency.Gem, T._("연구 즉시 완료에는 {0:가} 필요합니다.\n현재 보유량 {1}"), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SkillSystem>.Instance().SkipResearchSkillCategory(_category.Category);
			}
		});
	}
}
