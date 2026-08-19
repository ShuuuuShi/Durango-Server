using L10N;
using Messages;
using Shared.Ability;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ArchipelagoInfo : DiscoveryInfo
{
	[SerializeField]
	private UILabel _collectingPower;

	[SerializeField]
	private UILabel _resistance;

	public override void ShowUnknown()
	{
		base.gameObject.SetActive(value: false);
	}

	public void Set(Biome biome, int unstableFactor)
	{
		if (!_Set(biome, unstableFactor))
		{
			ShowUnknown();
		}
	}

	public bool _Set(Biome biome, int unstableFactor)
	{
		Derived derived = Singleton<Constants>.Instance.Resistance.TypeByBiome.Get(biome, Derived.Invalid);
		if (derived == Derived.Invalid)
		{
			return false;
		}
		Recommends recommends = SingletonDict<int, Recommends>.Instance.Get(unstableFactor);
		if (recommends == null)
		{
			return false;
		}
		Statistics? statistics = GameSystem<StatisticsSystem>.Instance().Statistics;
		if (!statistics.HasValue)
		{
			return false;
		}
		string countLabel = $"<em>{unstableFactor}</em>";
		SetCountLabel(countLabel);
		float num = statistics.Value.RepresentPowers.Get(RepresentType.CollectingPower, 0f);
		bool flag = num >= (float)recommends.CollectingPower;
		string text = ((!flag) ? "[icon=img_arrow_down:0.5]" : string.Empty);
		_collectingPower.color = ((!flag) ? PresetColor.ExploreRed : Color.white);
		_collectingPower.text = string.Format(T.Culture, "{0:N0} {1}", recommends.CollectingPower, text);
		int num2 = statistics.Value.ResistanceLevels.Get(derived, 0);
		bool flag2 = num2 >= recommends.ResistanceLevel;
		text = ((!flag2) ? " [icon=img_arrow_down:0.5]" : string.Empty);
		_resistance.color = ((!flag2) ? PresetColor.ExploreRed : Color.white);
		_resistance.text = string.Format(T.Culture, "[c][icon={0}][/c] {1:N0} {2}", IconMap.Get(biome), recommends.ResistanceLevel, text);
		_layout.UpdateLayout();
		return true;
	}
}
