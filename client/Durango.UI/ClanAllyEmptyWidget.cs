using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ClanAllyEmptyWidget : MonoBehaviour
{
	public Action ButtonClicked;

	[SerializeField]
	private SelectableButton _findClanButton;

	[SerializeField]
	private RectLayout _layout;

	private UIWidget _widget;

	private Point2 _size;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	private void Start()
	{
		_layout.UpdateOnSizeChange();
		_findClanButton.Clicked = delegate
		{
			if (ButtonClicked != null)
			{
				ButtonClicked();
			}
		};
	}

	public void Set(bool hasPermission)
	{
		_findClanButton.Disabled = !hasPermission;
	}
}
