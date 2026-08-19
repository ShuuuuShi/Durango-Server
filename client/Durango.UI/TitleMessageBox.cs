using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TitleMessageBox : TitleMessageBoxBase
{
	[SerializeField]
	private VisibleController _controller;

	public override void Show(string title, string message, Action onClick, Action onCancel = null, string okButtonLabel = null, string cancelButtonLabel = null)
	{
		_controller.HideExceptForMe(hide: true, "MessageBox");
		base.Show(title, message, onClick, onCancel, okButtonLabel, cancelButtonLabel);
		_okButton.Widget.bottomAnchor.absolute = ((onCancel == null) ? 54 : 140);
		_okButton.Widget.topAnchor.absolute = ((onCancel == null) ? 120 : 206);
		_okButton.Widget.UpdateAnchors();
	}

	public override void Close()
	{
		_controller.HideExceptForMe(hide: false, "MessageBox");
		base.Close();
	}
}
