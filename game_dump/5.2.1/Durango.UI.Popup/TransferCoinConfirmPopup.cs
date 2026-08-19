using Durango.Player;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class TransferCoinConfirmPopup : TooltipBase
{
	[SerializeField]
	private UILabel _sendCoinText;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private TransferCoinNode _targetPlayerInfo;

	[SerializeField]
	private RectLayout _layout;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		_confirmButton.Clicked = Hide;
		_confirmButton.Text = T._("확인");
	}

	public void Set([NotNull] PlayerInfo playerInfo, int coinAmount)
	{
		_targetPlayerInfo.SetContent(playerInfo, null);
		_sendCoinText.text = T._("듀랑고 코인 {0} <em>{1}</em> 이 전송되었습니다.", "[icon=durango_coin]", coinAmount.ToString());
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}

	protected override void OnTryConfirmOnModal()
	{
		Hide();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}
}
