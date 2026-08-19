using Durango.UI.Popup;
using UnityEngine;

namespace Durango.UI;

public class ContextActionGroup_PC : ContextActionGroupBase
{
	[SerializeField]
	private GameObject _tooltipParent;

	protected override void Start()
	{
		base.Start();
		_actionButtons.MenuHovered += OnMenuHover;
	}

	private void OnMenuHover(ContextActionButtonBase button, bool show)
	{
		if (show)
		{
			if (!string.IsNullOrEmpty(button.Description))
			{
				ActionTooltipBase actionTooltipBase = UIManager.Popup.Tooltip<ActionTooltipBase>();
				actionTooltipBase.Set(button);
				actionTooltipBase.Direction = TooltipBase.TooltipDirection.Vertical;
				actionTooltipBase.Sign = 1;
				actionTooltipBase.Show(_tooltipParent, Vector2.zero);
			}
		}
		else
		{
			ActionTooltipBase actionTooltipBase2 = UIManager.Popup.FindTooltip<ActionTooltipBase>();
			actionTooltipBase2.Hide();
		}
	}
}
