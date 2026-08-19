using System.Collections.Generic;
using Durango.Logic.Clan;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class ClanLevelPage : ClanMenuPage, IUIInitializable
{
	[SerializeField]
	private UITexture _emblemTexure;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UIWidget _expGaugeWidget;

	[SerializeField]
	private UISprite _expGaugeUpper;

	[SerializeField]
	private UILabel _expGaugeLabel;

	[SerializeField]
	private KScrollView _levelRewards;

	private Clan _clan;

	private List<KeyValuePair<int, ClanLevelReward>> _levelRewardInfos;

	void IUIInitializable.Init()
	{
		InitLevelRewards();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		SetEmblem(-Point2.one);
		Set(playerClan);
	}

	private void InitLevelRewards()
	{
		_levelRewardInfos = new List<KeyValuePair<int, ClanLevelReward>>();
		foreach (KeyValuePair<int, ClanLevelReward> levelReward in Singleton<ClanYaml>.Instance.LevelRewards)
		{
			if (!string.IsNullOrEmpty(levelReward.Value.Description))
			{
				_levelRewardInfos.Add(levelReward);
			}
		}
		_levelRewardInfos.Sort((KeyValuePair<int, ClanLevelReward> r1, KeyValuePair<int, ClanLevelReward> r2) => r1.Key - r2.Key);
		ListObjectPool nodes = _levelRewards.Nodes;
		nodes.Set(_levelRewardInfos.Count);
		for (int i = 0; i < nodes.Count; i++)
		{
			KeyValuePair<int, ClanLevelReward> keyValuePair = _levelRewardInfos[i];
			nodes[i].GetComponent<ClanLevelInfoNode>().Set(keyValuePair.Key, keyValuePair.Value.Description);
		}
		_levelRewards.ResetPosition();
	}

	public void Set([NotNull] Clan clan)
	{
		_clan = clan;
		_nameLabel.text = _clan.Name;
		_levelLabel.text = $"[icon=icon_align_lv:1.2][-] {_clan.Level}";
		_levelLabel.SetPosition(_nameLabel.GetPosition(1f, 1f) + Vector3.right * 5f, 0f, 1f);
		ClanSystem.GetEmblem(_clan.Id, SetEmblem);
		SetLevel(_clan.Level);
		SetExp(_clan.Level, _clan.Exp);
	}

	private void SetLevel(int level)
	{
		ListObjectPool nodes = _levelRewards.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			KeyValuePair<int, ClanLevelReward> keyValuePair = _levelRewardInfos[i];
			nodes[i].GetComponent<ClanLevelInfoNode>().Disabled = keyValuePair.Key > level;
		}
		_levelRewards.Reposition();
	}

	private void SetEmblem(Point2 pos)
	{
		if (pos.x < 0 || pos.y < 0)
		{
			_noEmblem.gameObject.SetActive(value: true);
			_emblemTexure.gameObject.SetActive(value: false);
		}
		else
		{
			_noEmblem.gameObject.SetActive(value: false);
			_emblemTexure.gameObject.SetActive(value: true);
			EmblemTexture.Set(_emblemTexure, pos);
		}
	}

	private void SetExp(int level, long exp)
	{
		ClanSystem.GetExpRange(level, out var min, out var max);
		long num = max - min;
		long num2 = exp - min;
		float num3 = Mathf.Clamp01((float)num2 / (float)num);
		_expGaugeLabel.text = T._("{0:lv:} ( <em>{1:N0}</em> [FFFFFF7F]/[-] {2:N0} )", level, num2, num);
		_expGaugeUpper.width = (int)((float)_expGaugeWidget.width * num3);
		_expGaugeUpper.alpha = ((!(num3 > 0f)) ? 0f : 1f);
	}
}
