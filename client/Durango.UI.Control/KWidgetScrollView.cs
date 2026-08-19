using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI.Control;

public class KWidgetScrollView : KScrollViewBase
{
	[SerializeField]
	private List<UIWidget> _widgets;

	public List<UIWidget> Widgets
	{
		get
		{
			return _widgets;
		}
		set
		{
			_widgets = value;
		}
	}

	public override UIWidget GetNode(int index)
	{
		return _widgets[index];
	}

	public override int GetNodeCount()
	{
		return _widgets.Count;
	}

	protected override float OnUpdateLayout(bool instant)
	{
		Vector3 basePosition = GetBasePosition();
		return UIUtility.WidgetsReposition(_widgets, base.Vector, basePosition, base.Margin, 0f, instant);
	}
}
