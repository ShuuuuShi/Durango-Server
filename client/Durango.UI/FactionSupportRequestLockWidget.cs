using System.Collections.Generic;
using Durango.Logic.Item;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionSupportRequestLockWidget : UIWidget
{
	[SerializeField]
	private UILabel _mainLabel;

	[SerializeField]
	private UILabel _subLabel;

	[SerializeField]
	private FactionSupportRequestRewardWidget _rewardBase;

	[SerializeField]
	private UIWidget _rewardsContainer;

	private ListObjectPool<FactionSupportRequestRewardWidget> _rewardList;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_rewardList = new ListObjectPool<FactionSupportRequestRewardWidget>();
			_rewardList.BaseObject = _rewardBase;
			_rewardList.UseBase = true;
		}
	}

	public void Set(FactionType faction, int level, List<SupportRequest> requests)
	{
		Init();
		Yaml.Faction faction2 = SingletonDict<FactionType, Yaml.Faction>.Get(faction);
		string text = T._("위 목록의 물품을 지원 요청할 수 있습니다.");
		_subLabel.text = ((!string.IsNullOrEmpty(faction2.HelpText)) ? $"{text}\n{faction2.HelpText}" : text);
		int num = base.width - 50;
		_mainLabel.width = num;
		_subLabel.width = num;
		_mainLabel.text = T._("{0:와}의 우호도를 쌓으면 {1}단계 지원 요청이 가능합니다!", faction2.Name, level);
		_rewardList.BeginLoad();
		List<ItemData> list = new List<ItemData>();
		List<Currency> list2 = new List<Currency>();
		foreach (SupportRequest request in requests)
		{
			GatherRewards(list, list2, request.Rewards);
			GatherRewards(list, list2, request.RandomRewards);
		}
		for (int i = 0; i < list.Count + list2.Count; i++)
		{
			FactionSupportRequestRewardWidget next = _rewardList.GetNext();
			if (i < list.Count)
			{
				next.Set(list[i]);
			}
			else
			{
				next.Set(new Money(0, list2[i - list.Count]));
			}
		}
		_rewardList.EndLoad();
		_rewardsContainer.width = num;
		int num2 = Mathf.FloorToInt(((float)num + 10f) / ((float)_rewardBase.width + 10f));
		int num3 = Mathf.CeilToInt((float)_rewardList.Count / (float)num2);
		int num4 = 0;
		for (int j = 0; j < num3; j++)
		{
			int num5 = Mathf.Min(num2, _rewardList.Count - j * num2);
			Vector3 vector = default(Vector3);
			vector.x = (0f - ((float)_rewardBase.width + 10f)) * (float)(num5 - 1) * 0.5f;
			vector.y = ((float)_rewardBase.height + 10f) * (float)(num3 - 1) * 0.5f - (float)j * ((float)_rewardBase.height + 10f);
			for (int k = 0; k < num5; k++)
			{
				_rewardList[num4].transform.localPosition = vector + Vector3.right * (10f + (float)_rewardBase.width) * k;
				num4++;
			}
		}
		_rewardsContainer.height = (int)((float)(_rewardBase.height * num3) + 10f * (float)Mathf.Max(0, num3 - 1) + 40f);
		_mainLabel.SetPosition(_rewardsContainer.GetPosition(0.5f, 1f) + Vector3.up * 40f, 0.5f, 0f);
		_subLabel.SetPosition(_rewardsContainer.GetPosition(0.5f, 0f) + Vector3.down * 40f, 0.5f, 1f);
	}

	private static void GatherRewards(List<ItemData> items, List<Currency> currencys, Messages.SupportRewards rewards)
	{
		ItemSupportReward[] items2 = rewards.Items;
		for (int i = 0; i < items2.Length; i++)
		{
			ItemSupportReward itemSupportReward = items2[i];
			bool flag = false;
			foreach (ItemData item in items)
			{
				if (item.PrototypeId == itemSupportReward.Item.Prototype)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				items.Add(new ItemData(itemSupportReward.Item));
			}
		}
		Money[] moneys = rewards.Moneys;
		for (int j = 0; j < moneys.Length; j++)
		{
			Money money = moneys[j];
			bool flag2 = false;
			foreach (Currency currency in currencys)
			{
				if (money.Currency == currency)
				{
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				currencys.Add(money.Currency);
			}
		}
	}
}
