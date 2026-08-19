using Durango.Logic;
using Durango.Logic.Skill;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Ability;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class IndicatorGroup : UIBase
{
	[SerializeField]
	private IndicatorList _indicator;

	private void Start()
	{
		GameSystem<FarmingEncyclopediaSystem>.Instance().FarmingDataUpdated += OnFarmingDataChanged;
		GameSystem<StatisticsSystem>.Instance().ExpGained += OnExpGained;
		GameSystem<SkillSystem>.Instance().SkillCategoryExperienced += OnSkillCategoryExperienced;
	}

	[ExposedInEditor(null)]
	public void Show(string icon, string text, Color iconColor, IndicatorWidget.Gauge? gauge = null)
	{
		_indicator.Show(icon, text, iconColor, gauge);
	}

	public void Show(string icon, string text)
	{
		Show(icon, text, Color.white);
	}

	private void OnFarmingDataChanged(string key, FarmingEncyclopediaData? prev, FarmingEncyclopediaData data)
	{
		if (prev.HasValue && prev.Value.CurrentLevel < data.CurrentLevel)
		{
			CropInfo cropInfo = SingletonDict<string, CropInfo>.Get(key);
			if (cropInfo != null)
			{
				Show("icon_encyclopedia_farming_small", T._("{0} {1:lv:} 달성", cropInfo.Name, data.CurrentLevel));
			}
		}
	}

	private void OnExpGained(ExpGained msg)
	{
		if (msg.EntityId == PlayerBehavior.LocalPlayer.EntityId)
		{
			if (msg.Exp > 0)
			{
				Show("icon_exp", (msg.BonusExp <= 0) ? msg.Exp.ToString() : $"{msg.Exp}[9F9F9F][size=16](+{msg.BonusExp})");
			}
			if (msg.ResistanceType.HasValue)
			{
				Derived value = msg.ResistanceType.Value;
				string icon = IconMap.Get(value);
				string text = string.Format("{0} +{1}", T._("신체 {0}", value.GetName()), msg.ResistanceExp);
				Pair<int, int> currentAndMaxResistanceExp = GameSystem<StatisticsSystem>.Instance().GetCurrentAndMaxResistanceExp(value);
				Show(icon, text, Color.white, new IndicatorWidget.Gauge(msg.ResistanceExp, currentAndMaxResistanceExp.Item1, currentAndMaxResistanceExp.Item2));
			}
			return;
		}
		Messages.Pet? pet = Durango.Utils.Singleton<PetManager>.Instance().GetPet(msg.EntityId);
		if (pet.HasValue && pet.Value.TamerEntityId == PlayerBehavior.LocalPlayer.EntityId)
		{
			if (msg.Exp + msg.BonusExp > 0)
			{
				Show("icon_exp_pet", (msg.BonusExp <= 0) ? msg.Exp.ToString() : $"{msg.Exp}[9F9F9F][size=16](+{msg.BonusExp})");
			}
			else
			{
				Show("icon_exp_pet_impossible", "FULL");
			}
		}
	}

	private void OnSkillCategoryExperienced(SkillCategoryExperienced msg)
	{
		string icon = Util.CategoryIcon(msg.Category) + "_small";
		IndicatorWidget.Gauge? gauge = null;
		string text;
		if (msg.Exp > 0)
		{
			GameSystem<SkillSystem>.Instance().GetCategoryExp(msg.Category, out var current, out var max);
			text = $"{msg.Category.GetName()} +{msg.Exp}";
			gauge = new IndicatorWidget.Gauge(msg.Exp, current, max);
		}
		else if (msg.ResearchReducedTime > 0.0)
		{
			text = T._("{0} 연구 -{1}", msg.Category.GetName(), TimedeltaFormatter.Format(msg.ResearchReducedTime));
		}
		else
		{
			Category skillCategory = GameSystem<SkillSystem>.Instance().GetSkillCategory(msg.Category);
			text = ((skillCategory == null || !skillCategory.IsReadyToResearch()) ? string.Format("{1} {0}", T._("FULL"), msg.Category.GetName()) : string.Format("{1} {0}", T._("연구 가능"), msg.Category.GetName()));
		}
		Show(icon, text, new Color32(74, 167, 186, byte.MaxValue), gauge);
	}
}
