using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class FriendTypeWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private SelectableButton _permissionButton;

	public void Set(string title, string description, Action clicked)
	{
		_title.text = title;
		_description.text = description;
		_permissionButton.Clicked = clicked;
	}
}
