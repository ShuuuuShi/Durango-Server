using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class SearchInfoWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _scrollView;

	private bool _isInit;

	public event Action SearchClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
		}
	}

	private void OnEnable()
	{
		if (!_isInit)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void Set(SearchOption option)
	{
		Init();
		if (option == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		if (!string.IsNullOrEmpty(option.Prototype))
		{
			List<Prototype> list = SingletonDict<string, List<Prototype>>.Get(option.Prototype);
			if (KUtility.GetSize(list) > 0)
			{
				SearchInfoItemNode component = nodes.GetNext().GetComponent<SearchInfoItemNode>();
				component.Set(string.Format("{0}: {1}", T._("원형"), list[0].Name), delegate
				{
					option.Prototype = string.Empty;
					OnClickSearchItem();
				});
			}
		}
		if (!string.IsNullOrEmpty(option.SearchKeyword))
		{
			SearchInfoItemNode component2 = nodes.GetNext().GetComponent<SearchInfoItemNode>();
			component2.Set(string.Format("{0}: {1}", T._("검색"), option.SearchKeyword), delegate
			{
				option.SearchKeyword = string.Empty;
				OnClickSearchItem();
			});
		}
		PriceRangePredicate price = option.Price;
		if (price.Min.HasValue || price.Max.HasValue)
		{
			SearchInfoItemNode component3 = nodes.GetNext().GetComponent<SearchInfoItemNode>();
			component3.Set(T._("[icon={0}] {1}", Durango.Logic.Item.Inventory.GetIcon(Currency.TStone), string.Format("{0} - {1}", (!price.Min.HasValue) ? string.Empty : price.Min.Value.ToString("N0", T.Culture), (!price.Max.HasValue) ? string.Empty : price.Max.Value.ToString("N0", T.Culture)).Trim()), delegate
			{
				option.Price.Min = null;
				option.Price.Max = null;
				OnClickSearchItem();
			});
		}
		RangePredicate level = option.Level;
		if (level.Min.HasValue || level.Max.HasValue)
		{
			SearchInfoItemNode component4 = nodes.GetNext().GetComponent<SearchInfoItemNode>();
			component4.Set($"Lv: {((!level.Min.HasValue) ? string.Empty : level.Min.ToString())} - {((!level.Max.HasValue) ? string.Empty : level.Max.ToString())}".Trim(), delegate
			{
				option.Level.Min = null;
				option.Level.Max = null;
				OnClickSearchItem();
			});
		}
		foreach (TagFilterBase elem2 in option.Tags)
		{
			SearchInfoItemNode component5 = nodes.GetNext().GetComponent<SearchInfoItemNode>();
			component5.Set(elem2.GetName(), delegate
			{
				option.Tags.Remove(elem2);
				OnClickSearchItem();
			});
		}
		foreach (TagFilterBase elem in option.Materials)
		{
			SearchInfoItemNode component6 = nodes.GetNext().GetComponent<SearchInfoItemNode>();
			component6.Set(elem.GetName(), delegate
			{
				option.Materials.Remove(elem);
				OnClickSearchItem();
			});
		}
		nodes.EndLoad();
		if (nodes.Count > 0)
		{
			_scrollView.ResetPosition();
			base.gameObject.SetActive(value: true);
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnClickSearchItem()
	{
		if (this.SearchClicked != null)
		{
			this.SearchClicked();
		}
	}
}
