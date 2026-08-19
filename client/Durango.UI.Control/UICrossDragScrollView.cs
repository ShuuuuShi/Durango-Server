using UnityEngine;

namespace Durango.UI.Control;

public class UICrossDragScrollView : MonoBehaviour
{
	public UIScrollView hScrollView;

	public UIScrollView vScrollView;

	private bool mAutoFind;

	private bool mStarted;

	private bool mMovableH;

	private bool mMovableV;

	private void OnPress(bool pressed)
	{
		if (hScrollView != null)
		{
			hScrollView.Press(pressed);
			mMovableH = pressed;
		}
		if (vScrollView != null)
		{
			vScrollView.Press(pressed);
			mMovableV = pressed;
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (mMovableV && mMovableH)
		{
			if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
			{
				mMovableV = false;
			}
			else
			{
				mMovableH = false;
			}
		}
		if (mMovableH)
		{
			hScrollView.Drag();
		}
		if (mMovableV)
		{
			vScrollView.Drag();
		}
	}
}
