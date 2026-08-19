using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class FriendListColumnHeaderWidget : SortableColumnWidget<string>
{
	protected override void GetStateColor(out Color normal, out Color selected)
	{
		normal = new Color32(183, 178, 167, byte.MaxValue);
		selected = PresetColor.UIYellow;
	}
}
