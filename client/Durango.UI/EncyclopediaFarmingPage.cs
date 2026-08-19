using System;
using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Encyclopedia;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EncyclopediaFarmingPage : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _progressLabel;

	[SerializeField]
	private NodesScrollView _scrollView;

	private readonly Dictionary<string, int> _prevFarmingLevels;

	void IUIInitializable.Init()
	{
		EncyclopediaCategory encyclopediaCategory = SingletonDict<EncyclopediaType, EncyclopediaCategory>.Get(EncyclopediaType.Farming);
		_titleLabel.text = ((encyclopediaCategory != null) ? encyclopediaCategory.Name.ToString() : EncyclopediaType.Farming.ToString());
		GameSystem<FarmingEncyclopediaSystem>.Instance().FarmingDataUpdated += OnUpdated;
		_scrollView.Nodes.Init(delegate(GameObject obj)
		{
			EncyclopediaCropWidget component = obj.GetComponent<EncyclopediaCropWidget>();
			component.Clicked += OnClickCropItem;
		});
	}

	private void OnUpdated(string key, FarmingEncyclopediaData? prev, FarmingEncyclopediaData data)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		foreach (GameObject node in _scrollView.Nodes)
		{
			EncyclopediaCropWidget component = node.GetComponent<EncyclopediaCropWidget>();
			if (component.Key == key)
			{
				component.Set(key, data);
				break;
			}
		}
	}

	public void Show()
	{
		int num = 0;
		Dictionary<string, EncyclopediaItem> dictionary = SingletonDict<EncyclopediaType, Dictionary<string, EncyclopediaItem>>.Get(EncyclopediaType.Farming);
		if (dictionary != null)
		{
			foreach (KeyValuePair<string, EncyclopediaItem> item in dictionary)
			{
				num += item.Value.MaxLevel;
			}
		}
		IEnumerable<KeyValuePair<string, FarmingEncyclopediaData>> farmingEncyclopediaDataList = GameSystem<FarmingEncyclopediaSystem>.Instance().GetFarmingEncyclopediaDataList();
		int num2 = 0;
		_scrollView.Nodes.BeginLoad();
		if (farmingEncyclopediaDataList != null)
		{
			KeyValuePair<string, FarmingEncyclopediaData>[] array = farmingEncyclopediaDataList.ToArray();
			Array.Sort(array, delegate(KeyValuePair<string, FarmingEncyclopediaData> d1, KeyValuePair<string, FarmingEncyclopediaData> d2)
			{
				int itemPriority = GetItemPriority(d1);
				int itemPriority2 = GetItemPriority(d2);
				int num3 = itemPriority - itemPriority2;
				if (num3 != 0)
				{
					return num3;
				}
				num3 = d2.Value.CurrentLevel - d1.Value.CurrentLevel;
				if (num3 != 0)
				{
					return num3;
				}
				num3 = d2.Value.CurrentExp - d1.Value.CurrentExp;
				return (num3 != 0) ? num3 : string.CompareOrdinal(d1.Key, d2.Key);
			});
			KeyValuePair<string, FarmingEncyclopediaData>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<string, FarmingEncyclopediaData> keyValuePair = array2[i];
				EncyclopediaCropWidget component = _scrollView.Nodes.GetNext().GetComponent<EncyclopediaCropWidget>();
				component.Set(keyValuePair.Key, keyValuePair.Value);
				num2 += keyValuePair.Value.CurrentLevel;
			}
		}
		_scrollView.Nodes.EndLoad();
		_scrollView.ResetPosition();
		_progressLabel.text = string.Format("{0} <em>{1:p0}</em>", T._("도감 완성도"), (num <= 0) ? 0f : ((float)num2 / (float)num));
	}

	private static int GetItemPriority(KeyValuePair<string, FarmingEncyclopediaData> item)
	{
		EncyclopediaItem encyclopediaItem = EncyclopediaItems.Get(EncyclopediaType.Farming, item.Key);
		if (encyclopediaItem == null)
		{
			return -1;
		}
		FarmingEncyclopediaData value = item.Value;
		if (value.CurrentLevel >= encyclopediaItem.MaxLevel)
		{
			bool flag = false;
			if (value.MasteryLevelToIndex != null)
			{
				KeyValuePair<int, KeyValuePair<string, float>[][]>[] masteryModifiersList = encyclopediaItem.GetMasteryModifiersList();
				for (int i = 0; i < masteryModifiersList.Length; i++)
				{
					KeyValuePair<int, KeyValuePair<string, float>[][]> keyValuePair = masteryModifiersList[i];
					if (keyValuePair.Key <= value.CurrentLevel && value.MasteryLevelToIndex.ContainsKey(keyValuePair.Key))
					{
						flag = true;
						break;
					}
				}
			}
			return flag ? 2 : 0;
		}
		return 1;
	}

	private void OnClickCropItem(string key)
	{
		FarmingEncyclopediaPopup farmingEncyclopediaPopup = UIManager.Popup.Tooltip<FarmingEncyclopediaPopup>();
		farmingEncyclopediaPopup.Set(key);
		farmingEncyclopediaPopup.Show();
	}
}
