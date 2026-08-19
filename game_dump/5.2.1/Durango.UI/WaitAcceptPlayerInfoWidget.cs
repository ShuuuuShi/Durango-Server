using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class WaitAcceptPlayerInfoWidget : PlayerInfoWidget
{
	public Action<string> Canceled;

	[SerializeField]
	private SelectableButton _cancelButton;

	public void Start()
	{
		_cancelButton.Clicked = OnClickCancel;
		_cancelButton.Text = T._("요청 취소");
	}

	private void OnClickCancel()
	{
		if (Canceled != null)
		{
			Canceled(base.EntityId);
		}
	}
}
