using System;
using System.Text;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Skill;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class SkillCategoryProgressGauge : UIWidget
{
	[SerializeField]
	private UILabel _normalTitleLabel;

	[SerializeField]
	private UILabel _readyToResearchTitleLabel;

	[SerializeField]
	private UILabel _researchingTitleLabel;

	[SerializeField]
	private UIWidget _normalWidget;

	[SerializeField]
	private UIWidget _readyToResearchWidget;

	[SerializeField]
	private UIWidget _researchingWidget;

	[SerializeField]
	private Selectable _researchBtn;

	[SerializeField]
	private UILabel _researchBtnLabel;

	[SerializeField]
	private UISprite _expUpperSprite;

	[SerializeField]
	private UILabel _expLabel;

	[SerializeField]
	private UILabel _researchLabel;

	[SerializeField]
	private UISpriteLabel _researchTimerLabel;

	[SerializeField]
	private UILabel _researchingHelpTooltip;

	[SerializeField]
	private Selectable _researchSkipButton;

	[SerializeField]
	private Selectable _researchCancelButton;

	private Durango.Logic.Skill.Category _category;

	private float _nextResearchingTimerUpdate;

	private UIWidget[] _mainWidgets;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		Selectable researchBtn = _researchBtn;
		researchBtn.Clicked = (Action)Delegate.Combine(researchBtn.Clicked, new Action(OnClickResearch));
		Selectable researchSkipButton = _researchSkipButton;
		researchSkipButton.Clicked = (Action)Delegate.Combine(researchSkipButton.Clicked, new Action(OnClickSkipResearch));
		Selectable researchCancelButton = _researchCancelButton;
		researchCancelButton.Clicked = (Action)Delegate.Combine(researchCancelButton.Clicked, new Action(OnClickCancelResearch));
		UILabel normalTitleLabel = _normalTitleLabel;
		string text = string.Format("<em>[icon=icon_skill_exp]</em> {0}", T._("숙련도"));
		_readyToResearchTitleLabel.text = text;
		normalTitleLabel.text = text;
		_researchingTitleLabel.text = string.Format("<em>[icon=icon_skill_start]</em> {0}", T._("남은 시간"));
		_mainWidgets = new UIWidget[3] { _normalWidget, _readyToResearchWidget, _researchingWidget };
		_researchingHelpTooltip.text = string.Format("{0} [icon=icon_question_big]", T._("연구 시간 줄이기"));
		UIEventListener uIEventListener = UIEventListener.Get(_researchingHelpTooltip.gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickResearchingToolip));
		UIEventListener uIEventListener2 = UIEventListener.Get(_researchingHelpTooltip.gameObject);
		uIEventListener2.onHover = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onHover, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isHover)
		{
			if (isHover)
			{
				OnClickResearchingToolip(go);
			}
			else
			{
				UIManager.Popup.Tooltip<WidgetTooltipControl>().Hide();
			}
		});
	}

	private void SetMainWidget(UIWidget widget)
	{
		base.height = widget.height;
		widget.SetPosition(base.localCenter, 0.5f, 0.5f);
		int i = 0;
		for (int size = KUtility.GetSize(_mainWidgets); i < size; i++)
		{
			_mainWidgets[i].gameObject.SetActive(_mainWidgets[i] == widget);
		}
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Set(Shared.Skill.Category category)
	{
		Init();
		_category = GameSystem<SkillSystem>.Instance().GetSkillCategory(category);
		if (_category.IsResearching())
		{
			base.gameObject.SetActive(value: true);
			SetMainWidget(_researchingWidget);
			UpdateResearchingTimer();
			return;
		}
		GameSystem<SkillSystem>.Instance().GetCategoryExp(_category.Type, out var current, out var max);
		if (max > 0)
		{
			base.gameObject.SetActive(value: true);
			_expUpperSprite.fillAmount = (float)current / (float)max;
			if (current < max)
			{
				SetMainWidget(_normalWidget);
				_expLabel.text = $"{current} / {max}";
				return;
			}
			int level = GameSystem<StatisticsSystem>.Instance().Level;
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_category.Type);
			if (categoryLevel < level)
			{
				SetMainWidget(_readyToResearchWidget);
				Yaml.SkillCategory skillCategory = SingletonDict<Shared.Skill.Category, Yaml.SkillCategory>.Get(_category.Type);
				int num = skillCategory.ResearchTimes.Get(_category.Level, 0);
				Pair<float, Durango.Logic.StatusEffect> researchTimeReducer = GetResearchTimeReducer(_category.Level);
				if (researchTimeReducer.Item1 > 0f && num > 0)
				{
					float num2 = Mathf.Max(0f, (float)num * (1f - researchTimeReducer.Item1));
					_researchBtnLabel.text = string.Format("{0} [icon=icon_timer] [s]{1}[/s] {2}", T._("연구 시작"), TimedeltaFormatter.Format(num), (!(num2 > 0f)) ? T._("즉시") : TimedeltaFormatter.Format(num2));
				}
				else
				{
					_researchBtnLabel.text = ((num <= 0) ? T._("연구 시작") : string.Format("{0} [icon=icon_timer] {1}", T._("연구 시작"), TimedeltaFormatter.Format(num)));
				}
			}
			else
			{
				SetMainWidget(_normalWidget);
				_expLabel.text = T._("최대치");
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private static Pair<float, Durango.Logic.StatusEffect> GetResearchTimeReducer(int level)
	{
		float num = 0f;
		float num2 = 0f;
		Durango.Logic.StatusEffect item = null;
		if (level <= 50)
		{
			Durango.Logic.StatusEffects statusEffects = GameSystem<StatusEffectSystem>.Instance().GetStatusEffects();
			foreach (Durango.Logic.StatusEffect item2 in statusEffects.List)
			{
				foreach (Messages.EffectDetail effectDetail in item2.EffectDetails)
				{
					if (effectDetail.Key.StartsWith("reduce_skill_research"))
					{
						float value = effectDetail.Value;
						num2 += value;
						if (num < value)
						{
							num = value;
							item = item2;
						}
					}
				}
			}
		}
		return new Pair<float, Durango.Logic.StatusEffect>(num2, item);
	}

	protected override void OnUpdate()
	{
		if (Application.isPlaying && _category != null && _category.IsResearching() && _nextResearchingTimerUpdate > 0f && _nextResearchingTimerUpdate < Time.time)
		{
			UpdateResearchingTimer();
		}
	}

	private void UpdateResearchingTimer()
	{
		double researchEnd = _category.ResearchEnd;
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		float num = 0f;
		double num2 = researchEnd - predictedServerTime;
		if (num2 > 0.0)
		{
			if (_category.ResearchReducedTime > 0.0)
			{
				Yaml.SkillCategory skillCategory = SingletonDict<Shared.Skill.Category, Yaml.SkillCategory>.Get(_category.Type);
				int num3 = skillCategory.ResearchTimes.Get(_category.Level, 0);
				float num4 = Mathf.Max((float)(_category.ResearchReducedTime / (double)num3), 0.01f);
				_researchTimerLabel.text = $"{TimedeltaFormatter.Format(num2)}  [preset=round_box?{$"{num4:P0} [icon=skill_arrow_down]"}]";
			}
			else
			{
				_researchTimerLabel.text = TimedeltaFormatter.Format(num2);
			}
			num = TimedeltaFormatter.NextPeriod(num2);
		}
		else
		{
			_researchTimerLabel.text = string.Empty;
		}
		int num5 = (int)_category.ResearchSkipCost.Get();
		if (num5 > 0)
		{
			_researchLabel.text = T._("즉시 완료! {0}", Durango.Logic.Item.Inventory.CurrencyFormat(num5, Currency.Gem));
			double num6 = _category.ResearchSkipCost.When(num5 - 1);
			if (num6 > predictedServerTime)
			{
				float num7 = (float)(num6 - predictedServerTime);
				num = ((!(num > 0f)) ? num7 : Mathf.Min(num, num7));
			}
		}
		else
		{
			_researchLabel.text = T._("즉시 완료!");
		}
		if (num > 0f)
		{
			_nextResearchingTimerUpdate = Time.time + num;
		}
		else
		{
			_nextResearchingTimerUpdate = 0f;
		}
	}

	private void OnClickResearch()
	{
		Durango.Logic.Skill.Category researching = GameSystem<SkillSystem>.Instance().GetResearchingCategory();
		string lowerText = null;
		Pair<float, Durango.Logic.StatusEffect> researchTimeReducer = GetResearchTimeReducer(_category.Level);
		Durango.Logic.StatusEffect item = researchTimeReducer.Item2;
		if (item != null)
		{
			lowerText = T._("<em>{0}</em> 의 효과로 전체 연구시간이 {1:P0} 단축됩니다.  <bar/>  <em>{2} 남음</em>", item.Name, researchTimeReducer.Item1, Times.GetRemainTime(item.Until));
		}
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.SetLowerText(lowerText);
		if (researching != null)
		{
			int skipCost = (int)researching.ResearchSkipCost.Get();
			messageBox.ShowPayConfirm(skipCost, Currency.Gem, T._("이미 <em>{0}</em> 연구가 진행중 입니다.\n즉시 완료하고 <em>{1}</em>{1:-을} 연구 시작하시겠습니까?", Durango.Logic.Skill.Util.CategoryLocalizeName(researching.Type), Durango.Logic.Skill.Util.CategoryLocalizeName(_category.Type)), delegate(bool ok)
			{
				if (ok)
				{
					GameSystem<SkillSystem>.Instance().ResearchSkillCategory(_category.Type, researching.Type, skipCost);
				}
			});
			return;
		}
		string confirm = null;
		if (researchTimeReducer.Item1 >= 1f)
		{
			confirm = T._("즉시완료");
		}
		messageBox.Show(T._("{0:을} 연구 시작하시겠습니까?", Durango.Logic.Skill.Util.CategoryLocalizeName(_category.Type)), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SkillSystem>.Instance().ResearchSkillCategory(_category.Type);
			}
		}, confirm);
	}

	private void OnClickSkipResearch()
	{
		int num = (int)_category.ResearchSkipCost.Get();
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.ShowPayConfirm(num, Currency.Gem, T._("<em>{0}</em>{0:-을} 즉시 완료 하시겠습니까?", Durango.Logic.Skill.Util.CategoryLocalizeName(_category.Type)), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SkillSystem>.Instance().SkipResearchSkillCategory(_category.Type);
			}
		});
	}

	private void OnClickCancelResearch()
	{
		UIManager.MessageBox.Show(T._("<em>{0}</em> 연구를 취소하시겠습니까? 지금까지의 연구 진행이 모두 취소됩니다.", Durango.Logic.Skill.Util.CategoryLocalizeName(_category.Type)), delegate(bool ok)
		{
			if (ok)
			{
				GameSystem<SkillSystem>.Instance().CancelResearchSkillCategory(_category.Type);
			}
		});
	}

	private void OnClickResearchingToolip(GameObject obj)
	{
		WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
		float researchReduceTimeLimit = Yaml.Util.Singleton<Constants>.Instance.Skill.ResearchReduceTimeLimit;
		Yaml.SkillCategory skillCategory = SingletonDict<Shared.Skill.Category, Yaml.SkillCategory>.Get(_category.Type);
		if (skillCategory == null)
		{
			return;
		}
		int num = skillCategory.ResearchTimes.Get(_category.Level, 0);
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value = reusable.Value;
			value.Append(T._("연구 시간 중 숙련을 얻으면 연구 시간을 <em>최대 {0:P0}</em> 단축할 수 있습니다.", researchReduceTimeLimit));
			value.Append("<br>20</br>");
			value.AppendLine(T._("현재 단축 시간 <em>{0}</em>", TimedeltaFormatter.Format(_category.ResearchReducedTime)));
			value.AppendLine(T._("최대 단축 시간 <em>{0}</em>", TimedeltaFormatter.Format((float)num * researchReduceTimeLimit)));
			if (_category.ResearchReduceRate > 0f)
			{
				value.AppendLine();
				value.AppendLine(T._("스킬 연구 시간 감소 효과로 인해 전체 연구 시간의 <em>{0:P0}</em>가 단축됩니다.", _category.ResearchReduceRate));
			}
			widgetTooltipControl.Set(null, value.ToString().Trim(), 400);
		}
		widgetTooltipControl.AutoPosition = false;
		widgetTooltipControl.Show();
		Vector2 printedSize = _researchingHelpTooltip.printedSize;
		Vector3 position = _researchingHelpTooltip.localCorners[1];
		position += new Vector3(printedSize.x, 0f - printedSize.y);
		position += new Vector3((float)(-_researchingHelpTooltip.fontSize) * 0.5f, -5f);
		position = _researchingHelpTooltip.transform.TransformPoint(position);
		position = widgetTooltipControl.transform.parent.InverseTransformPoint(position);
		widgetTooltipControl.Widget.SetPosition(position, 0.5f, 1f);
	}
}
