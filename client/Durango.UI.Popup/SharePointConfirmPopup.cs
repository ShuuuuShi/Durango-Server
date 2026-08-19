using System;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using Messages;
using Shared.Chat;
using UnityEngine;

namespace Durango.UI.Popup;

public class SharePointConfirmPopup : TooltipBase
{
	[SerializeField]
	private GameObject _close;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private SelectableButton _cancelButton;

	private Vector2 _tilePos;

	private Action _onConfirm;

	private Action _onCancel;

	private Action _onClose;

	private ChannelType? _channelType;

	private string _conversationId;

	private bool _showLoadingRing;

	private string _balloonEntityId;

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

	protected override void OnAwake()
	{
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Default);
		base.Widget.SetAnchor(rootAnchor.gameObject, 0, 0, 0, 0);
		UIEventListener.Get(_close).onClick = CloseButton_Clicked;
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(ConfirmButton_Clicked));
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(CancelButton_Clicked));
	}

	public void Set(Vector2 tilePos, Action onConfirm, Action onCancel, Action onClose, ChannelType? channelType = null, string conversationId = null)
	{
		_tilePos = tilePos;
		_onConfirm = onConfirm;
		_onCancel = onCancel;
		_onClose = onClose;
		_channelType = channelType;
		_conversationId = conversationId;
	}

	protected override void FillData()
	{
		ShowLoadingRing(show: true);
		RequestShowBalloon();
	}

	private void RequestShowBalloon()
	{
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(PlayerBehavior.LocalPlayer.EntityId, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid && _showLoadingRing)
			{
				Singleton<MapIndicators>.Instance().AddAnnounceBalloon(AnnounceType.SharePinPoint, _tilePos, info);
				_balloonEntityId = info.EntityId;
				ShowLoadingRing(show: false);
			}
		});
	}

	private void HideBalloon()
	{
		if (!string.IsNullOrEmpty(_balloonEntityId))
		{
			Singleton<MapIndicators>.Instance().RemoveAnnounceBalloon(AnnounceType.SharePinPoint, _balloonEntityId);
			_balloonEntityId = null;
		}
	}

	private void ShowLoadingRing(bool show)
	{
		LoadingRingWidget loadingRing = UIManager.Popup.LoadingRing;
		if (show)
		{
			loadingRing.AttachToWidget(base.gameObject);
			loadingRing.ShowInstantly();
		}
		else
		{
			loadingRing.DetachFromWidget(base.gameObject);
		}
		_showLoadingRing = show;
	}

	private void ConfirmButton_Clicked()
	{
		GameSystem<SocialSystem>.Instance().SystemSay(new RadioPin
		{
			RegionId = GameManager.Region.Id,
			Tile = new Point2((int)_tilePos.x, (int)_tilePos.y)
		}, _channelType, _conversationId);
		ShowLoadingRing(show: false);
		Hide();
		if (_onConfirm != null)
		{
			_onConfirm();
		}
	}

	private void CancelButton_Clicked()
	{
		ShowLoadingRing(show: false);
		HideBalloon();
		Hide();
		if (_onCancel != null)
		{
			_onCancel();
		}
	}

	private void CloseButton_Clicked(GameObject go)
	{
		ShowLoadingRing(show: false);
		HideBalloon();
		Hide();
		if (_onClose != null)
		{
			_onClose();
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		ConfirmButton_Clicked();
	}

	protected override void OnTryCancelOnModal()
	{
		CancelButton_Clicked();
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
}
