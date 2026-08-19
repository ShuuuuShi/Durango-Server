using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class ToDoListHandleWidget : SelectableWidget
{
	[SerializeField]
	private TweenerPlayer _appearTweenerPlayer;

	public void Show(bool show)
	{
		if (show)
		{
			_appearTweenerPlayer.ResetToFirst();
			_appearTweenerPlayer.Play();
		}
		else
		{
			_appearTweenerPlayer.ResetToLast();
			_appearTweenerPlayer.Play(forward: false, null);
		}
	}
}
