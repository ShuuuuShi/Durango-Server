using UnityEngine;

namespace Durango.UI;

public class ActionTooltip : ActionTooltipBase
{
	public override bool DragLock => true;

	protected override void UpdateLayout()
	{
		Vector2 preferredSize = _titleLabel.GetPreferredSize(400);
		_descriptionLabel.width = 360;
		Vector2 printedSize = _descriptionLabel.printedSize;
		float a = Mathf.Max(preferredSize.x, printedSize.x + 40f);
		a = Mathf.Min(a, 400f);
		int num = Mathf.CeilToInt(a);
		_titleLabel.UpdateLayout(num);
		_descriptionLabel.width = num - 40;
		_layout.UpdateLayout(num, 0f);
		UIUtility.UpdateAnchors(base.transform);
	}
}
