using UnityEngine;

public class KWidgetScrollView : KScrollViewBase
{
	[SerializeField]
	private UIWidget[] _widgets;

	public UIWidget[] Widgets => _widgets;

	public override UIWidget GetNode(int index)
	{
		return _widgets[index];
	}

	public override int GetNodeCount()
	{
		return _widgets.Length;
	}
}
