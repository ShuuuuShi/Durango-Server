using System;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ReactingPropPopup : TooltipBase
{
	public struct RequiredItemTags
	{
		private OrTagFilter _tagFilter;

		public int Count { get; private set; }

		public string Icon { get; private set; }

		public string LocalizedTagRequiredMsg { get; private set; }

		public RequiredItemTags(RequiredItems requiredItems)
		{
			_tagFilter = new OrTagFilter(requiredItems.RequiredTags);
			Count = requiredItems.Count;
			Icon = ((KUtility.GetSize(requiredItems.RequiredTags) <= 0) ? "icon_question" : TagData.GetTagIcon(requiredItems.RequiredTags[0]));
			LocalizedTagRequiredMsg = Util.LocalizedTagRequiredMsg(_tagFilter, showLevel: false);
		}

		public bool Filter(ItemData itemData)
		{
			if (itemData.HasTag(_tagFilter, ignoreLevel: true))
			{
				return !itemData.IsDestroyed();
			}
			return false;
		}
	}

	private const string _visibleKey = "ReactingPropPopup";

	[SerializeField]
	private CurrencyWidget _currencyWidget;

	[SerializeField]
	private KWidgetScrollView _scrollView;

	[SerializeField]
	private GameObject _labelEffect;

	[SerializeField]
	private UILabel _textStatusEffect;

	[SerializeField]
	private GameObject _effectLowerMargin;

	[SerializeField]
	private GameObject _labelGivingItems;

	[SerializeField]
	private UIWidget _givingItems;

	[SerializeField]
	private ReactingPropItemWidget _itemWidgetBase;

	[SerializeField]
	private GameObject _durationWidget;

	[SerializeField]
	private UILabel _textDuration;

	[SerializeField]
	private GameObject _cooltimeWidget;

	[SerializeField]
	private UILabel _textCooltime;

	[SerializeField]
	private GameObject _labelResource;

	[SerializeField]
	private UIWidget _requiredResource;

	[SerializeField]
	private ReactingPropResourceWidget _resourceWidgetBase;

	[SerializeField]
	private GameObject _availableAtWidget;

	[SerializeField]
	private UILabel _textAvailableAt;

	[SerializeField]
	private SelectableButton _button;

	[SerializeField]
	private RectLayout _layout;

	private ListObjectPool<ReactingPropResourceWidget> _resourceWidgets;

	private ListObjectPool<ReactingPropItemWidget> _itemWidgets;

	private VisibleController _visibleController;

	private Action _onConfirm;

	private double _availableAt;

	private bool _notAvailable;

	private bool _notEnoughResources;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		_button.CanClickWhenDisabled = true;
		SelectableButton button = _button;
		button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(ButtonClicked));
		_resourceWidgets = new ListObjectPool<ReactingPropResourceWidget>();
		_resourceWidgets.BaseObject = _resourceWidgetBase;
		_resourceWidgets.UseBase = true;
		_resourceWidgets.Init(null);
		_itemWidgets = new ListObjectPool<ReactingPropItemWidget>();
		_itemWidgets.BaseObject = _itemWidgetBase;
		_itemWidgets.UseBase = true;
		_itemWidgets.Init(null);
		_visibleController = base.gameObject.AddMissingComponent<VisibleController>();
		_layout.UpdateLayout();
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
	}

	protected override void OnShow()
	{
		base.OnShow();
		VisibleController.Hide(VisibleType.Base, hide: true, "ReactingPropPopup");
	}

	protected override void OnHide()
	{
		base.OnHide();
		VisibleController.Hide(VisibleType.Base, hide: false, "ReactingPropPopup", 0.1f);
	}

	public void Show(RequiredItemTags? requiredItemTags, Messages.Cost? requiredMoney, Messages.RewardItem[] givingItems, RewardStatusEffect? rewardStatusEffect, Cooltime? cooltime, Action onConfirm)
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		_onConfirm = onConfirm;
		_availableAt = ((!cooltime.HasValue) ? 0.0 : cooltime.Value.AvailableAt);
		RefreshCurrencyWidget(requiredMoney);
		RefreshStatusEffect(rewardStatusEffect);
		RefreshGivingItems(givingItems);
		RefreshTimes(rewardStatusEffect, cooltime);
		RefreshResourceItems(requiredItemTags, requiredMoney);
		RefreshAvailableAtText(_availableAt > predictedServerTime);
		RefreshButtonState();
		_scrollView.ResetPosition();
		Show();
	}

	private void RefreshCurrencyWidget(Messages.Cost? requiredMoney)
	{
		if (requiredMoney.HasValue)
		{
			_currencyWidget.SetCurrencyType(requiredMoney.Value.Currency);
			_currencyWidget.gameObject.SetActive(value: true);
		}
		else
		{
			_currencyWidget.gameObject.SetActive(value: false);
		}
	}

	private void RefreshStatusEffect(RewardStatusEffect? rewardStatusEffect)
	{
		string text = ((!rewardStatusEffect.HasValue) ? string.Empty : GetStatusEffectText(rewardStatusEffect.Value));
		if (!string.IsNullOrEmpty(text))
		{
			_labelEffect.SetActive(value: true);
			_textStatusEffect.gameObject.SetActive(value: true);
			_effectLowerMargin.SetActive(value: true);
			_textStatusEffect.text = text;
		}
		else
		{
			_labelEffect.SetActive(value: false);
			_textStatusEffect.gameObject.SetActive(value: false);
			_effectLowerMargin.SetActive(value: false);
		}
	}

	private void RefreshResourceItems(RequiredItemTags? requiredItemTags, Messages.Cost? requiredMoney)
	{
		_resourceWidgets.BeginLoad();
		string getButtonCurrencyText = null;
		_notEnoughResources = false;
		if (requiredMoney.HasValue)
		{
			if (!_resourceWidgets.GetNext().Set(requiredMoney.Value))
			{
				_notEnoughResources = true;
			}
			AddButtonCurrencyText(ref getButtonCurrencyText, Durango.Logic.Item.Inventory.CurrencyFormat(requiredMoney.Value.Amount, requiredMoney.Value.Currency));
		}
		if (requiredItemTags.HasValue)
		{
			if (!_resourceWidgets.GetNext().Set(requiredItemTags.Value))
			{
				_notEnoughResources = true;
			}
			AddButtonCurrencyText(ref getButtonCurrencyText, Durango.Logic.Item.Inventory.CurrencyFormat(requiredItemTags.Value.Count, requiredItemTags.Value.Icon));
		}
		_resourceWidgets.EndLoad();
		RefreshRequiredResourceWidget();
		SetButtonText(getButtonCurrencyText);
	}

	private void RefreshGivingItems(Messages.RewardItem[] givingItems)
	{
		_itemWidgets.BeginLoad();
		foreach (Messages.RewardItem item in givingItems)
		{
			_itemWidgets.GetNext().Set(item);
		}
		_itemWidgets.EndLoad();
		RefreshGivingItemsWidget();
	}

	private void RefreshTimes(RewardStatusEffect? rewardStatusEffect, Cooltime? cooltime)
	{
		if (rewardStatusEffect.HasValue)
		{
			_durationWidget.SetActive(value: true);
			_textDuration.text = TimedeltaFormatter.Format(rewardStatusEffect.Value.Duration, 2, "min");
		}
		else
		{
			_durationWidget.SetActive(value: false);
		}
		if (cooltime.HasValue)
		{
			_cooltimeWidget.SetActive(value: true);
			_textCooltime.text = TimedeltaFormatter.Format(cooltime.Value.Duration, 2, "min");
		}
		else
		{
			_cooltimeWidget.SetActive(value: false);
		}
	}

	private void RefreshAvailableAtText(bool notAvailable)
	{
		_notAvailable = notAvailable;
		if (_notAvailable)
		{
			_availableAtWidget.gameObject.SetActive(value: true);
			_textAvailableAt.SetText(new SyncString(delegate(out string text, out float period)
			{
				if (SyncString.UpdateRemainTimeMsg(_availableAt, T._("{0} 후 이용 가능"), out text, out period, string.Empty) <= 0.0)
				{
					RefreshAvailableAtText(notAvailable: false);
					RefreshButtonState();
					UpdateLayout();
				}
			}));
		}
		else
		{
			_availableAtWidget.gameObject.SetActive(value: false);
			_textAvailableAt.text = null;
		}
	}

	private void RefreshRequiredResourceWidget()
	{
		_labelResource.SetActive(_resourceWidgets.Count > 0);
		_requiredResource.height = (int)UIUtility.WidgetsReposition(_resourceWidgets, _requiredResource, Vector3.down);
	}

	private void RefreshGivingItemsWidget()
	{
		if (_itemWidgets.Count > 0)
		{
			_labelGivingItems.SetActive(value: true);
			_givingItems.gameObject.SetActive(value: true);
			Vector2 vector = UIUtility.WidgetsGridReposition(_itemWidgets, null, Vector2.down, Vector3.zero, _givingItems.width, new Vector2(_itemWidgetBase.width, _itemWidgetBase.height), 0f, 0f);
			_givingItems.height = (int)vector.y;
		}
		else
		{
			_labelGivingItems.SetActive(value: false);
			_givingItems.gameObject.SetActive(value: false);
		}
	}

	private void SetButtonText(string buttonCurrencyText)
	{
		string text = T._("이용");
		_button.Text = ((!string.IsNullOrEmpty(buttonCurrencyText)) ? (text + "  [preset=round_box?" + buttonCurrencyText + "]") : text);
	}

	private void RefreshButtonState()
	{
		_button.Disabled = _notAvailable || _notEnoughResources;
	}

	private static string GetStatusEffectText(RewardStatusEffect rewardStatusEffect)
	{
		StatusEffectTemplate statusEffectTemplate = StatusEffectTemplateYaml.GetStatusEffectTemplate(rewardStatusEffect.StatusEffectId, rewardStatusEffect.Level);
		if (statusEffectTemplate != null)
		{
			string text = Durango.Logic.StatusEffect.EffectsText(statusEffectTemplate.GetEffects(rewardStatusEffect.Level));
			if (string.IsNullOrEmpty(text))
			{
				text = statusEffectTemplate.Description;
			}
			return text.Trim();
		}
		return string.Empty;
	}

	private static void AddButtonCurrencyText(ref string getButtonCurrencyText, string currency)
	{
		getButtonCurrencyText = ((!string.IsNullOrEmpty(getButtonCurrencyText)) ? (getButtonCurrencyText + "   " + currency) : currency);
	}

	private void ButtonClicked()
	{
		if (_button.Disabled)
		{
			if (_notAvailable)
			{
				double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
				UIManager.SystemMsg(T._("{0} 후 이용 가능합니다.", TimedeltaFormatter.Format(_availableAt - predictedServerTime)));
			}
			else if (_notEnoughResources)
			{
				UIManager.SystemMsg(T._("필요 자원이 부족합니다."));
			}
		}
		else
		{
			Action onConfirm = _onConfirm;
			Hide();
			onConfirm?.Invoke();
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		ButtonClicked();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _button;
	}

	public static RequiredItemTags? GetRequiredItemTags(RequiredItems? requiredItems)
	{
		if (requiredItems.HasValue)
		{
			return new RequiredItemTags(requiredItems.Value);
		}
		return null;
	}
}
