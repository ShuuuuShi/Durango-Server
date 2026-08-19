using System;
using System.Collections;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MessageBox : UIBase
{
	public enum Position
	{
		Top,
		Center
	}

	public struct Button
	{
		public string Text;

		public PresetButton.Style Style;

		public string Sound;

		public bool Disabled;

		public PresetButton.Effect Effect;

		public Button(string text, PresetButton.Style style = PresetButton.Style.Solid, string sound = null, bool disabled = false, PresetButton.Effect effect = PresetButton.Effect.None)
		{
			Text = text;
			Style = style;
			Sound = sound;
			Disabled = disabled;
			Effect = effect;
		}

		public static implicit operator Button(string value)
		{
			Button result = default(Button);
			result.Text = value;
			result.Style = PresetButton.Style.Border;
			return result;
		}
	}

	private struct CustomWidget
	{
		public UIWidget Widget;

		public Transform OriginParent;
	}

	[SerializeField]
	private GameObject _messageBox;

	[SerializeField]
	private SelectableButton _baseButton;

	[SerializeField]
	private UILabel _mainTextLabel;

	[SerializeField]
	private UILabel _subTextLabel;

	[SerializeField]
	private UILabel _lowerTextLabel;

	[SerializeField]
	private MessageBoxInfoWidget _infoWidget;

	[SerializeField]
	private Transform _mainContainer;

	[SerializeField]
	private UIWidget _currencyContainer;

	[SerializeField]
	private CurrencyWidgetBase _baseCurrencyWidget;

	[SerializeField]
	private MessageBoxSlideSelector _slideSelector;

	[SerializeField]
	private UIWidget _modelViewer;

	[SerializeField]
	private ClanAllyInfoWidget _clanAllyInfoWidget;

	[SerializeField]
	private int _minButtonWidth;

	private Action _onOk;

	private Action<bool> _onOkCancel;

	private Action<int> _onSelect;

	private ListObjectPool<SelectableButton> _buttons;

	private List<UIWidget>[] _reservedCustomWidgets;

	private List<CustomWidget>[] _customWidgets;

	private readonly List<Currency> _currencies = new List<Currency>();

	private readonly List<string> _vouchers = new List<string>();

	private bool _viewClanFund;

	private string _lowerText;

	private ListObjectPool<CurrencyWidgetBase> _currencyWidgets;

	private bool _isWait;

	private float _hideAt;

	private AnimationWidget _animWidget;

	public MessageBoxSlideSelector SlideSelector => _slideSelector;

	public UIWidget ModelViewer => _modelViewer;

	public ClanAllyInfoWidget ClanAllyInfo => _clanAllyInfoWidget;

	public bool IsShow { get; private set; }

	private void Start()
	{
		Position[] array = Enums<Position>.All();
		_reservedCustomWidgets = new List<UIWidget>[array.Length];
		_customWidgets = new List<CustomWidget>[array.Length];
		for (int i = 0; i < array.Length; i++)
		{
			_reservedCustomWidgets[i] = new List<UIWidget>();
			_customWidgets[i] = new List<CustomWidget>();
		}
		_animWidget = _messageBox.GetComponent<AnimationWidget>();
		_animWidget.Widget.alpha = 0f;
		NGUITools.SetLayer(base.gameObject, LayerHelper.UIOverLayer);
		_buttons = new ListObjectPool<SelectableButton>();
		_buttons.BaseObject = _baseButton;
		_buttons.UseBase = true;
		_buttons.Init(Init_Buttons);
		if (_baseCurrencyWidget != null)
		{
			_currencyWidgets = new ListObjectPool<CurrencyWidgetBase>();
			_currencyWidgets.BaseObject = _baseCurrencyWidget;
		}
		SetChildrenActive(activated: false);
	}

	private void Update()
	{
		if (IsShow && _hideAt > 0f)
		{
			float time = Time.time;
			if (_hideAt < time)
			{
				Hide();
			}
		}
	}

	protected override void OnScreenResized()
	{
		base.OnScreenResized();
		UpdateLayout();
	}

	private void Init_Buttons(SelectableButton btn)
	{
		btn.Clicked = OnButtonClick;
	}

	private void OnButtonClick()
	{
		Show(isShow: false);
		int num = _buttons.IndexOf((SelectableButton)Selectable.Current);
		if (num != -1)
		{
			if (_onOk != null)
			{
				if (num == 0)
				{
					_onOk();
				}
			}
			else if (_onOkCancel != null)
			{
				switch (num)
				{
				case 0:
					_onOkCancel(obj: true);
					break;
				case 1:
					_onOkCancel(obj: false);
					break;
				}
			}
			else if (_onSelect != null)
			{
				_onSelect(num);
			}
		}
		if (!IsShow && !_isWait)
		{
			Clear();
		}
	}

	public void SetCustomWidget(UIWidget widget, Position position)
	{
		List<UIWidget> list = _reservedCustomWidgets.Get((int)position);
		list.Add(widget);
	}

	public void SetHideTimer(float hideAt)
	{
		_hideAt = hideAt;
	}

	public void SetCurrencyInfo(Currency currency)
	{
		_currencies.Add(currency);
	}

	public void SetVoucherInfo(string voucherId)
	{
		_vouchers.Add(voucherId);
	}

	public void SetClanFund()
	{
		_viewClanFund = true;
	}

	public void AddKeyValueInfo(SyncString key, SyncString value)
	{
		_infoWidget.Add(key, value);
	}

	public void SetLowerText(string lowerText)
	{
		_lowerText = lowerText;
	}

	public void Show(string mainText, Action onOk = null, string confirm = null)
	{
		Show(mainText, null, onOk, confirm);
	}

	public void Show(string mainText, string subText, Action onOk = null, string confirm = null)
	{
		_onOk = onOk;
		_onOkCancel = null;
		_onSelect = null;
		ShowImplement(mainText, subText, new Button(confirm ?? T._("확인")));
	}

	public void Show(string mainText, Action<bool> onOkCancel, string confirm = null, string cancel = null)
	{
		Show(mainText, null, onOkCancel, confirm, cancel);
	}

	public void Show(string mainText, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)
	{
		_onOk = null;
		_onOkCancel = onOkCancel;
		_onSelect = null;
		if (string.IsNullOrEmpty(confirm))
		{
			confirm = T._("확인");
		}
		if (string.IsNullOrEmpty(cancel))
		{
			cancel = T._("취소");
		}
		ShowImplement(mainText, subText, new Button(confirm), cancel);
	}

	public void ShowLockConfirm([NotNull] ItemData item, Action onOk)
	{
		if (item.SafeLevel != 0)
		{
			ShowLockItemConfirm(onOk, item.Name, 1, item.SafeLevel);
		}
		else
		{
			onOk();
		}
	}

	public void ShowLockConfirm([NotNull] IList<ItemData> items, [NotNull] Action<string[]> onOk)
	{
		string[] itemIds = Util.ItemsToIds(items);
		for (int i = 0; i < items.Count; i++)
		{
			ItemData itemData = items[i];
			if (itemData.SafeLevel != 0)
			{
				ShowLockItemConfirm(delegate
				{
					onOk(itemIds);
				}, itemData.Name, items.Count, itemData.SafeLevel);
				return;
			}
		}
		onOk(itemIds);
	}

	private void ShowLockItemConfirm(Action onOk, string lockedItemName, int count, SafeLevel safeLevel)
	{
		_onOk = onOk;
		_onOkCancel = null;
		_onSelect = null;
		string mainText = null;
		switch (safeLevel)
		{
		case SafeLevel.Locked:
			mainText = ((count <= 1) ? T._("<em>잠금</em> 설정된 <em>{0}</em>{0:-을} 사용하시겠습니까?", lockedItemName) : T._("<em>잠금</em> 설정된 <em>{0}</em> 외 {1}개 물품을 사용하시겠습니까?", lockedItemName, count - 1));
			break;
		case SafeLevel.Protected:
			mainText = ((count <= 1) ? T._("<em>임무</em> 수행에 필요한 <em>{0}</em>{0:-을} 사용하시겠습니까?", lockedItemName) : T._("<em>임무</em> 수행에 필요한 <em>{0}</em> 외 {1}개 물품을 사용하시겠습니까?", lockedItemName, count - 1));
			break;
		}
		ShowImplement(mainText, null, new Button(T._("확인")), T._("취소"));
	}

	public void ShowCostConfirm(Cost cost, string comment, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)
	{
		if (cost == null)
		{
			Show(comment, subText, onOkCancel, confirm, cancel);
			return;
		}
		if (cost.PayableByVoucher())
		{
			int voucherCount = InventorySystem.Wallet.GetVoucherCount(cost.VoucherId);
			if (voucherCount > cost.VoucherAmount)
			{
				ShowPayConfirmWithVoucher(cost.VoucherAmount, cost.VoucherId, comment, subText, onOkCancel, confirm, cancel);
				return;
			}
		}
		ShowPayConfirm(cost.GetAmount(), cost.Currency, comment, subText, onOkCancel, confirm, cancel);
	}

	public void ShowPayConfirmWithVoucher(int cost, string voucherId, string comment, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)
	{
		Voucher voucher = SingletonDict<string, Voucher>.Get(voucherId);
		if (!voucher.IsValid())
		{
			UIManager.SystemMsg(T._("잘못된 이용권입니다."));
			return;
		}
		if (string.IsNullOrEmpty(confirm))
		{
			confirm = T._("확인");
		}
		if (cost > 0)
		{
			confirm = $"{confirm}  {voucher.GetEmphasisCostFormat(cost)}";
		}
		SetVoucherInfo(voucherId);
		Show(comment, subText, onOkCancel, confirm);
	}

	public void ShowPayConfirm(long cost, Currency currency, string comment, Action<bool> onOkCancel, string confirm = null, string cancel = null)
	{
		ShowPayConfirm(cost, currency, comment, null, onOkCancel, confirm, cancel);
	}

	public void ShowPayConfirm(long cost, Currency currency, string comment, string subText, Action<bool> onOkCancel, string confirm = null, string cancel = null)
	{
		if (string.IsNullOrEmpty(confirm))
		{
			confirm = T._("확인");
		}
		if (cost > 0 && currency != Currency.Invalid)
		{
			confirm = Inventory.ToCurrencyButtonText(confirm, cost, currency);
		}
		SetCurrencyInfo(currency);
		Show(comment, subText, onOkCancel, confirm, cancel);
	}

	public void Show(string mainText, Action<int> onSelect, params Button[] items)
	{
		Show(mainText, null, onSelect, items);
	}

	public void Show(string mainText, string subText, Action<int> onSelect, params Button[] items)
	{
		_onOk = null;
		_onOkCancel = null;
		_onSelect = onSelect;
		ShowImplement(mainText, subText, items);
	}

	private void ShowImplement(string mainText, string subText, params Button[] buttons)
	{
		_hideAt = 0f;
		SetComment(mainText, subText);
		SetCustomWidget(_reservedCustomWidgets);
		int i = 0;
		for (int size = KUtility.GetSize(_reservedCustomWidgets); i < size; i++)
		{
			_reservedCustomWidgets[i].Clear();
		}
		_infoWidget.Refresh();
		SetCurrencyInfos();
		SetButtons(buttons);
		if (string.IsNullOrEmpty(_lowerText))
		{
			_lowerTextLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_lowerTextLabel.text = _lowerText;
			_lowerTextLabel.gameObject.SetActive(value: true);
			_lowerText = null;
		}
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if (loadingCurtainGroup != null && loadingCurtainGroup.IsVisible)
		{
			if (!_isWait)
			{
				StartCoroutine(CoLateShow());
			}
		}
		else
		{
			LateShow();
		}
	}

	private void ClickOkButton()
	{
		if (IsShow && !_isWait)
		{
			Show(isShow: false);
			if (_onOk != null)
			{
				_onOk();
			}
			else if (_onOkCancel != null)
			{
				_onOkCancel(obj: true);
			}
			Clear();
		}
	}

	private void ClickCancelButton()
	{
		if (IsShow && !_isWait)
		{
			Hide(byBackButton: true);
		}
	}

	private void SetComment(string mainText, string subText)
	{
		if (string.IsNullOrEmpty(mainText))
		{
			_mainTextLabel.gameObject.SetActive(value: false);
		}
		else
		{
			_mainTextLabel.text = mainText;
			_mainTextLabel.gameObject.SetActive(value: true);
		}
		if (string.IsNullOrEmpty(subText))
		{
			_subTextLabel.gameObject.SetActive(value: false);
			return;
		}
		_subTextLabel.text = subText;
		_subTextLabel.gameObject.SetActive(value: true);
	}

	private void SetCustomWidget(List<UIWidget>[] widgets)
	{
		int i = 0;
		for (int size = KUtility.GetSize(_customWidgets); i < size; i++)
		{
			int j = 0;
			for (int size2 = KUtility.GetSize(_customWidgets[i]); j < size2; j++)
			{
				CustomWidget customWidget = _customWidgets[i][j];
				customWidget.Widget.transform.parent = customWidget.OriginParent;
				customWidget.Widget.gameObject.SetActive(value: false);
			}
			_customWidgets[i].Clear();
		}
		if (widgets == null)
		{
			return;
		}
		int k = 0;
		for (int size3 = KUtility.GetSize(widgets); k < size3; k++)
		{
			List<CustomWidget> list = _customWidgets[k];
			int l = 0;
			for (int size4 = KUtility.GetSize(widgets[k]); l < size4; l++)
			{
				UIWidget uIWidget = widgets[k][l];
				Transform transform = uIWidget.transform;
				CustomWidget item = default(CustomWidget);
				item.OriginParent = transform.parent;
				item.Widget = uIWidget;
				transform.parent = _mainContainer;
				transform.localScale = Vector3.one;
				transform.gameObject.SetActive(value: true);
				NGUITools.SetLayer(transform.gameObject, LayerHelper.UIOverLayer);
				list.Add(item);
			}
		}
	}

	private void SetCurrencyInfos()
	{
		if (_currencyWidgets == null)
		{
			return;
		}
		_currencyWidgets.BeginLoad();
		foreach (Currency currency in _currencies)
		{
			_currencyWidgets.GetNext().SetCurrencyType(currency);
		}
		foreach (string voucher in _vouchers)
		{
			_currencyWidgets.GetNext().SetVoucherType(voucher);
		}
		if (_viewClanFund)
		{
			_currencyWidgets.GetNext().SetClanFund();
		}
		_currencyWidgets.EndLoad();
		if (_currencyWidgets.Count > 0)
		{
			_currencyContainer.gameObject.SetActive(value: true);
			UIUtility.WidgetsReposition(_currencyWidgets, _currencyContainer, Vector3.left, 20f);
		}
		else
		{
			_currencyContainer.gameObject.SetActive(value: false);
		}
		_vouchers.Clear();
		_currencies.Clear();
		_viewClanFund = false;
	}

	private void SetButtons(Button[] values)
	{
		int size = KUtility.GetSize(values);
		_buttons.Set(size);
		int num = -1;
		int num2 = -1;
		if (_onOk != null)
		{
			num = 0;
		}
		else if (_onOkCancel != null)
		{
			num = 0;
			num2 = 1;
		}
		else if (_onSelect != null)
		{
			num2 = size - 1;
		}
		for (int i = 0; i < size; i++)
		{
			SelectableButton component = _buttons[i].GetComponent<SelectableButton>();
			Button button = values[i];
			component.Text = button.Text;
			component.SetStyle(button.Style);
			component.SetClickSound(button.Sound);
			component.Disabled = button.Disabled;
			component.SetEffect(button.Effect);
			if (num == i)
			{
				component.ShortcutCommand = InputCommand.ConfirmModalPopup;
			}
			else if (num2 == i)
			{
				component.ShortcutCommand = InputCommand.CancelModalPopup;
			}
			else
			{
				component.ShortcutCommand = InputCommand.None;
			}
		}
	}

	private IEnumerator CoLateShow()
	{
		_isWait = true;
		LoadingCurtainGroup loadingCurtain = UIManager.FindScript<LoadingCurtainGroup>();
		while (loadingCurtain != null && loadingCurtain.IsVisible)
		{
			yield return null;
		}
		_isWait = false;
		LateShow();
	}

	private void LateShow()
	{
		if (_buttons.Count == 0)
		{
			Show(isShow: false);
			return;
		}
		UpdateLayout();
		Show(isShow: true);
	}

	private void UpdateLayout()
	{
		int num = _minButtonWidth;
		int count = _buttons.Count;
		for (int i = 0; i < count; i++)
		{
			SelectableButton component = _buttons[i].GetComponent<SelectableButton>();
			int x = component.GetPreferredSize().x;
			num = Mathf.Max(x, num);
		}
		float height = 0f;
		UpdateCustomWidgetsLayout(Position.Top, ref height, 0f);
		if (_mainTextLabel.gameObject.activeSelf)
		{
			if (height > 0f)
			{
				height += 20f;
			}
			_mainTextLabel.SetPosition(new Vector3(0f, 0f - height), 0.5f, 1f);
			height += _mainTextLabel.printedSize.y;
		}
		if (_subTextLabel.gameObject.activeSelf)
		{
			if (height > 0f)
			{
				height += 35f;
			}
			_subTextLabel.SetPosition(new Vector3(0f, 0f - height), 0.5f, 1f);
			height += _subTextLabel.printedSize.y;
		}
		if (_infoWidget.gameObject.activeSelf)
		{
			if (height > 0f)
			{
				height += 30f;
			}
			_infoWidget.SetPosition(new Vector3(0f, 0f - height), 0.5f, 1f);
			height += (float)_infoWidget.height;
		}
		UpdateCustomWidgetsLayout(Position.Center, ref height, 30f);
		for (int j = 0; j < count; j++)
		{
			height = ((j <= 0) ? (height + 50f) : (height + 30f));
			SelectableButton selectableButton = _buttons[j];
			selectableButton.Widget.width = num;
			selectableButton.Widget.SetPosition(new Vector3(0f, 0f - height), 0.5f, 1f);
			height += (float)selectableButton.Widget.height;
		}
		_mainContainer.transform.localPosition = new Vector3(0f, height * 0.5f);
	}

	private void UpdateCustomWidgetsLayout(Position type, ref float height, float spacing)
	{
		List<CustomWidget> list = _customWidgets.Get((int)type);
		if (KUtility.GetSize(list) != 0)
		{
			height += spacing;
			for (int i = 0; i < list.Count; i++)
			{
				CustomWidget customWidget = list[i];
				customWidget.Widget.SetPosition(new Vector3(0f, 0f - height), 0.5f, 1f);
				height += customWidget.Widget.height;
			}
		}
	}

	public void Hide(bool byBackButton = false)
	{
		if (byBackButton)
		{
			if (_onOkCancel != null)
			{
				_onOkCancel(obj: false);
			}
			if (_onSelect != null)
			{
				_onSelect(_buttons.Count - 1);
			}
		}
		Clear();
		Show(isShow: false);
	}

	private void Clear()
	{
		_onOk = null;
		_onOkCancel = null;
		_onSelect = null;
	}

	protected virtual void Show(bool isShow)
	{
		if (isShow == IsShow)
		{
			return;
		}
		IsShow = isShow;
		if (isShow)
		{
			BlurController.BlurOn("MessageBox", BlurController.Mask.UI);
			VisibleController.Hide(VisibleType.Base, hide: true, "MessageBox");
			DialogueGroupBase dialogueGroupBase = UIManager.FindScript<DialogueGroupBase>();
			if (dialogueGroupBase != null)
			{
				dialogueGroupBase.SetVisible(visible: false, "MessageBox");
			}
			_messageBox.gameObject.SetActive(value: true);
			_animWidget.Alpha = 1f;
			GameSystem<InputSystem>.Instance().On(InputCommand.ConfirmModalPopup, delegate
			{
				ClickOkButton();
			});
			GameSystem<InputSystem>.Instance().On(InputCommand.CancelModalPopup, delegate
			{
				ClickCancelButton();
			});
		}
		else
		{
			BlurController.BlurOff("MessageBox");
			VisibleController.Hide(VisibleType.Base, hide: false, "MessageBox", 0.1f);
			DialogueGroupBase dialogueGroupBase2 = UIManager.FindScript<DialogueGroupBase>();
			if (dialogueGroupBase2 != null)
			{
				dialogueGroupBase2.SetVisible(visible: true, "MessageBox");
			}
			_animWidget.Alpha = 0f;
			SetCustomWidget(null);
			GameSystem<InputSystem>.Instance().Off(InputCommand.ConfirmModalPopup, delegate
			{
				ClickOkButton();
			});
			GameSystem<InputSystem>.Instance().Off(InputCommand.CancelModalPopup, delegate
			{
				ClickCancelButton();
			});
		}
	}
}
