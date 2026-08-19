using System;
using System.Collections.Generic;
using MarketData;
using UnityEngine;

public class MarketSearchWidget : MonoBehaviour
{
	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private PrototypeSearchWidget _prototype;

	[SerializeField]
	private LevelRangeSearchWidget _levelRange;

	[SerializeField]
	private CurrencyRangeSearchWidget _currencyRange;

	[SerializeField]
	private TagFilterSearchWidget _tagFilter;

	[SerializeField]
	private TagFilterSelectorWidget _tagSelector;

	[SerializeField]
	private Selectable _resetButton;

	[SerializeField]
	private Selectable _searchButton;

	private Commodities _commodities;

	public bool IsOpen { get; private set; }

	public event Action Searched;

	private void Start()
	{
		Selectable resetButton = _resetButton;
		resetButton.Clicked = (Action)Delegate.Combine(resetButton.Clicked, new Action(Reset));
		Selectable searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, new Action(Search));
		_tagSelector.Opened += delegate
		{
			((Component)this).GetComponent<UIRect>().alpha = 0f;
		};
		_tagSelector.Closed += delegate
		{
			((Component)this).GetComponent<UIRect>().alpha = 1f;
		};
	}

	private void OnEnable()
	{
		if (!IsOpen)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void Reset()
	{
		_prototype.Set(null);
		_levelRange.Set(0, 0);
		_currencyRange.Set(0, 0);
		_tagFilter.Set(null);
	}

	public void Search()
	{
		FilterOption filter = _commodities.Filter;
		filter.Level.Min = _levelRange.Min;
		filter.Level.Max = _levelRange.Max;
		filter.Currency.Min = _currencyRange.Min;
		filter.Currency.Max = _currencyRange.Max;
		if (_prototype.Prototype.Count == 0)
		{
			if (filter.Prototype != null)
			{
				filter.Prototype.Clear();
			}
		}
		else
		{
			if (filter.Prototype == null)
			{
				filter.Prototype = new List<RangeOption>();
			}
			filter.Prototype.Clear();
			List<string> prototype = _prototype.Prototype;
			for (int i = 0; i < prototype.Count; i++)
			{
				filter.Prototype.Add(new RangeOption
				{
					Key = prototype[i]
				});
			}
		}
		if (_tagFilter.Tags.Count == 0)
		{
			if (filter.Tags != null)
			{
				filter.Tags.Clear();
			}
		}
		else
		{
			if (filter.Tags == null)
			{
				filter.Tags = new List<RangeOption>();
			}
			filter.Tags.Clear();
			List<string> tags = _tagFilter.Tags;
			for (int j = 0; j < tags.Count; j++)
			{
				filter.Tags.Add(new RangeOption
				{
					Key = tags[j]
				});
			}
		}
		_commodities.Request.Type = CommodityOwner.Region;
		_commodities.Request.Id = KSingleton<GameManager>.Instance().Region.Id;
		_commodities.Get(reset: true);
		if (this.Searched != null)
		{
			this.Searched();
		}
	}

	public void Open(Commodities commodities)
	{
		IsOpen = true;
		_commodities = commodities;
		FilterOption filter = _commodities.Filter;
		_levelRange.Set(filter.Level.Min, filter.Level.Max);
		_currencyRange.Set(filter.Currency.Min, filter.Currency.Max);
		_prototype.Set(filter.Prototype);
		_tagFilter.Set(filter.Tags);
		((Component)this).gameObject.SetActive(true);
		((Component)_scrollView.ScrollView).GetComponent<UIPanel>().SetDirty();
	}

	public bool Close(bool all)
	{
		if (((Component)_tagSelector).gameObject.activeSelf)
		{
			_tagSelector.Reset();
			_tagSelector.Close();
			if (!all)
			{
				return false;
			}
		}
		_commodities = null;
		IsOpen = false;
		((Component)this).gameObject.SetActive(false);
		return true;
	}
}
