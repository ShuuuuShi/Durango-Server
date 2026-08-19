using System;
using System.Collections.Generic;
using ClanData;
using JetBrains.Annotations;
using L10N;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class ClanLevelPage : MonoBehaviour
{
	[SerializeField]
	private UITexture _emblemTexure;

	[SerializeField]
	private GameObject _noEmblem;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UIWidget _expGaugeWidget;

	[SerializeField]
	private UISprite _expGaugeUpper;

	[SerializeField]
	private UILabel _expGaugeLable;

	[SerializeField]
	private GameObject _expHelpButton;

	[SerializeField]
	private KScrollView _levelRewards;

	private Clan _clan;

	private List<KeyValuePair<int, ClanLevelReward>> _levelRewardInfos;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIEventListener uIEventListener = UIEventListener.Get(_expHelpButton);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate(GameObject go)
			{
				//IL_0056: Unknown result type (might be due to invalid IL or missing references)
				Transform val = go.transform.FindChild("Sprite");
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(null, T._("캐릭터가 경험치를 획득하면 부족도 함께 경험치를 획득합니다.\n획득한 경험치는 부족 경험치에 천천히 반영됩니다."));
				widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
				widgetTooltipControl.Sign = 1;
				widgetTooltipControl.Show((!((Object)(object)val == (Object)null)) ? ((Component)val).gameObject : null, Vector2.zero, 10f);
			});
			InitLevelRewards();
		}
	}

	private void OnEnable()
	{
		Clan playerClan = GameSystem<ClanSystem>.Instance().PlayerClan;
		if (playerClan == null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		SetEmblem(null);
		Set(playerClan);
	}

	private void InitLevelRewards()
	{
		_levelRewardInfos = new List<KeyValuePair<int, ClanLevelReward>>();
		Dictionary<int, ClanLevelReward> level_rewards = Singleton<ClanYaml>.Instance.level_rewards;
		foreach (KeyValuePair<int, ClanLevelReward> item in level_rewards)
		{
			if (!string.IsNullOrEmpty(item.Value.description))
			{
				_levelRewardInfos.Add(item);
			}
		}
		_levelRewardInfos.Sort((KeyValuePair<int, ClanLevelReward> r1, KeyValuePair<int, ClanLevelReward> r2) => r1.Key - r2.Key);
		ListObjectPool nodes = _levelRewards.Nodes;
		nodes.Set(_levelRewardInfos.Count);
		for (int i = 0; i < nodes.Count; i++)
		{
			KeyValuePair<int, ClanLevelReward> keyValuePair = _levelRewardInfos[i];
			ClanLevelInfoNode component = nodes[i].GetComponent<ClanLevelInfoNode>();
			component.Set(keyValuePair.Key, keyValuePair.Value.description);
		}
		_levelRewards.ResetPosition();
	}

	public void Set([NotNull] Clan clan)
	{
		Init();
		_clan = clan;
		_clan.GetEmblem(SetEmblem);
		SetLevel(_clan.Level);
		SetExp(_clan.Level, _clan.Exp);
	}

	private void SetLevel(int level)
	{
		_levelLabel.text = T.Format("{0:lv:}", _clan.Level);
		ListObjectPool nodes = _levelRewards.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			KeyValuePair<int, ClanLevelReward> keyValuePair = _levelRewardInfos[i];
			ClanLevelInfoNode component = nodes[i].GetComponent<ClanLevelInfoNode>();
			component.SetActiveEffect(keyValuePair.Key <= level);
		}
		_levelRewards.Reposition();
	}

	private void SetEmblem(Texture2D texture)
	{
		if ((Object)(object)texture == (Object)null)
		{
			_noEmblem.gameObject.SetActive(true);
			((Component)_emblemTexure).gameObject.SetActive(false);
		}
		else
		{
			_noEmblem.gameObject.SetActive(false);
			((Component)_emblemTexure).gameObject.SetActive(true);
			_emblemTexure.mainTexture = (Texture)(object)texture;
		}
	}

	private void SetExp(int level, long exp)
	{
		GameSystem<ClanSystem>.Instance().GetExpRange(level, out var min, out var max);
		long num = max - min;
		long num2 = exp - min;
		float num3 = Mathf.Clamp01((float)num2 / (float)num);
		_expGaugeLable.text = $"<em>{num2:N0}</em> [aaaaaa]/[-] {num:N0}";
		_expGaugeUpper.width = (int)((float)_expGaugeWidget.width * num3);
		_expGaugeUpper.alpha = ((!(num3 > 0f)) ? 0f : 1f);
	}
}
