using System;
using Durango.System.Config;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class EngagementPopup : TooltipBase
{
	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _okButton;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(Hide));
		SelectableButton okButton = _okButton;
		okButton.Clicked = (Action)Delegate.Combine(okButton.Clicked, (Action)delegate
		{
			Hide();
			UIManager.FindScript<ConfigGroup>().Open("account");
			ConfigInstance.NotifyAction("engagement");
		});
	}
}
