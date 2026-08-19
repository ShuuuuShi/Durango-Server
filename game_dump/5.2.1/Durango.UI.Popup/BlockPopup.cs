using System;
using Durango.Player;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class BlockPopup : TooltipBase
{
	[SerializeField]
	private UILabel _labelTitle;

	[SerializeField]
	private UITexture _texture;

	[SerializeField]
	private Texture _textureMask;

	[SerializeField]
	private UILabel _textUserName;

	[SerializeField]
	private UIWidget _warningWidget;

	[SerializeField]
	private SelectableButton _buttonYes;

	[SerializeField]
	private SelectableButton _buttonNo;

	[SerializeField]
	private RectLayout _layout;

	private bool _isBlocked;

	private PlayerInfo _playerInfo;

	private Action _onSuccess;

	protected override void OnAwake()
	{
		base.OnAwake();
		SelectableButton buttonYes = _buttonYes;
		buttonYes.Clicked = (Action)Delegate.Combine(buttonYes.Clicked, (Action)delegate
		{
			bool flag = GameSystem<SocialSystem>.Instance().IsBlocked(_playerInfo.EntityId);
			GameSystem<SocialSystem>.Instance().Block(_playerInfo.EntityId, !flag, OnSuccessBlock);
			Hide();
		});
		SelectableButton buttonNo = _buttonNo;
		buttonNo.Clicked = (Action)Delegate.Combine(buttonNo.Clicked, new Action(Hide));
		_buttonNo.Text = T._("취소");
	}

	protected override void OnShow()
	{
		base.OnShow();
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	public void Set(PlayerInfo playerInfo, Action onSuccess)
	{
		_playerInfo = playerInfo;
		_onSuccess = onSuccess;
		_isBlocked = GameSystem<SocialSystem>.Instance().IsBlocked(_playerInfo.EntityId);
	}

	private void OnSuccessBlock()
	{
		if (_isBlocked)
		{
			UIManager.SystemMsg(T._("{0} 님을 차단 해제했습니다.", _playerInfo.GetNameFreq(21, string.Empty)));
		}
		else
		{
			UIManager.SystemMsg(T._("{0} 님을 차단했습니다.", _playerInfo.GetNameFreq(21, string.Empty)));
		}
		if (_onSuccess != null)
		{
			_onSuccess();
		}
		Hide();
	}

	protected override void FillData()
	{
		PortraitBuilder.Argument portraitArgument = _playerInfo.GetPortraitArgument();
		portraitArgument.Mask = _textureMask;
		PortraitBuilder.Set(portraitArgument, _texture);
		_textUserName.text = _playerInfo.GetNameFreq(21, string.Empty);
		if (_isBlocked)
		{
			_labelTitle.text = T._("차단을 해제하시겠습니까?");
			_buttonYes.Text = T._("해제");
			_warningWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_labelTitle.text = T._("차단하시겠습니까?");
			_buttonYes.Text = T._("차단");
			_warningWidget.gameObject.SetActive(value: true);
		}
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
	}
}
