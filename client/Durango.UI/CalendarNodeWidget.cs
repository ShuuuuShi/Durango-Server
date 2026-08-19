using System;
using Durango.Logic.Event;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Attendance;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class CalendarNodeWidget : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UISprite _currencyIconSprite;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _levelLabel;

	[SerializeField]
	private UISprite _emBackground;

	[SerializeField]
	private GlitteringDots _readyEffectObject;

	[SerializeField]
	private TweenerPlayer _completedEffecObject;

	[SerializeField]
	private GameObject _restorableMarker;

	[SerializeField]
	private UILabel _attendanceDateLabel;

	[SerializeField]
	private UILabel _attendanceOrderLabel;

	private Point2 _readyEffectParentSize;

	private bool _isInit;

	public CalenderReward Reward { get; private set; }

	public event Action Clicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_readyEffectObject.transform.parent.GetComponent<UIWidget>().AddOnChange(delegate
		{
			UIWidget component = _readyEffectObject.transform.parent.GetComponent<UIWidget>();
			Point2 point = new Point2(component.width, component.height);
			if (!(point == _readyEffectParentSize))
			{
				_readyEffectParentSize = point;
				Vector3[] localCorners = component.localCorners;
				ref Vector3 reference = ref _readyEffectObject.Points[0];
				reference = localCorners[0] + new Vector3(2f, 2f);
				ref Vector3 reference2 = ref _readyEffectObject.Points[1];
				reference2 = localCorners[1] + new Vector3(2f, -2f);
				ref Vector3 reference3 = ref _readyEffectObject.Points[2];
				reference3 = localCorners[2] + new Vector3(-2f, -2f);
				ref Vector3 reference4 = ref _readyEffectObject.Points[3];
				reference4 = localCorners[3] + new Vector3(-2f, 2f);
				if (_readyEffectObject.IsShow)
				{
					_readyEffectObject.Initialize();
				}
			}
		});
	}

	public void Set(CalenderReward reward, bool highlight)
	{
		Init();
		RewardState state = Reward.State;
		Reward = reward;
		SetIcon(reward);
		SetCount(reward);
		SetCaption(reward);
		SetAttendanceDate(reward.Index + 1);
		SetLabelColor(highlight);
		_readyEffectObject.gameObject.SetActive(reward.State == RewardState.Ready);
		_completedEffecObject.gameObject.SetActive(reward.State == RewardState.Completed);
		if (_restorableMarker != null)
		{
			_restorableMarker.SetActive(reward.State == RewardState.Restorable);
		}
		switch (reward.State)
		{
		case RewardState.Completed:
			if (state == RewardState.Ready || state == RewardState.Restorable)
			{
				_completedEffecObject.Play();
			}
			else
			{
				_completedEffecObject.ResetToLast();
			}
			break;
		case RewardState.Ready:
			_readyEffectObject.enabled = true;
			break;
		}
	}

	private void SetLabelColor(bool highlight)
	{
		Color color = new Color32(byte.MaxValue, 216, 91, byte.MaxValue);
		Color color2 = ((!highlight) ? Color.white : color);
		if (_attendanceDateLabel != null)
		{
			_attendanceDateLabel.color = color2;
		}
		_countLabel.color = color2;
		_levelLabel.color = color2;
	}

	private void SetAttendanceDate(int dateOrder)
	{
		if (_attendanceDateLabel != null)
		{
			_attendanceDateLabel.text = T._("{0}일", dateOrder);
		}
		if (_attendanceOrderLabel != null)
		{
			_attendanceOrderLabel.text = dateOrder.ToString();
		}
	}

	private void SetIcon(CalenderReward reward)
	{
		if (reward.Item != null)
		{
			_iconTexture.gameObject.SetActive(value: true);
			_currencyIconSprite.gameObject.SetActive(value: false);
			_iconTexture.SetIcon(reward.Item);
		}
		else if (reward.Money.Currency != Currency.Invalid && reward.Money.Amount > 0)
		{
			_iconTexture.gameObject.SetActive(value: false);
			_currencyIconSprite.gameObject.SetActive(value: true);
			_currencyIconSprite.spriteName = Durango.Logic.Item.Inventory.GetIcon(reward.Money.Currency);
			UIUtility.ResizeToSquare(_currencyIconSprite);
		}
		else if (reward.Voucher.HasValue)
		{
			VoucherInfo value = reward.Voucher.Value;
			if (string.IsNullOrEmpty(value.VoucherId) || value.Count <= 0)
			{
			}
			Voucher voucher = SingletonDict<string, Voucher>.Get(value.VoucherId);
			if (!voucher.IsValid())
			{
			}
			_iconTexture.gameObject.SetActive(value: false);
			_currencyIconSprite.gameObject.SetActive(value: true);
			_currencyIconSprite.spriteName = voucher.Icon;
			_currencyIconSprite.color = voucher.GetHexColor().ToColor();
			UIUtility.ResizeToSquare(_currencyIconSprite);
		}
		else
		{
			_iconTexture.gameObject.SetActive(value: false);
			_currencyIconSprite.gameObject.SetActive(value: false);
		}
	}

	private void SetCount(CalenderReward reward)
	{
		int num = 0;
		if (reward.Item != null)
		{
			num = reward.ItemCount;
		}
		else if (reward.Money.Currency != Currency.Invalid && reward.Money.Amount > 0)
		{
			num = reward.Money.Amount;
		}
		else if (reward.Voucher.HasValue)
		{
			num = reward.Voucher.Value.Count;
		}
		if (num > 0)
		{
			_countLabel.text = num.ToString();
			_countLabel.gameObject.SetActive(value: true);
		}
		else
		{
			_countLabel.gameObject.SetActive(value: false);
		}
	}

	private void SetCaption(CalenderReward reward)
	{
		switch (reward.Type)
		{
		case RewardType.None:
			_emBackground.gameObject.SetActive(value: false);
			_titleLabel.gameObject.SetActive(value: false);
			break;
		case RewardType.Rare:
			_emBackground.gameObject.SetActive(value: true);
			_titleLabel.gameObject.SetActive(value: false);
			break;
		}
		if (reward.Item == null)
		{
			_levelLabel.gameObject.SetActive(value: false);
			return;
		}
		_levelLabel.gameObject.SetActive(value: true);
		_levelLabel.text = T._("{0:lv:}", reward.Item.Level);
	}

	[UsedImplicitly]
	private void OnClick()
	{
		UISound.PlayClick(UISound.ClickType.CalendarItem);
		if (Reward.Item != null)
		{
			ItemInfoTooltip itemInfoTooltip = UIManager.Popup.Tooltip<ItemInfoTooltip>();
			itemInfoTooltip.Set(Reward.Item);
			itemInfoTooltip.Direction = TooltipBase.TooltipDirection.Horizontal;
			itemInfoTooltip.Show(GetComponent<UIWidget>(), Vector2.zero, 60f);
		}
		else if (Reward.Money.Currency != Currency.Invalid && Reward.Money.Amount > 0)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(Durango.Logic.Item.Inventory.CurrencyFormat(Reward.Money.Amount, Reward.Money.Currency), null);
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(GetComponent<UIWidget>(), Vector2.zero, 60f);
		}
		else if (Reward.Voucher.HasValue)
		{
			WidgetTooltipControl widgetTooltipControl2 = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			Voucher voucher = SingletonDict<string, Voucher>.Get(Reward.Voucher.Value.VoucherId);
			string arg = T._("해당 이용권은 <em>상점</em> 메뉴 내 <em>특송 화물</em> 카테고리의 <em>이용권 보기</em> 항목을 통해 확인할 수 있습니다.");
			widgetTooltipControl2.Set(voucher.Name, $"{voucher.Description}\n{arg}");
			widgetTooltipControl2.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl2.Show(GetComponent<UIWidget>(), Vector2.zero, 60f);
		}
		if (this.Clicked != null)
		{
			this.Clicked();
		}
	}
}
