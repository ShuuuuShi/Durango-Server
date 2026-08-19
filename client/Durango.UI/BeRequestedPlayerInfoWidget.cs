using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class BeRequestedPlayerInfoWidget : PlayerInfoWidget
{
	public Action<string> Accepted;

	public Action<string> Rejected;

	[SerializeField]
	private SelectableButton _acceptButton;

	[SerializeField]
	private SelectableButton _rejectButton;

	public void Start()
	{
		_acceptButton.Clicked = OnClickAccept;
		_rejectButton.Clicked = OnClickReject;
		_acceptButton.Text = T._("수락");
		_rejectButton.Text = T._("거절");
	}

	private void OnClickAccept()
	{
		if (Accepted != null)
		{
			Accepted(base.EntityId);
		}
	}

	private void OnClickReject()
	{
		if (Rejected != null)
		{
			Rejected(base.EntityId);
		}
	}
}
