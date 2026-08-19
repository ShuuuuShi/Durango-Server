using UnityEngine;

namespace Durango.UI;

public class ClanMenuPage : MonoBehaviour
{
	[SerializeField]
	private RectLayout _layout;

	private UIWidget _widget;

	private Point2 _size;

	protected RectLayout Layout => _layout;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				return _widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public virtual bool Back()
	{
		return true;
	}

	protected virtual void OnEnable()
	{
		Point2 point = new Point2(Widget.localSize);
		if (!(point == _size))
		{
			_size = point;
			UpdateLayout();
		}
	}

	protected virtual void UpdateLayout()
	{
		_layout.UpdateLayout(_size.x, _size.y);
		UIUtility.UpdateAnchors(base.transform);
	}
}
