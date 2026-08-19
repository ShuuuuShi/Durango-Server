using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ArchipelagoDiscoveryInfos : MonoBehaviour
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private ArchipelagoInfo _archipelagoInfo;

	[SerializeField]
	private MissionInfo _missionInfo;

	[SerializeField]
	private BiocomInfo _biocomInfo;

	[SerializeField]
	private AnimalInfo _animalInfo;

	private readonly DiscoveryInfo[] _infos = new DiscoveryInfo[4];

	private void Awake()
	{
		_infos[0] = _archipelagoInfo;
		_infos[1] = _missionInfo;
		_infos[2] = _biocomInfo;
		_infos[3] = _animalInfo;
		Action value = delegate
		{
			_scrollView.Reposition();
		};
		DiscoveryInfo[] infos = _infos;
		foreach (DiscoveryInfo discoveryInfo in infos)
		{
			discoveryInfo.LayoutUpdated += value;
		}
	}

	public void SetSubject(string apparentClimate, int level)
	{
		_titleLabel.text = T._("불안정 {0} 해역\n[size=24]{1:lv:}[/size]", apparentClimate, level);
	}

	public void Set([CanBeNull] List<RegionTemplate> templates, ArchipelagoRoute archipelagoRoute, bool isUnstableFactorVisible)
	{
		DiscoveryInfo[] infos2 = _infos;
		foreach (DiscoveryInfo discoveryInfo in infos2)
		{
			discoveryInfo.ShowUnknown();
		}
		if (isUnstableFactorVisible)
		{
			_archipelagoInfo.Set(archipelagoRoute.Biome, archipelagoRoute.UnstableFactor);
		}
		_archipelagoInfo.gameObject.SetActive(isUnstableFactorVisible);
		if (KUtility.GetSize(templates) == 0)
		{
			_scrollView.Reposition();
			return;
		}
		UIManager.ShowLoadingIcon(show: true);
		GameSystem<MapSystem>.Instance().GetDiscoveryInfos(templates.Select((RegionTemplate template) => template.Id).ToList(), delegate(Messages.DiscoveryInfo[] infos)
		{
			UIManager.ShowLoadingIcon(show: false);
			Dictionary<string, bool> dictionary = new Dictionary<string, bool>();
			Dictionary<ushort, bool> dictionary2 = new Dictionary<ushort, bool>();
			for (int j = 0; j < infos.Length; j++)
			{
				Messages.DiscoveryInfo discoveryInfo2 = infos[j];
				Pair<string, bool>[] biocomNames = discoveryInfo2.BiocomNames;
				for (int k = 0; k < biocomNames.Length; k++)
				{
					Pair<string, bool> pair = biocomNames[k];
					if (!dictionary.Get(pair.Item1, defaultValue: false))
					{
						dictionary[pair.Item1] = pair.Item2;
					}
				}
				Pair<ushort, bool>[] animalTypes = discoveryInfo2.AnimalTypes;
				for (int l = 0; l < animalTypes.Length; l++)
				{
					Pair<ushort, bool> pair2 = animalTypes[l];
					if (!dictionary2.Get(pair2.Item1, defaultValue: false))
					{
						dictionary2[pair2.Item1] = pair2.Item2;
					}
				}
			}
			_biocomInfo.Set(dictionary);
			_animalInfo.Set(dictionary2);
			Messages.Archipelago archipelago = GameSystem<ExploreSystem>.Instance().GetArchipelago(archipelagoRoute.ArchipelagoId);
			bool flag = SingletonDict<string, Dictionary<string, ArchipelagoMission>>.Get(archipelago.TemplateId) != null;
			if (flag)
			{
				_missionInfo.Set(archipelago.IncludedRegions);
			}
			_missionInfo.gameObject.SetActive(flag);
			_scrollView.Reposition();
		});
	}
}
