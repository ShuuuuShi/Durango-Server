using UnityEngine;

namespace Durango.UI;

public class BottomLeftMenuGroupBase : UIBase
{
	[SerializeField]
	protected BottomMenuWidgetBase _bottomMenuWidget;

	public BottomMenuWidgetBase BottomMenuWidget => _bottomMenuWidget;

	protected virtual void Start()
	{
		base.VisibleController.Changed += OnVisibleChanged;
	}

	private static void OnVisibleChanged(bool visible)
	{
		GameSystem<InputSystem>.Instance().DrawMode = false;
	}
}
