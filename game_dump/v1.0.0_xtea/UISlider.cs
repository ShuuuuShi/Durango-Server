using System;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/NGUI Slider")]
[ExecuteInEditMode]
public class UISlider : UIProgressBar
{
	private enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded
	}

	[SerializeField]
	[HideInInspector]
	private Transform foreground;

	[HideInInspector]
	[SerializeField]
	private float rawValue = 1f;

	[SerializeField]
	[HideInInspector]
	private Direction direction = Direction.Upgraded;

	[SerializeField]
	[HideInInspector]
	protected bool mInverted;

	public bool isColliderEnabled
	{
		get
		{
			Collider component = ((Component)this).GetComponent<Collider>();
			if ((Object)(object)component != (Object)null)
			{
				return component.enabled;
			}
			Collider2D component2 = ((Component)this).GetComponent<Collider2D>();
			return (Object)(object)component2 != (Object)null && ((Behaviour)component2).enabled;
		}
	}

	[Obsolete("Use 'value' instead")]
	public float sliderValue
	{
		get
		{
			return base.value;
		}
		set
		{
			base.value = value;
		}
	}

	[Obsolete("Use 'fillDirection' instead")]
	public bool inverted
	{
		get
		{
			return base.isInverted;
		}
		set
		{
		}
	}

	protected override void Upgrade()
	{
		if (direction != Direction.Upgraded)
		{
			mValue = rawValue;
			if ((Object)(object)foreground != (Object)null)
			{
				mFG = ((Component)foreground).GetComponent<UIWidget>();
			}
			if (direction == Direction.Horizontal)
			{
				mFill = (mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight);
			}
			else
			{
				mFill = ((!mInverted) ? FillDirection.BottomToTop : FillDirection.TopToBottom);
			}
			direction = Direction.Upgraded;
		}
	}

	protected override void OnStart()
	{
		GameObject go = ((!((Object)(object)mBG != (Object)null) || (!((Object)(object)((Component)mBG).GetComponent<Collider>() != (Object)null) && !((Object)(object)((Component)mBG).GetComponent<Collider2D>() != (Object)null))) ? ((Component)this).gameObject : ((Component)mBG).gameObject);
		UIEventListener uIEventListener = UIEventListener.Get(go);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnPressBackground));
		uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(OnDragBackground));
		if ((Object)(object)thumb != (Object)null && ((Object)(object)((Component)thumb).GetComponent<Collider>() != (Object)null || (Object)(object)((Component)thumb).GetComponent<Collider2D>() != (Object)null) && ((Object)(object)mFG == (Object)null || (Object)(object)thumb != (Object)(object)mFG.cachedTransform))
		{
			UIEventListener uIEventListener2 = UIEventListener.Get(((Component)thumb).gameObject);
			uIEventListener2.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener2.onPress, new UIEventListener.BoolDelegate(OnPressForeground));
			uIEventListener2.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener2.onDrag, new UIEventListener.VectorDelegate(OnDragForeground));
		}
	}

	protected void OnPressBackground(GameObject go, bool isPressed)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			base.value = ScreenToValue(UICamera.lastEventPosition);
			if (!isPressed && onDragFinished != null)
			{
				onDragFinished();
			}
		}
	}

	protected void OnDragBackground(GameObject go, Vector2 delta)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			base.value = ScreenToValue(UICamera.lastEventPosition);
		}
	}

	protected void OnPressForeground(GameObject go, bool isPressed)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			if (isPressed)
			{
				mOffset = ((!((Object)(object)mFG == (Object)null)) ? (base.value - ScreenToValue(UICamera.lastEventPosition)) : 0f);
			}
			else if (onDragFinished != null)
			{
				onDragFinished();
			}
		}
	}

	protected void OnDragForeground(GameObject go, Vector2 delta)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if (UICamera.currentScheme != UICamera.ControlScheme.Controller)
		{
			mCam = UICamera.currentCamera;
			base.value = mOffset + ScreenToValue(UICamera.lastEventPosition);
		}
	}

	public override void OnPan(Vector2 delta)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (((Behaviour)this).enabled && isColliderEnabled)
		{
			base.OnPan(delta);
		}
	}
}
