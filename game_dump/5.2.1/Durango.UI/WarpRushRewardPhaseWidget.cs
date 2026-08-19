using System;
using Durango.Logic;
using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using NestedPrefab;
using Shared.Season2;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class WarpRushRewardPhaseWidget : MonoBehaviour
{
	[EnumList(typeof(WarpRushSystem.RewardType), false, 0, -1)]
	[SerializeField]
	private NestedPrefabLinker[] _reward;

	[SerializeField]
	private LevelGaugeController _levelGaugeController;

	[SerializeField]
	private UILabel _levelLabel;

	private ResourceType _resourceType = ResourceType.Invalid;

	private int _level = -1;

	private Action[] _requestReward;

	public void Init(UIScrollView scrollView)
	{
		_requestReward = new Action[2] { RequestLevelReward, RequestCashReward };
		NestedPrefabLinker[] reward = _reward;
		for (int i = 0; i < reward.Length; i++)
		{
			reward[i].Object.GetComponent<WarpRushRewardItem>().SetScrollView(scrollView);
		}
	}

	public void Refresh()
	{
		WarpRushSystem.RewardType[] array = Enums<WarpRushSystem.RewardType>.All();
		for (int i = 0; i < array.Length; i++)
		{
			RefreshRewardWidget(array[i]);
		}
		SetLevelGauge();
	}

	private void SetLevelGauge()
	{
		int level = GameSystem<WarpRushSystem>.Instance().GetRewardStatus(_resourceType).Level;
		if (_level < level)
		{
			_levelGaugeController.SetAccepted();
		}
		else if (_level == level)
		{
			_levelGaugeController.SetFirstAcceptable();
		}
		else
		{
			_levelGaugeController.SetNonAcceptable();
		}
	}

	private void RefreshRewardWidget(WarpRushSystem.RewardType rewardType)
	{
		WarpRushRewardItem component = _reward[(int)rewardType].Object.GetComponent<WarpRushRewardItem>();
		WarpRushSystem.RewardState rewardState = GameSystem<WarpRushSystem>.Instance().GetRewardState(rewardType, _resourceType, _level);
		if (rewardState != WarpRushSystem.RewardState.Invalid)
		{
			bool flag = rewardType == WarpRushSystem.RewardType.Cash && GameSystem<WarpRushSystem>.Instance().GetCashRewardPurchase() == null && GameSystem<WarpRushSystem>.Instance().IsCashRewardPurchasable();
			component.Clicked = ((rewardState != WarpRushSystem.RewardState.Available || flag) ? new Action(component.ShowTooltip) : _requestReward[(int)rewardType]);
			component.SetState(rewardState, flag);
		}
	}

	public void Set(ResourceType resourceType, int level, WarpRushReward levelReward, WarpRushReward cashReward)
	{
		_resourceType = resourceType;
		_level = level;
		_levelLabel.text = LocalizeUtil.FormatLevel(_level);
		SetLevelGauge();
		SetReward(WarpRushSystem.RewardType.Level, levelReward);
		SetReward(WarpRushSystem.RewardType.Cash, cashReward);
	}

	private void SetReward(WarpRushSystem.RewardType rewardType, WarpRushReward reward)
	{
		WarpRushRewardItem component = _reward[(int)rewardType].Object.GetComponent<WarpRushRewardItem>();
		if (reward == null)
		{
			component.gameObject.SetActive(value: false);
			return;
		}
		component.Set(reward);
		component.gameObject.SetActive(value: true);
		RefreshRewardWidget(rewardType);
	}

	private void RequestLevelReward()
	{
	}

	private void RequestCashReward()
	{
		WarpRushReward cashReward = Yaml.Util.Singleton<Yaml.WarpRushRewards>.Instance.GetCashReward(_resourceType, _level);
		if (cashReward == null)
		{
			return;
		}
		Purchase cashRewardPurchase = GameSystem<WarpRushSystem>.Instance().GetCashRewardPurchase();
		if (cashRewardPurchase == null)
		{
			return;
		}
		GameSystem<ShopSystem>.Instance().AcceptSubPurchase(cashRewardPurchase.Id, cashReward.CommodityId, delegate(bool success)
		{
			if (success)
			{
				ReceiveRewardsPopup receiveRewardsPopup = UIManager.Popup.FindTooltip<ReceiveRewardsPopup>();
				string deliveryMessage = WarpRushSystem.GetDeliveryMessage(isLevelUpReward: true, _resourceType);
				receiveRewardsPopup.ShowWarpRushRewardItemReceived(deliveryMessage, cashReward);
			}
		});
	}
}
