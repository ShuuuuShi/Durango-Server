using UnityEngine;

namespace Durango.UI;

public class MinimapGroup : MinimapGroupBase
{
	[SerializeField]
	private Transform _container;

	protected override void Start()
	{
		base.Start();
		UIManager.FindScript<ToDoListGroupBase>().AddWidthOnChanged(ToDoListGroup_WidthRatioChanged);
	}

	private void ToDoListGroup_WidthRatioChanged(float ratio)
	{
		Vector3 localPosition = GetComponent<UIRect>().localCorners[2];
		localPosition.x += (ratio - 1f) * (float)ToDoListGroupBase.Width;
		_container.localPosition = localPosition;
	}
}
