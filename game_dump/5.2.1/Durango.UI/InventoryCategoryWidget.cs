using System;
using Durango.Logic.Market;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class InventoryCategoryWidget : UIWidget
{
	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private SelectableWidget _button;

	public void Set(Category.Main data, bool isSelected, Action<Category.Main> clicked)
	{
		_text.text = data.Name;
		_button.Clicked = delegate
		{
			if (clicked != null)
			{
				clicked(data);
			}
		};
		SetSelection(isSelected);
	}

	public void SetSelection(bool isSelected)
	{
		_button.Selected = isSelected;
	}
}
