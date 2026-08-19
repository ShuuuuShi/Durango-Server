using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using Messages;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class CreateItemWidget : MonoBehaviour
{
	[SerializeField]
	private UIInput _searchInput;

	[SerializeField]
	private GameObject _clearButton;

	[SerializeField]
	private KInfiniteScrollView _items;

	[SerializeField]
	private IntSelector _intSelector;

	private KInfiniteScrollView.View<KeyValuePair<int, string>, Transform> _itemsView;

	private string[] _categoryList;

	private readonly List<KeyValuePair<string, Prototype>> _prototypes = new List<KeyValuePair<string, Prototype>>();

	private readonly List<KeyValuePair<int, string>> _filteredList = new List<KeyValuePair<int, string>>();

	private string _filterName;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_intSelector.Set(60, 1, 60);
		if (SingletonDict<string, List<Prototype>>.Instance == null)
		{
			_isInit = false;
			return;
		}
		Dictionary<string, List<string>> dictionary = new Dictionary<string, List<string>>();
		foreach (KeyValuePair<string, List<Prototype>> item in SingletonDict<string, List<Prototype>>.Instance)
		{
			List<Prototype> value = item.Value;
			if (value == null || value.Count == 0)
			{
				continue;
			}
			Prototype prototype = value[0];
			if (!string.IsNullOrEmpty(prototype.Category))
			{
				List<string> list = dictionary.Get(prototype.Category);
				if (list == null)
				{
					list = new List<string>();
					dictionary[prototype.Category] = list;
				}
				list.Add(item.Key);
			}
		}
		_categoryList = new string[dictionary.Count];
		int num = 0;
		foreach (KeyValuePair<string, List<string>> item2 in dictionary)
		{
			_categoryList[num] = item2.Key;
			num++;
		}
		foreach (KeyValuePair<string, List<Prototype>> item3 in SingletonDict<string, List<Prototype>>.Instance)
		{
			if (KUtility.GetSize(item3.Value) != 0)
			{
				_prototypes.Add(new KeyValuePair<string, Prototype>(item3.Key, item3.Value[0]));
			}
		}
		_prototypes.Sort((KeyValuePair<string, Prototype> n1, KeyValuePair<string, Prototype> n2) => string.Compare(n1.Value.Name, n2.Value.Name));
		KInfiniteScrollView.View<KeyValuePair<int, string>, Transform> view = _items.Initialize(delegate(Transform obj, KeyValuePair<int, string> val)
		{
			obj.Find("Text").GetComponent<UILabel>().text = val.Value;
		}, delegate(Transform o)
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(o.gameObject);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickItemNode));
		});
		view.SetList(_filteredList);
		_itemsView = view;
		EventDelegate.Set(_searchInput.onSubmit, delegate
		{
			FilterItem(_searchInput.value);
		});
		UIEventListener uIEventListener = UIEventListener.Get(_clearButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			FilterItem(null);
		});
	}

	private void OnClickItemNode(GameObject obj)
	{
		int num = _itemsView.IndexOf(obj.transform);
		if (num != -1)
		{
			num = _filteredList[num].Key;
			Connections.Frontend.Send(new Cheat
			{
				_Cheat = $"it {_prototypes[num].Key} {_intSelector.Value}"
			}).On(delegate(Info msg, PacketHeader _)
			{
				UIManager.SystemMsg(msg.Text);
			});
		}
	}

	private void FilterItem(string text)
	{
		_filterName = text;
		_searchInput.value = text;
		RefreshItems();
	}

	private void RefreshItems()
	{
		if (!string.IsNullOrEmpty(_filterName))
		{
			_filteredList.Clear();
			for (int i = 0; i < _prototypes.Count; i++)
			{
				KeyValuePair<string, Prototype> keyValuePair = _prototypes[i];
				Prototype value = keyValuePair.Value;
				string text = value.Name;
				if (text.Contains(_filterName) || keyValuePair.Key.Contains(_filterName))
				{
					string value2 = $"{text} [Lv.{value.MinLevel}~{value.MaxLevel}]";
					_filteredList.Add(new KeyValuePair<int, string>(i, value2));
				}
			}
		}
		else
		{
			_filteredList.Clear();
		}
		_itemsView.Refresh();
		_items.ResetPosition();
	}

	private void OnEnable()
	{
		Init();
		FilterItem(null);
	}
}
