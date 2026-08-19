using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueInteractionButtonGroup : PrologueInteractionButtonGroupBase
{
	[SerializeField]
	private PrologueInteractionButtonControl _buttonControl;

	protected override void Start()
	{
		base.Start();
		_buttonControl.InteractionClicked += base.OnTouchInteractionObject;
		Singleton<PlayerController>.Instance().MoveStarted += OnStartMove;
		Singleton<PlayerController>.Instance().MoveEnded += OnEndMove;
	}

	private void OnStartMove()
	{
		PrologueInteractionButtonGroupBase.ShowInteractionButton("Moving", show: false);
	}

	private void OnEndMove()
	{
		PrologueInteractionButtonGroupBase.RefreshInteractions();
		PrologueInteractionButtonGroupBase.ShowInteractionButton("Moving", show: true);
	}

	protected override void OnTargetSelected(InteractionObject obj)
	{
		base.OnTargetSelected(obj);
		_buttonControl.UnselectAnimation();
		_buttonControl.SelectAnimation(obj);
	}

	protected override void SetInteractionButtons(IList<InteractionObject> list)
	{
		_buttonControl.SetInteractionButtons(list);
	}
}
