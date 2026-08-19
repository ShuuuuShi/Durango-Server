using System.Collections.Generic;
using Durango.Logic;
using Durango.UI.Control;
using Messages;
using Shared.Ability;
using Shared.Region;
using Shared.StatusEffect;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FatigueWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _totalVelocity;

	[SerializeField]
	private UISprite _velocitySign;

	[SerializeField]
	private KScrollView _fatigueMomentums;

	[SerializeField]
	private GameObject _noData;

	private void OnEnable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated += OnUpdateFatigue;
		OnUpdateFatigue();
	}

	private void OnDisable()
	{
		GameSystem<FatigueSystem>.Instance().FatigueUpdated -= OnUpdateFatigue;
	}

	private void OnUpdateFatigue()
	{
		float fatigueVelocity = GameSystem<FatigueSystem>.Instance().FatigueVelocity;
		float num = Mathf.Abs((float)Mathf.RoundToInt(fatigueVelocity * 60f * 10f) / 10f);
		if (num > 0f)
		{
			_totalVelocity.text = $"{num:F1}";
			_velocitySign.alpha = 1f;
			_velocitySign.flip = ((fatigueVelocity > 0f) ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing);
			_velocitySign.color = ((!(fatigueVelocity > 0f)) ? PresetColor.UISkyBlue : PresetColor.UILightRed);
		}
		else
		{
			_totalVelocity.text = "0";
			_velocitySign.alpha = 0f;
		}
		_velocitySign.UpdateAnchors();
		List<FatigueVelocity> fatigueVelocities = GameSystem<FatigueSystem>.Instance().FatigueVelocities;
		ListObjectPool nodes = _fatigueMomentums.Nodes;
		nodes.BeginLoad();
		for (int i = 0; i < fatigueVelocities.Count; i++)
		{
			FatigueVelocity fatigueVelocity2 = fatigueVelocities[i];
			if (Mathf.Abs(fatigueVelocity2.Value) > 0f)
			{
				nodes.GetNext().GetComponent<FatigueMomentum>().Set(fatigueVelocity2);
			}
		}
		foreach (Durango.Logic.StatusEffect item in GameSystem<StatusEffectSystem>.Instance().GetStatusEffects().List)
		{
			if (Mathf.Abs(item.GetDetail(EffectType.Survival, "fatigue")) > 0f)
			{
				nodes.GetNext().GetComponent<FatigueMomentum>().Set(item);
			}
		}
		BiomeFatigue? biomeFatigue = GameSystem<FatigueSystem>.Instance().BiomeFatigue;
		if (biomeFatigue.HasValue && biomeFatigue.Value.Velocity > 0f)
		{
			Biome key = GameManager.Region.MajorBiome();
			Derived derived = Singleton<Constants>.Instance.Resistance.TypeByBiome.Get(key, Derived.Invalid);
			if (derived != Derived.Invalid)
			{
				nodes.GetNext().GetComponent<FatigueMomentum>().Set(biomeFatigue.Value, derived);
			}
		}
		nodes.EndLoad();
		_fatigueMomentums.Reposition();
		_noData.gameObject.SetActive(nodes.Count == 0);
	}
}
