using UnityEngine;

namespace Durango.UI;

public class ContextActionButton : ContextActionButtonBase
{
	protected override void SetState(State state)
	{
		base.SetState(state);
		switch (state)
		{
		case State.Normal:
			_text.color = base.Menu.Color;
			_icon.color = base.Menu.Color;
			_cooltime.color = Color.white;
			break;
		case State.Pressed:
			_text.color = PresetColor.UIYellow;
			_icon.color = PresetColor.UIYellow;
			_cooltime.color = PresetColor.UIYellow;
			break;
		}
	}
}
