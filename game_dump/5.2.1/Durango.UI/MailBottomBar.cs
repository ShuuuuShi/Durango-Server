using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MailBottomBar : MonoBehaviour
{
	public Action AcceptAllClicked;

	[SerializeField]
	private SelectableButton _acceptAllButton;

	public void Init()
	{
		_acceptAllButton.Text = T._("모두 받기");
		SelectableButton acceptAllButton = _acceptAllButton;
		acceptAllButton.Clicked = (Action)Delegate.Combine(acceptAllButton.Clicked, new Action(OnClick_AcceptAllButton));
	}

	private void OnClick_AcceptAllButton()
	{
		if (AcceptAllClicked != null)
		{
			AcceptAllClicked();
		}
	}
}
