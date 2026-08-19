using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Drag Scroll View")]
public class UIDragScrollView : MonoBehaviour
{
	public UIScrollView scrollView;

	[SerializeField]
	[HideInInspector]
	private UIScrollView draggablePanel;

	private Transform mTrans;

	private UIScrollView mScroll;

	private bool mAutoFind;

	private bool mStarted;

	private void OnEnable()
	{
		mTrans = ((Component)this).transform;
		if ((Object)(object)scrollView == (Object)null && (Object)(object)draggablePanel != (Object)null)
		{
			scrollView = draggablePanel;
			draggablePanel = null;
		}
		if (mStarted && (mAutoFind || (Object)(object)mScroll == (Object)null))
		{
			FindScrollView();
		}
	}

	private void Start()
	{
		mStarted = true;
		FindScrollView();
	}

	private void FindScrollView()
	{
		UIScrollView uIScrollView = NGUITools.FindInParents<UIScrollView>(mTrans);
		if ((Object)(object)scrollView == (Object)null || (mAutoFind && (Object)(object)uIScrollView != (Object)(object)scrollView))
		{
			scrollView = uIScrollView;
			mAutoFind = true;
		}
		else if ((Object)(object)scrollView == (Object)(object)uIScrollView)
		{
			mAutoFind = true;
		}
		mScroll = scrollView;
	}

	private void OnDisable()
	{
		if ((Object)(object)mScroll != (Object)null && (Object)(object)((Component)mScroll).GetComponentInChildren<UIWrapContent>() == (Object)null)
		{
			mScroll.Press(pressed: false);
			mScroll = null;
		}
	}

	private void OnPress(bool pressed)
	{
		if (mAutoFind && (Object)(object)mScroll != (Object)(object)scrollView)
		{
			mScroll = scrollView;
			mAutoFind = false;
		}
		if (Object.op_Implicit((Object)(object)scrollView) && ((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject))
		{
			scrollView.Press(pressed);
			if (!pressed && mAutoFind)
			{
				scrollView = NGUITools.FindInParents<UIScrollView>(mTrans);
				mScroll = scrollView;
			}
		}
	}

	private void OnDrag(Vector2 delta)
	{
		if (Object.op_Implicit((Object)(object)scrollView) && NGUITools.GetActive((Behaviour)(object)this))
		{
			scrollView.Drag();
		}
	}

	private void OnScroll(float delta)
	{
		if (Object.op_Implicit((Object)(object)scrollView) && NGUITools.GetActive((Behaviour)(object)this))
		{
			scrollView.Scroll(delta);
		}
	}

	public void OnPan(Vector2 delta)
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)scrollView) && NGUITools.GetActive((Behaviour)(object)this))
		{
			scrollView.OnPan(delta);
		}
	}
}
