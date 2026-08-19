using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Clusters;
using Durango.Player;
using Durango.UI.Control;
using L10N;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class TransferCoinPopup : TooltipBase
{
	[SerializeField]
	private SelectableWidget _coinInputButton;

	[SerializeField]
	private UILabel _selectedCoinAmountLabel;

	[SerializeField]
	private TweenAlpha _emphasisEffectTweener;

	[SerializeField]
	private SelectionMarker _selectedWidgetMarker;

	[SerializeField]
	private KScrollView _scroll;

	[SerializeField]
	private UIWidget _contentArea;

	[SerializeField]
	private UIWidget _coinSelectionArea;

	[SerializeField]
	private UIWidget _confirmArea;

	[SerializeField]
	private UILabel _coinAmountCheckLabel;

	[SerializeField]
	private TransferCoinNode _transferTargetWidget;

	[SerializeField]
	private UIWidget _loadingRingWidget;

	[SerializeField]
	private RectLayoutComponent _rectLayout;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private SelectableButton _cancelButton;

	private int _coinAmount;

	private bool _isSendTargetSelected;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void Start()
	{
		base.Start();
		_coinInputButton.Clicked = delegate
		{
			UIManager.Popup.Tooltip<NumberInputPopup>().Show(title: T._("금액 설정"), initialValue: _coinAmount, currency: Currency.Coin, onConfirm: delegate(long res)
			{
				CheckCoinAmountValidity(res);
				PinchSettingCoinAmount(_coinAmount <= 0);
				SetConfirmButton();
			});
		};
	}

	private bool CheckCoinAmountValidity(long res)
	{
		if (InventorySystem.Wallet.GetBalance(Currency.Coin) < res)
		{
			UIManager.SystemMsg(T._("듀랑고 코인이 부족합니다."));
			SetCoin(0L);
			return false;
		}
		if (res < 1)
		{
			UIManager.SystemMsg(T._("보낼 코인으로 0 을 지정하셨습니다."));
			SetCoin(0L);
			return false;
		}
		SetCoin(res);
		return true;
	}

	private void ActiveWidget(UIWidget target)
	{
		_contentArea.gameObject.SetActive(_contentArea == target);
		_coinSelectionArea.gameObject.SetActive(_contentArea == target);
		_confirmArea.gameObject.SetActive(_confirmArea == target);
		_loadingRingWidget.gameObject.SetActive(_loadingRingWidget == target);
		_rectLayout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	private void SetCoin(long amount)
	{
		_coinAmount = (int)Math.Min(amount, 2147483647L);
		_selectedCoinAmountLabel.text = string.Format("{0}{1}", amount, ("/" + InventorySystem.Wallet.GetBalance(Currency.Coin)).ToEncodedColor(PresetColor.UIMoreLightGray));
	}

	public void Set()
	{
		_selectedWidgetMarker.gameObject.SetActive(value: false);
		ActiveWidget(_loadingRingWidget);
		PinchSettingCoinAmount(on: false);
		_isSendTargetSelected = false;
		SetConfirmButton(delegate
		{
			UIManager.SystemMsg(T._("지금은 코인을 전송할 대상이 없습니다."));
		});
		_cancelButton.Clicked = Hide;
		Clusters.RequestAccounts(GameManager.GatewayUrl, delegate(Account account)
		{
			List<Durango.Logic.Clusters.PlayerInfo> list = ((account != null) ? account.Players.Where((Durango.Logic.Clusters.PlayerInfo o) => !o.IsSoftDeleted).ToList() : new List<Durango.Logic.Clusters.PlayerInfo>());
			if (list.Count < 2)
			{
				UIManager.MessageBox.Show(T._("코인은 자기 캐릭터에게만 전송할 수 있습니다.\n지금은 코인을 전송할 대상이 없습니다."), (Action)Hide, (string)null);
			}
			else
			{
				SetCoin(0L);
				ActiveWidget(_contentArea);
				_scroll.Nodes.BeginLoad();
				int i = 0;
				for (int size = KUtility.GetSize(list); i < size; i++)
				{
					if (!(GameManager.PlayerId == list[i].PlayerEntityId))
					{
						CreatePlayerInfoButton(_scroll.Nodes, list[i].PlayerEntityId);
					}
				}
				_scroll.Nodes.EndLoad();
				_scroll.ResetPosition();
			}
		});
	}

	private void CreatePlayerInfoButton(ListObjectPool nodes, string currentId)
	{
		GameObject next = nodes.GetNext();
		TransferCoinNode comp = next.GetComponent<TransferCoinNode>();
		comp.Set(currentId, delegate(Durango.Player.PlayerInfo info)
		{
			PinchSettingCoinAmount(_coinAmount <= 0);
			_isSendTargetSelected = true;
			SetConfirmButton(delegate
			{
				if (CheckCoinAmountValidity(_coinAmount))
				{
					SwitchToConfirmWindow(info, _coinAmount);
					_selectedWidgetMarker.Set(comp);
					_coinAmountCheckLabel.text = T._("듀랑고 코인 <coin></coin> <em>{0}</em> 을 전송합니다.", _coinAmount.ToString());
				}
			});
			_selectedWidgetMarker.Set(comp);
		});
	}

	private void SwitchToConfirmWindow(Durango.Player.PlayerInfo playerInfo, int coinAmount)
	{
		ActiveWidget(_confirmArea);
		_transferTargetWidget.Set(playerInfo.EntityId, null);
		SetConfirmButton(delegate
		{
			ShopSystem.SendDurangoCoin(playerInfo.EntityId, coinAmount, delegate
			{
				TransferCoinConfirmPopup transferCoinConfirmPopup = UIManager.Popup.Tooltip<TransferCoinConfirmPopup>();
				transferCoinConfirmPopup.Set(playerInfo, coinAmount);
				transferCoinConfirmPopup.Show();
			});
			Hide();
		});
		_cancelButton.Clicked = Set;
	}

	private void SetConfirmButton()
	{
		SetConfirmButton(_confirmButton.Clicked);
	}

	private void SetConfirmButton(Action clicked)
	{
		_confirmButton.Disabled = !_isSendTargetSelected || _coinAmount <= 0;
		_confirmButton.Clicked = clicked;
	}

	protected override void OnTryConfirmOnModal()
	{
		if (!_confirmButton.Disabled && _confirmButton.Clicked != null)
		{
			_confirmButton.Clicked();
		}
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelButton;
	}

	private void PinchSettingCoinAmount(bool on)
	{
		if (on)
		{
			if (!_emphasisEffectTweener.gameObject.activeSelf)
			{
				_emphasisEffectTweener.gameObject.SetActive(value: true);
				_emphasisEffectTweener.PlayForward();
				_emphasisEffectTweener.tweenFactor = 0f;
				_emphasisEffectTweener.Sample(0f, isFinished: false);
			}
		}
		else if (_emphasisEffectTweener.gameObject.activeSelf)
		{
			_emphasisEffectTweener.gameObject.SetActive(value: false);
		}
	}
}
