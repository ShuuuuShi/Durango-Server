using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.UI.Control;
using Durango.UI.Popup;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MarketSearchWidget : MonoBehaviour
{
	private struct PrototypeTagWrapper
	{
		public readonly TagFilterBase Tag;

		public readonly TagFilterBase Material;

		public readonly string Prototype;

		public PrototypeTagWrapper([CanBeNull] TagFilterBase tag, [CanBeNull] TagFilterBase material, [CanBeNull] string prototype)
		{
			Tag = tag;
			Material = material;
			Prototype = prototype;
		}
	}

	private const int MaxSearchHistory = 5;

	private const float SearchHistoryMargin = 8f;

	[SerializeField]
	private UILabel _searchStatueLabel;

	[SerializeField]
	private UIWidget _textSearchContainer;

	[SerializeField]
	private SelectableWidget _textSearchWidget;

	[SerializeField]
	private UIWidget _searchHistoryContainer;

	[SerializeField]
	private ListObjectPool _searchHistoryItems;

	[SerializeField]
	private UIInput _textInput;

	[SerializeField]
	private SelectableWidget _tagFieldWidget;

	[SerializeField]
	private VerticalLayoutWidget _tagHolder;

	[SerializeField]
	private SelectableWidget _minPriceWidget;

	[SerializeField]
	private UILabel _minPriceLabel;

	[SerializeField]
	private SelectableWidget _maxPriceWidget;

	[SerializeField]
	private UILabel _maxPriceLabel;

	[SerializeField]
	private GameObject _levelContainer;

	[SerializeField]
	private SelectableWidget _minLevelWidget;

	[SerializeField]
	private UILabel _minLevelLabel;

	[SerializeField]
	private SelectableWidget _maxLevelWidget;

	[SerializeField]
	private UILabel _maxLevelLabel;

	[SerializeField]
	private UIWidget _levelButtonContainer;

	[SerializeField]
	private SelectableButton _baseLevelButton;

	[SerializeField]
	private SelectableWidget _clearButton;

	[SerializeField]
	private SelectableButton _searchButton;

	[SerializeField]
	private RectLayoutComponent _searchListLayout;

	[SerializeField]
	private KWidgetScrollView _searchListScrollView;

	[SerializeField]
	private AnimationWidget _animWidget;

	private UIWidget _widget;

	private ListObjectPool<SelectableButton> _levelButtons;

	private readonly int[] _levelTerms = new int[4] { 1, 5, 10, 60 };

	private readonly LinkedList<string> _searchHistory = new LinkedList<string>();

	private Selectable[] _selectableWidgets;

	private SearchOption _searchOption;

	private bool _isInit;

	public bool IsOpen { get; private set; }

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public event Action<bool> Enabled;

	public event Action SearchClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_animWidget.SetAlpha(0f, useTween: false);
		InitLevelTermButtons();
		UIEventListener uIEventListener = UIEventListener.Get(_textInput.gameObject);
		uIEventListener.onSelect = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onSelect, (UIEventListener.BoolDelegate)delegate(GameObject obj, bool selected)
		{
			if (selected)
			{
				SetToggleAsSelected(_textSearchWidget);
			}
			else
			{
				SetToggleAsSelected(null);
				_searchOption.SearchKeyword = _textInput.value;
				Refresh();
			}
			_searchListScrollView.MoveTo(0f, instant: false);
		});
		EventDelegate.Set(_textInput.onSubmit, delegate
		{
			_textInput.isSelected = false;
		});
		_textInput.defaultText = T._("아이템 이름을 입력하세요");
		SelectableWidget minPriceWidget = _minPriceWidget;
		minPriceWidget.Clicked = (Action)Delegate.Combine(minPriceWidget.Clicked, new Action(OnClickPriceWidget));
		SelectableWidget maxPriceWidget = _maxPriceWidget;
		maxPriceWidget.Clicked = (Action)Delegate.Combine(maxPriceWidget.Clicked, new Action(OnClickPriceWidget));
		SelectableWidget minLevelWidget = _minLevelWidget;
		minLevelWidget.Clicked = (Action)Delegate.Combine(minLevelWidget.Clicked, new Action(OnClickLevelWidget));
		SelectableWidget maxLevelWidget = _maxLevelWidget;
		maxLevelWidget.Clicked = (Action)Delegate.Combine(maxLevelWidget.Clicked, new Action(OnClickLevelWidget));
		SelectableWidget tagFieldWidget = _tagFieldWidget;
		tagFieldWidget.Clicked = (Action)Delegate.Combine(tagFieldWidget.Clicked, new Action(OnClickTagWidget));
		SelectableWidget clearButton = _clearButton;
		clearButton.Clicked = (Action)Delegate.Combine(clearButton.Clicked, new Action(OnClickClearButton));
		SelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, (Action)delegate
		{
			if (!string.IsNullOrEmpty(_textInput.value.Trim()))
			{
				_searchHistory.Remove(_textInput.value);
				_searchHistory.AddLast(_textInput.value);
				if (_searchHistory.Count > 5)
				{
					_searchHistory.RemoveFirst();
				}
			}
			if (this.SearchClicked != null)
			{
				this.SearchClicked();
			}
			Close();
		});
		_selectableWidgets = new Selectable[6] { _textSearchWidget, _tagFieldWidget, _minPriceWidget, _maxPriceWidget, _minLevelWidget, _maxLevelWidget };
	}

	private void OnTagSelectFinished()
	{
		List<PrototypeTagWrapper> list = new List<PrototypeTagWrapper>();
		if (_searchOption != null)
		{
			if (_searchOption.Prototype != null)
			{
				list.Add(new PrototypeTagWrapper(null, null, _searchOption.Prototype));
			}
			foreach (TagFilterBase material in _searchOption.Materials)
			{
				list.Add(new PrototypeTagWrapper(null, material, null));
			}
			foreach (TagFilterBase tag in _searchOption.Tags)
			{
				list.Add(new PrototypeTagWrapper(tag, null, null));
			}
		}
		_tagHolder.SetGrids(list, delegate(PrototypeTagWrapper data, ItemTagWidget obj, int idx)
		{
			if (data.Tag != null)
			{
				obj.Set(data.Tag.GetName(), delegate
				{
					_searchOption.Tags.Remove(data.Tag);
					OnTagSelectFinished();
				});
			}
			else if (data.Material != null)
			{
				obj.Set(data.Material.GetName(), delegate
				{
					_searchOption.Materials.Remove(data.Material);
					OnTagSelectFinished();
				});
			}
			else
			{
				List<Prototype> list2 = SingletonDict<string, List<Prototype>>.Get(data.Prototype);
				if (KUtility.GetSize(list2) > 0)
				{
					obj.Set(list2[0].Name, delegate
					{
						_searchOption.Prototype = null;
						OnTagSelectFinished();
					});
				}
			}
		});
		_searchListLayout.UpdateLayout();
		_searchListScrollView.Reposition();
	}

	private void InitLevelTermButtons()
	{
		_levelButtons = new ListObjectPool<SelectableButton>();
		_levelButtons.BaseObject = _baseLevelButton;
		_levelButtons.UseBase = true;
		_levelButtons.Set(_levelTerms.Length + 1);
		for (int i = 0; i < _levelTerms.Length; i++)
		{
			SelectableButton selectableButton = _levelButtons[i + 1];
			selectableButton.Text = _levelTerms[i].ToString();
			selectableButton.Clicked = (Action)Delegate.Combine(selectableButton.Clicked, new Action(OnClickLevelTermButton));
		}
		SelectableButton selectableButton2 = _levelButtons[0];
		selectableButton2.Clicked = (Action)Delegate.Combine(selectableButton2.Clicked, new Action(OnClickLevelClearButton));
		selectableButton2.Icon = "market_icon_refresh_small";
		_levelButtonContainer.AddOnChange(UpdateLevelButton);
	}

	private void UpdateLevelButton()
	{
		int width = _levelButtonContainer.width;
		int count = _levelButtons.Count;
		int width2 = (width - 5 * (count - 1)) / count;
		for (int i = 0; i < _levelButtons.Count; i++)
		{
			_levelButtons[i].Widget.width = width2;
		}
		UIUtility.UpdateAnchors(_levelButtonContainer.transform);
		_levelButtons.BaseObject.Widget.SetPosition(Vector3.Lerp(_levelButtonContainer.localCorners[0], _levelButtonContainer.localCorners[1], 0.5f), 0f, 0.5f);
		_levelButtons.Reposition(Vector3.right, 5);
	}

	private void OnEnable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Combine(UICamera.onClick, new UICamera.VoidDelegate(OnClicked));
		if (this.Enabled != null)
		{
			this.Enabled(obj: true);
		}
		if (!IsOpen)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		UICamera.onClick = (UICamera.VoidDelegate)Delegate.Remove(UICamera.onClick, new UICamera.VoidDelegate(OnClicked));
		if (this.Enabled != null)
		{
			this.Enabled(obj: false);
		}
		if (_isInit)
		{
			IsOpen = false;
			_animWidget.SetAlpha(0f, useTween: false);
		}
	}

	public void Open([NotNull] SearchOption option)
	{
		Init();
		IsOpen = true;
		_searchOption = option;
		Transform transform = _textSearchContainer.transform.Find("Label/Label");
		if (transform != null)
		{
			transform.GetComponent<UILabel>().text = ((_searchOption.MainCategory == null) ? T._("속성") : T._("결과 내 검색"));
		}
		ShowSearchHistory();
		base.gameObject.SetActive(value: true);
		_animWidget.Alpha = 1f;
		_searchListLayout.UpdateLayout();
		_searchListScrollView.ResetPosition();
		Refresh();
		SetToggleAsSelected(null);
	}

	public void Close()
	{
		IsOpen = false;
		_animWidget.Alpha = 0f;
	}

	private void ShowSearchHistory()
	{
		_searchHistoryContainer.gameObject.SetActive(_searchHistory.Count > 0);
		_searchHistoryItems.BeginLoad();
		foreach (string item in _searchHistory)
		{
			_searchHistoryItems.GetNext().GetComponent<SearchInfoItemNode>().Set(item, OnClickSearchHistory);
		}
		_searchHistoryItems.EndLoad();
		UIUtility.WidgetsReposition(_searchHistoryItems, _searchHistoryContainer, Vector3.right, 8f);
	}

	private void OnClickSearchHistory(GameObject go)
	{
		int num = _searchHistoryItems.IndexOf(go);
		if (num >= 0 && _searchHistory.Count > num)
		{
			string searchKeyword = _searchHistory.ElementAt(num);
			_searchOption.SearchKeyword = searchKeyword;
			Refresh();
		}
	}

	private void SetToggleAsSelected(Selectable comp)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_selectableWidgets); i < size; i++)
		{
			_selectableWidgets[i].Selected = comp == _selectableWidgets[i];
		}
	}

	private void OnClickTagWidget()
	{
		List<string> list = new List<string>();
		foreach (TagFilterBase tag in _searchOption.Tags)
		{
			if (tag is SingularTagFilter singularTagFilter)
			{
				list.Add(singularTagFilter.Id);
			}
		}
		SetToggleAsSelected(Selectable.Current);
		TagSelectPopup tagSelectPopup = UIManager.Popup.Tooltip<TagSelectPopup>();
		tagSelectPopup.Show();
		tagSelectPopup.Set(list, ApplySelectedTag);
	}

	private void ApplySelectedTag(HashSet<string> result)
	{
		if (_searchOption != null)
		{
			_searchOption.Tags.RemoveWhere((TagFilterBase elem) => elem is SingularTagFilter);
			foreach (string item in result)
			{
				_searchOption.Tags.Add(new SingularTagFilter(item, 1));
			}
		}
		OnTagSelectFinished();
		SetToggleAsSelected(null);
	}

	private void OnClickPriceWidget()
	{
		Selectable current = Selectable.Current;
		SetToggleAsSelected(current);
		NumberInputPopup numberInputPopup = UIManager.Popup.Tooltip<NumberInputPopup>();
		bool num = current == _minPriceWidget;
		PriceRangePredicate price = _searchOption.Price;
		long? num2 = ((!num) ? price.Max : price.Min);
		numberInputPopup.Show((!num2.HasValue) ? 0 : num2.Value, Currency.TStone, T._("가격 설정"), OnPriceInputConfirmed);
		numberInputPopup.AddOnFinished(delegate
		{
			SetToggleAsSelected(null);
		});
	}

	private void OnPriceInputConfirmed(long value)
	{
		if (_minPriceWidget.Selected)
		{
			if (value > 0)
			{
				_searchOption.Price.Min = value;
				if (_searchOption.Price.Max.HasValue)
				{
					long? max = _searchOption.Price.Max;
					bool hasValue = max.HasValue;
					long? min = _searchOption.Price.Min;
					if ((hasValue & min.HasValue) && max.GetValueOrDefault() < min.GetValueOrDefault())
					{
						_searchOption.Price.Max = _searchOption.Price.Min;
					}
				}
			}
			else
			{
				_searchOption.Price.Min = null;
			}
		}
		else if (value > 0)
		{
			_searchOption.Price.Max = value;
			if (_searchOption.Price.Min.HasValue)
			{
				long? min2 = _searchOption.Price.Min;
				bool hasValue2 = min2.HasValue;
				long? max2 = _searchOption.Price.Max;
				if ((hasValue2 & max2.HasValue) && min2.GetValueOrDefault() > max2.GetValueOrDefault())
				{
					_searchOption.Price.Min = _searchOption.Price.Max;
				}
			}
		}
		else
		{
			_searchOption.Price.Max = null;
		}
		Refresh();
	}

	private void OnClickLevelWidget()
	{
		Selectable current = Selectable.Current;
		current.Disabled = false;
		SetToggleAsSelected(current);
		Refresh();
	}

	private void OnClickLevelTermButton()
	{
		int num = _levelButtons.IndexOf(Selectable.Current as SelectableButton);
		if (num == -1)
		{
			return;
		}
		int num2 = _levelTerms[num - 1];
		if (_maxLevelWidget.Selected)
		{
			int num3 = (_searchOption.Level.Max.HasValue ? _searchOption.Level.Max.Value : 0);
			num3 += num2;
			_searchOption.Level.Max = num3;
			if (_searchOption.Level.Min.HasValue)
			{
				int? min = _searchOption.Level.Min;
				if (min.HasValue && min.GetValueOrDefault() > num3)
				{
					_searchOption.Level.Min = num3;
				}
			}
		}
		else
		{
			if (!_minLevelWidget.Selected)
			{
				SetToggleAsSelected(_minLevelWidget);
			}
			int num4 = (_searchOption.Level.Min.HasValue ? _searchOption.Level.Min.Value : 0);
			num4 += num2;
			_searchOption.Level.Min = num4;
			if (_searchOption.Level.Max.HasValue)
			{
				int? max = _searchOption.Level.Max;
				if (max.HasValue && max.GetValueOrDefault() < num4)
				{
					_searchOption.Level.Max = num4;
				}
			}
		}
		Refresh();
	}

	private void OnClickLevelClearButton()
	{
		if (_maxLevelWidget.Selected)
		{
			_searchOption.Level.Max = null;
		}
		else if (_minLevelWidget.Selected)
		{
			_searchOption.Level.Min = null;
		}
		else
		{
			_searchOption.Level.Min = null;
			_searchOption.Level.Max = null;
		}
		Refresh();
	}

	private void OnClickClearButton()
	{
		HashSet<TagFilterBase> tags = new HashSet<TagFilterBase>(_searchOption.Tags);
		HashSet<TagFilterBase> materials = new HashSet<TagFilterBase>(_searchOption.Materials.ToList());
		_searchOption.Clear();
		_searchOption.Tags = tags;
		_searchOption.Materials = materials;
		Refresh();
	}

	private void Refresh()
	{
		if (_searchOption.MainCategory != null)
		{
			_searchStatueLabel.text = T._("{0}/{1}", _searchOption.MainCategory.Name, (_searchOption.SubCategory != null) ? _searchOption.SubCategory.Name : T._("전체"));
		}
		else
		{
			_searchStatueLabel.text = T._("전체 검색");
		}
		_textInput.value = _searchOption.SearchKeyword;
		PriceRangePredicate price = _searchOption.Price;
		_minPriceLabel.text = ((!price.Min.HasValue) ? "1" : Durango.Logic.Item.Inventory.CurrencyFormat(price.Min.Value, price.Currency));
		_maxPriceLabel.text = ((!price.Max.HasValue) ? "∞" : Durango.Logic.Item.Inventory.CurrencyFormat(price.Max.Value, price.Currency));
		RangePredicate level = _searchOption.Level;
		_minLevelLabel.text = T._("{0:lv:}", (!level.Min.HasValue) ? 1 : level.Min.Value);
		_maxLevelLabel.text = ((!level.Max.HasValue) ? T._("{0:lv:}", "∞") : T._("{0:lv:}", level.Max.Value));
		OnTagSelectFinished();
	}

	private void OnClicked(GameObject obj)
	{
		if ((_minLevelWidget.Selected || _maxLevelWidget.Selected) && (obj == null || !NGUITools.IsChild(_levelContainer.transform, obj.transform)))
		{
			_minLevelWidget.Selected = false;
			_maxLevelWidget.Selected = false;
			Refresh();
		}
	}
}
