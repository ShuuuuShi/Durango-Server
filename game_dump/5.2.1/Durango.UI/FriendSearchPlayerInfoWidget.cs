using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class FriendSearchPlayerInfoWidget : PlayerInfoWidget
{
	public Action<string> Requested;

	[SerializeField]
	private Selectable _requestButton;

	public void Start()
	{
		_requestButton.Clicked = OnClickRequest;
	}

	private void OnClickRequest()
	{
		if (Requested != null)
		{
			Requested(base.EntityId);
		}
	}
}
