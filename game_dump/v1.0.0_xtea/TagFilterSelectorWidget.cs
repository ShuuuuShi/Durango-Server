using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class TagFilterSelectorWidget : MonoBehaviour
{
	public enum ItemType
	{
		None,
		Tag,
		Prototype
	}

	public struct ItemStruct
	{
		public string Id;

		public string Name;

		public string Icon;
	}

	[SerializeField]
	private KScrollView _items;

	[SerializeField]
	private UIInput _filterInput;

	[SerializeField]
	private Selectable _filterClear;

	[SerializeField]
	private Selectable _confirmBtn;

	[SerializeField]
	private Selectable _resetBtn;

	private Action _selected;

	private IList<string> _result;

	private ItemType _type;

	private bool _isOpen;

	public event Action Opened;

	public event Action Closed;

	private void Start()
	{
		Selectable confirmBtn = _confirmBtn;
		confirmBtn.Clicked = (Action)Delegate.Combine(confirmBtn.Clicked, new Action(Close));
		Selectable resetBtn = _resetBtn;
		resetBtn.Clicked = (Action)Delegate.Combine(resetBtn.Clicked, new Action(Reset));
		EventDelegate.Add(_filterInput.onSubmit, delegate
		{
			_filterInput.isSelected = false;
		});
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_filterInput).gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject go, bool isSelect)
		{
			if (!isSelect)
			{
				Filtering(_filterInput.value);
			}
		});
		EventDelegate.Add(_filterInput.onChange, delegate
		{
			((Component)_filterClear).gameObject.SetActive(!string.IsNullOrEmpty(_filterInput.value));
		});
		Selectable filterClear = _filterClear;
		filterClear.Clicked = (Action)Delegate.Combine(filterClear.Clicked, (Action)delegate
		{
			_filterInput.value = string.Empty;
			Filtering(string.Empty);
		});
		((Component)_filterClear).gameObject.SetActive(false);
	}

	private void OnEnable()
	{
		if (!_isOpen)
		{
			((Component)this).gameObject.SetActive(false);
		}
		else
		{
			((Component)this).GetComponent<UIRect>().alpha = 1f;
		}
	}

	private void OnDisable()
	{
		((Component)this).GetComponent<UIRect>().alpha = 0f;
	}

	private void SetType(ItemType type)
	{
		if (_type == type)
		{
			return;
		}
		_type = type;
		IDictionary dictionary = null;
		switch (type)
		{
		case ItemType.Tag:
			dictionary = SingletonDict<string, Tag>.Instance;
			break;
		case ItemType.Prototype:
			dictionary = SingletonDict<string, List<Prototype>>.Instance;
			break;
		}
		List<KeyValuePair<string, List<ItemStruct>>> list = new List<KeyValuePair<string, List<ItemStruct>>>();
		foreach (object item2 in dictionary)
		{
			string text = string.Empty;
			ItemStruct item = default(ItemStruct);
			DictionaryEntry dictionaryEntry = (DictionaryEntry)item2;
			switch (type)
			{
			case ItemType.Tag:
				if (!(dictionaryEntry.Value is Tag { visible: not false } tag))
				{
					continue;
				}
				item.Id = dictionaryEntry.Key as string;
				item.Name = tag.name;
				item.Icon = tag.icon;
				text = tag.category;
				break;
			case ItemType.Prototype:
			{
				if (!(dictionaryEntry.Value is List<Prototype> { Count: not 0 } list2))
				{
					continue;
				}
				Prototype prototype = list2[0];
				item.Id = dictionaryEntry.Key as string;
				item.Name = prototype.name;
				item.Icon = prototype.icon;
				string key = $"#prototype_category_small_{prototype.category}";
				text = LocalizeSystem.Get(key);
				break;
			}
			}
			if (string.IsNullOrEmpty(item.Id) || string.IsNullOrEmpty(item.Name))
			{
				continue;
			}
			int num = -1;
			for (int i = 0; i < list.Count; i++)
			{
				if (list[i].Key == text)
				{
					num = i;
					break;
				}
			}
			if (num == -1)
			{
				num = list.Count;
				list.Add(new KeyValuePair<string, List<ItemStruct>>(text, new List<ItemStruct>()));
			}
			list[num].Value.Add(item);
		}
		list.Sort(ItemListComparison);
		foreach (KeyValuePair<string, List<ItemStruct>> item3 in list)
		{
			item3.Value.Sort(ItemComparison);
		}
		ListObjectPool nodes = _items.Nodes;
		nodes.Init(InitItemObject);
		nodes.Set(list.Count);
		for (int j = 0; j < list.Count; j++)
		{
			TagFilterCategoryWidget component = nodes[j].GetComponent<TagFilterCategoryWidget>();
			component.Set(list[j], type);
		}
		_items.Reposition(resetPosition: true, tween: false);
	}

	private void InitItemObject(GameObject obj)
	{
		TagFilterCategoryWidget component = obj.GetComponent<TagFilterCategoryWidget>();
		component.HeightChanged += OnCategoryHeightChange;
	}

	private void OnCategoryHeightChange()
	{
		_items.Reposition();
	}

	private static int ItemListComparison(KeyValuePair<string, List<ItemStruct>> t1, KeyValuePair<string, List<ItemStruct>> t2)
	{
		return string.CompareOrdinal(t1.Key, t2.Key);
	}

	private static int ItemComparison(ItemStruct t1, ItemStruct t2)
	{
		return string.CompareOrdinal(t1.Name, t2.Name);
	}

	public void Open(Action selected, IList<string> result, ItemType type)
	{
		_isOpen = true;
		_selected = selected;
		_result = result;
		SetType(type);
		Reset(result);
		((Component)this).gameObject.SetActive(true);
		((Component)_items.ScrollView).GetComponent<UIPanel>().SetDirty();
		if (this.Opened != null)
		{
			this.Opened();
		}
	}

	public void Close()
	{
		if (_selected == null)
		{
			return;
		}
		_result.Clear();
		ListObjectPool nodes = _items.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			TagFilterCategoryWidget component = nodes[i].GetComponent<TagFilterCategoryWidget>();
			ListObjectPool nodes2 = component.Nodes;
			for (int j = 0; j < nodes2.Count; j++)
			{
				TagSearchCard component2 = nodes2[j].GetComponent<TagSearchCard>();
				if (component2.Select)
				{
					_result.Add(component2.Id);
				}
			}
		}
		_selected();
		_selected = null;
		_result = null;
		_isOpen = false;
		((Component)this).gameObject.SetActive(false);
		if (this.Closed != null)
		{
			this.Closed();
		}
	}

	public void Reset()
	{
		_filterInput.value = string.Empty;
		Reset(_result);
	}

	private void Reset(ICollection<string> selectedList)
	{
		ListObjectPool nodes = _items.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			TagFilterCategoryWidget component = nodes[i].GetComponent<TagFilterCategoryWidget>();
			ListObjectPool nodes2 = component.Nodes;
			for (int j = 0; j < nodes2.Count; j++)
			{
				TagSearchCard component2 = nodes2[j].GetComponent<TagSearchCard>();
				bool select = selectedList?.Contains(component2.Id) ?? false;
				component2.Select = select;
			}
			component.Reset();
		}
	}

	private void Filtering(string key)
	{
		ListObjectPool nodes = _items.Nodes;
		for (int i = 0; i < nodes.Count; i++)
		{
			TagFilterCategoryWidget component = nodes[i].GetComponent<TagFilterCategoryWidget>();
			component.FilterByName(key);
		}
		_items.ResetPosition();
	}
}
