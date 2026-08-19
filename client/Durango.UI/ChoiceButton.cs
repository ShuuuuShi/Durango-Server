using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ChoiceButton : SelectableWidget
{
	[SerializeField]
	private UILabel _label;

	public int Index { get; private set; }

	public void Set(string text, int index)
	{
		_label.text = T._(text);
		Index = index;
	}
}
