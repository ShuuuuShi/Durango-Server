using System.Collections.Generic;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueInteractionButtonGroup_PC : PrologueInteractionButtonGroupBase
{
	[SerializeField]
	private PrologueInteractionButtonControl_PC _buttonControl;

	protected override void Start()
	{
		base.Start();
		_buttonControl.InteractionClicked += base.OnTouchInteractionObject;
		Singleton<PlayerController>.Instance().MoveEnded += OnEndMove;
	}

	private void OnEndMove()
	{
		PrologueInteractionButtonGroupBase.RefreshInteractions();
	}

	protected override void SetInteractionButtons(IList<InteractionObject> list)
	{
		_buttonControl.SetInteractionButtons(list);
	}
}
