using System;
using System.Collections.Generic;
using L10N;
using MarketData;
using UnityEngine;
using Yaml;
using Yaml.Util;

public class SearchFilterInfoWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _filters;

	[SerializeField]
	private UIWidget _clearBtn;

	private UIWidget _widget;

	private Commodities _commodities;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_clearBtn).gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_commodities.Filter.Reset();
			_commodities.Get(reset: true);
			Set(_commodities);
		});
		((Component)this).gameObject.SetActive(false);
	}

	public void Set(Commodities commodities)
	{
		_commodities = commodities;
		FilterOption filterOption = ((_commodities != null) ? _commodities.Filter : null);
		if (filterOption == null)
		{
			((Component)this).gameObject.SetActive(false);
			return;
		}
		bool flag = false;
		ListObjectPool nodes = _filters.Nodes;
		nodes.Init(InitFiletrCard);
		int i = 0;
		for (int size = KUtility.GetSize(filterOption.Prototype); i < size; i++)
		{
			List<Prototype> list = SingletonDict<string, List<Prototype>>.Get(filterOption.Prototype[i].Key);
			using List<Prototype>.Enumerator enumerator = list.GetEnumerator();
			if (enumerator.MoveNext())
			{
				Prototype current = enumerator.Current;
				if (!flag)
				{
					nodes.Clear();
					flag = true;
				}
				TagFilterCard tagFilterCard = ((ListObjectPoolBase<GameObject>)nodes).Add<TagFilterCard>();
				tagFilterCard.Set(current.name);
			}
		}
		if (filterOption.Level.Min > 1 || filterOption.Level.Max > 0)
		{
			if (!flag)
			{
				nodes.Clear();
				flag = true;
			}
			TagFilterCard tagFilterCard2 = ((ListObjectPoolBase<GameObject>)nodes).Add<TagFilterCard>();
			tagFilterCard2.Set(string.Format("{0}-{1}", T._("레벨 {0}", Mathf.Max(1, filterOption.Level.Min)), (filterOption.Level.Max <= 0) ? "∞" : filterOption.Level.Max.ToString()));
		}
		if (filterOption.Currency.Min > 1 || filterOption.Currency.Max > 0)
		{
			if (!flag)
			{
				nodes.Clear();
				flag = true;
			}
			TagFilterCard tagFilterCard3 = ((ListObjectPoolBase<GameObject>)nodes).Add<TagFilterCard>();
			tagFilterCard3.Set(string.Format("[tstone_icon] {0:N0}-{1}", Mathf.Max(1, filterOption.Currency.Min), (filterOption.Currency.Max <= 0) ? "∞" : filterOption.Currency.Max.ToString("N0")));
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(filterOption.Tags); j < size2; j++)
		{
			if (!flag)
			{
				nodes.Clear();
				flag = true;
			}
			TagFilterCard tagFilterCard4 = ((ListObjectPoolBase<GameObject>)nodes).Add<TagFilterCard>();
			Tag tag = SingletonDict<string, Tag>.Get(filterOption.Tags[j].Key);
			tagFilterCard4.Set(tag.name);
		}
		if (flag)
		{
			bool activeInHierarchy = ((Component)this).gameObject.activeInHierarchy;
			((Component)this).gameObject.SetActive(true);
			_filters.Reposition(!activeInHierarchy, activeInHierarchy);
		}
		else
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void InitFiletrCard(GameObject obj)
	{
		TagFilterCard component = obj.GetComponent<TagFilterCard>();
		component.Removed = (Action<GameObject>)Delegate.Combine(component.Removed, new Action<GameObject>(OnRemoveFilterCard));
	}

	private void OnRemoveFilterCard(GameObject obj)
	{
		int index = _filters.Nodes.IndexOf(obj);
		if (RemoveFilterOption(_commodities.Filter, index))
		{
			_commodities.Get(reset: true);
			Set(_commodities);
		}
	}

	private bool RemoveFilterOption(FilterOption option, int index)
	{
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(option.Prototype); i < size; i++)
		{
			if (num == index)
			{
				option.Prototype.RemoveAt(i);
				return true;
			}
			num++;
		}
		if (option.Level.Min > 1 || option.Level.Max > 0)
		{
			if (num == index)
			{
				option.Level.Min = 0;
				option.Level.Max = 0;
				return true;
			}
			num++;
		}
		if (option.Currency.Min > 1 || option.Currency.Max > 0)
		{
			if (num == index)
			{
				option.Currency.Min = 0;
				option.Currency.Max = 0;
				return true;
			}
			num++;
		}
		int j = 0;
		for (int size2 = KUtility.GetSize(option.Tags); j < size2; j++)
		{
			if (num == index)
			{
				option.Tags.RemoveAt(j);
				return true;
			}
			num++;
		}
		return false;
	}
}
