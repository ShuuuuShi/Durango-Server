using System.Collections.Generic;
using Durango.Logic.Clusters;
using Durango.Logic.Encyclopedia;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using NestedPrefab;
using Shared.Encyclopedia;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Encyclopedia")]
public class EncyclopediaGroup : UIBase
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	private EncyclopediaFarmingPage _farmingPage;

	[SerializeField]
	private EncyclopediaMemoPage _memoPage;

	private GameObject[] _pages;

	private IconTabList _tabList;

	private readonly List<KeyValuePair<EncyclopediaType, EncyclopediaCategory>> _categories = new List<KeyValuePair<EncyclopediaType, EncyclopediaCategory>>();

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Encyclopedia;
		_titleWidget.Object.SetTitle(T._("도감"));
		_pages = new GameObject[2] { _farmingPage.gameObject, _memoPage.gameObject };
		InitializeTab();
		GameSystem<MemoSystem>.Instance().MemoCollected += OnMemoCollect;
		base.OnOpenSucceed += Opened;
		SetChildrenActive(activated: false);
	}

	private void InitializeTab()
	{
		_tabList = _tabLinker.Object.GetComponent<IconTabList>();
		_categories.Clear();
		Role role = GameManager.Region.Role();
		if (role != Role.Tutorial && role != Role.Safehouse && GameManager.ClusterMode == Mode.Online)
		{
			EncyclopediaType[] array = Enums<EncyclopediaType>.Greater(EncyclopediaType.Invalid);
			EncyclopediaType[] array2 = array;
			foreach (EncyclopediaType key in array2)
			{
				EncyclopediaCategory encyclopediaCategory = SingletonDict<EncyclopediaType, EncyclopediaCategory>.Get(key);
				if (encyclopediaCategory != null)
				{
					_categories.Add(new KeyValuePair<EncyclopediaType, EncyclopediaCategory>(key, encyclopediaCategory));
				}
			}
			_categories.Sort((KeyValuePair<EncyclopediaType, EncyclopediaCategory> i1, KeyValuePair<EncyclopediaType, EncyclopediaCategory> i2) => i1.Value.Order - i2.Value.Order);
		}
		_tabList.BeginLoad();
		foreach (KeyValuePair<EncyclopediaType, EncyclopediaCategory> category in _categories)
		{
			_tabList.Add(category.Value.Icon, category.Value.Name.ToString());
		}
		_tabList.Add("icon_encyclopedia_submemo", T._("메모"));
		_tabList.EndLoad();
		_tabList.Clicked += OnClickTab;
	}

	private void OnClickTab(int index)
	{
		if (index < _categories.Count)
		{
			ShowEncyclopediaPage(_categories[index].Key);
		}
		else
		{
			ShowMemoPage();
		}
	}

	private int EncyclopediaTypeIndexOf(EncyclopediaType type)
	{
		for (int i = 0; i < _categories.Count; i++)
		{
			if (_categories[i].Key == type)
			{
				return i;
			}
		}
		return -1;
	}

	private void ShowEncyclopediaPage(EncyclopediaType type)
	{
		int index = EncyclopediaTypeIndexOf(type);
		_tabList.Select(index);
		GameObject page = null;
		if (type == EncyclopediaType.Farming)
		{
			page = _farmingPage.gameObject;
			_farmingPage.Show();
		}
		ShowPage(page);
	}

	private void ShowMemoPage(MemoType type = MemoType.Fiction, int? memoId = null)
	{
		_tabList.Select(_categories.Count);
		if (!memoId.HasValue)
		{
			_memoPage.ShowMemoList(type);
		}
		else
		{
			_memoPage.ShowMemo(type, memoId.Value);
		}
		ShowPage(_memoPage.gameObject);
	}

	private void ShowPage(GameObject page)
	{
		GameObject[] pages = _pages;
		foreach (GameObject gameObject in pages)
		{
			gameObject.gameObject.SetActive(page == gameObject);
		}
	}

	[ExposedInEditor(null)]
	public void Open(MemoType type, int? memoId = null)
	{
		Open();
		ShowMemoPage(type, memoId);
	}

	private void Opened()
	{
		if (_categories.Count > 0)
		{
			ShowEncyclopediaPage(_categories[0].Key);
		}
		else
		{
			ShowMemoPage();
		}
	}

	protected override bool TryClose()
	{
		if (_memoPage.gameObject.activeSelf && !_memoPage.Close())
		{
			return false;
		}
		return base.TryClose();
	}

	private void OnMemoCollect(MemoType type, int index)
	{
		switch (type)
		{
		default:
			if (type != MemoType.Collect && type != MemoType.Faction)
			{
				return;
			}
			break;
		case MemoType.Fiction:
		case MemoType.Tooltip:
			return;
		case MemoType.Survival:
			break;
		}
		string memoTitle = MemoSystem.GetMemoTitle(type, index);
		string memoText = MemoSystem.GetMemoText(type, index);
		if (!string.IsNullOrEmpty(memoText))
		{
			string arg = ((20 >= memoText.Length) ? memoText : (memoText.Substring(0, 20) + "..."));
			UIManager.Alarm.ShowNotify($"{memoTitle}: {arg}", "alarm_memo", major: true, 1.8f, delegate
			{
				Open(type, index);
			});
		}
	}
}
