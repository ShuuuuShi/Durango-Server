using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class SelectableWidget : Selectable
{
	[HideInInspector]
	[SerializeField]
	protected ButtonStates _states;

	private Color _tint = Color.white;

	public bool IsHoldWidgetState { get; private set; }

	protected override void OnInit()
	{
	}

	protected override void OnRefresh(State state)
	{
		SetWidgetState(state);
	}

	protected void SetWidgetState(State state, bool ignoreHoldState = false)
	{
		if (!IsHoldWidgetState || ignoreHoldState)
		{
			for (int i = 0; i < _states.Count; i++)
			{
				ButtonState buttonState = _states[i];
				buttonState.Tint = _tint;
				buttonState.SetState(state);
			}
		}
	}

	public void SetTint(Color color)
	{
		_tint = color;
		Refresh();
	}

	public void HoldWidgetState(bool isHold)
	{
		if (IsHoldWidgetState != isHold)
		{
			IsHoldWidgetState = isHold;
			if (!isHold)
			{
				Refresh();
			}
		}
	}

	public void HoldWidgetState(bool isHold, State state)
	{
		if (isHold)
		{
			SetWidgetState(state, ignoreHoldState: true);
			IsHoldWidgetState = true;
		}
		else
		{
			HoldWidgetState(isHold: false);
		}
	}

	[UsedImplicitly]
	protected virtual void OnPress(bool isPress)
	{
		base.Pressed = isPress;
	}
}
