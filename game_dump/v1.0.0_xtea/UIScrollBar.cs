using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/Interaction/NGUI Scroll Bar")]
public class UIScrollBar : UISlider
{
	private enum Direction
	{
		Horizontal,
		Vertical,
		Upgraded
	}

	[SerializeField]
	[HideInInspector]
	protected float mSize = 1f;

	[HideInInspector]
	[SerializeField]
	private float mScroll;

	[HideInInspector]
	[SerializeField]
	private Direction mDir = Direction.Upgraded;

	[Obsolete("Use 'value' instead")]
	public float scrollValue
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

	public float barSize
	{
		get
		{
			return mSize;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mSize == num)
			{
				return;
			}
			mSize = num;
			mIsDirty = true;
			if (NGUITools.GetActive((Behaviour)(object)this))
			{
				if ((Object)(object)UIProgressBar.current == (Object)null && onChange != null)
				{
					UIProgressBar.current = this;
					EventDelegate.Execute(onChange);
					UIProgressBar.current = null;
				}
				ForceUpdate();
			}
		}
	}

	protected override void Upgrade()
	{
		if (mDir != Direction.Upgraded)
		{
			mValue = mScroll;
			if (mDir == Direction.Horizontal)
			{
				mFill = (mInverted ? FillDirection.RightToLeft : FillDirection.LeftToRight);
			}
			else
			{
				mFill = ((!mInverted) ? FillDirection.TopToBottom : FillDirection.BottomToTop);
			}
			mDir = Direction.Upgraded;
		}
	}

	protected override void OnStart()
	{
		base.OnStart();
		if ((Object)(object)mFG != (Object)null && (Object)(object)((Component)mFG).gameObject != (Object)(object)((Component)this).gameObject && ((Object)(object)((Component)mFG).GetComponent<Collider>() != (Object)null || (Object)(object)((Component)mFG).GetComponent<Collider2D>() != (Object)null))
		{
			UIEventListener uIEventListener = UIEventListener.Get(((Component)mFG).gameObject);
			uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(base.OnPressForeground));
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(base.OnDragForeground));
			mFG.autoResizeBoxCollider = true;
		}
	}

	protected override float LocalToValue(Vector2 localPos)
	{
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mFG != (Object)null)
		{
			float num = Mathf.Clamp01(mSize) * 0.5f;
			float num2 = num;
			float num3 = 1f - num;
			Vector3[] localCorners = mFG.localCorners;
			if (base.isHorizontal)
			{
				num2 = Mathf.Lerp(localCorners[0].x, localCorners[2].x, num2);
				num3 = Mathf.Lerp(localCorners[0].x, localCorners[2].x, num3);
				float num4 = num3 - num2;
				if (num4 == 0f)
				{
					return base.value;
				}
				return (!base.isInverted) ? ((localPos.x - num2) / num4) : ((num3 - localPos.x) / num4);
			}
			num2 = Mathf.Lerp(localCorners[0].y, localCorners[1].y, num2);
			num3 = Mathf.Lerp(localCorners[3].y, localCorners[2].y, num3);
			float num5 = num3 - num2;
			if (num5 == 0f)
			{
				return base.value;
			}
			return (!base.isInverted) ? ((localPos.y - num2) / num5) : ((num3 - localPos.y) / num5);
		}
		return base.LocalToValue(localPos);
	}

	public override void ForceUpdate()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mFG != (Object)null)
		{
			mIsDirty = false;
			float num = Mathf.Clamp01(mSize) * 0.5f;
			float num2 = Mathf.Lerp(num, 1f - num, base.value);
			float num3 = num2 - num;
			float num4 = num2 + num;
			if (base.isHorizontal)
			{
				mFG.drawRegion = ((!base.isInverted) ? new Vector4(num3, 0f, num4, 1f) : new Vector4(1f - num4, 0f, 1f - num3, 1f));
			}
			else
			{
				mFG.drawRegion = ((!base.isInverted) ? new Vector4(0f, num3, 1f, num4) : new Vector4(0f, 1f - num4, 1f, 1f - num3));
			}
			if ((Object)(object)thumb != (Object)null)
			{
				Vector4 drawingDimensions = mFG.drawingDimensions;
				Vector3 val = default(Vector3);
				((Vector3)(ref val))._002Ector(Mathf.Lerp(drawingDimensions.x, drawingDimensions.z, 0.5f), Mathf.Lerp(drawingDimensions.y, drawingDimensions.w, 0.5f));
				SetThumbPosition(mFG.cachedTransform.TransformPoint(val));
			}
		}
		else
		{
			base.ForceUpdate();
		}
	}
}
