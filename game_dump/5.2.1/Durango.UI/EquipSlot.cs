using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class EquipSlot : EquipSlotBase
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private Color _equipBgColor;

	protected override void OnRefresh(State state)
	{
		base.OnRefresh(state);
		_background.color = ((base.Item != null) ? _equipBgColor : Color.clear);
		base.Widget.alpha = ((!base.Disabled) ? 1f : 0.5f);
	}
}
