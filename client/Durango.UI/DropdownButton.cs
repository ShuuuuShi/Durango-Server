using System;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class DropdownButton : SelectableWidget
{
	public Action<int> ButtonClicked;

	[SerializeField]
	private UILabel _label;

	public int Index { get; private set; }

	private void Awake()
	{
		Clicked = (Action)Delegate.Combine(Clicked, (Action)delegate
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			ButtonClicked(Index);
		});
	}

	public void Set(string text, int index)
	{
		_label.text = text;
		Index = index;
	}
}
