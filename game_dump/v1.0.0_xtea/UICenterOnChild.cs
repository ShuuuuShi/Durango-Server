using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/Interaction/Center Scroll View on Child")]
public class UICenterOnChild : MonoBehaviour
{
	public delegate void OnCenterCallback(GameObject centeredObject);

	public float springStrength = 8f;

	public float nextPageThreshold;

	public SpringPanel.OnFinished onFinished;

	public OnCenterCallback onCenter;

	private UIScrollView mScrollView;

	private GameObject mCenteredObject;

	public GameObject centeredObject => mCenteredObject;

	private void Start()
	{
		Recenter();
	}

	private void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)mScrollView))
		{
			mScrollView.centerOnChild = this;
			Recenter();
		}
	}

	private void OnDisable()
	{
		if (Object.op_Implicit((Object)(object)mScrollView))
		{
			mScrollView.centerOnChild = null;
		}
	}

	private void OnDragFinished()
	{
		if (((Behaviour)this).enabled)
		{
			Recenter();
		}
	}

	private void OnValidate()
	{
		nextPageThreshold = Mathf.Abs(nextPageThreshold);
	}

	[ContextMenu("Execute")]
	public void Recenter()
	{
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_051c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mScrollView == (Object)null)
		{
			mScrollView = NGUITools.FindInParents<UIScrollView>(((Component)this).gameObject);
			if ((Object)(object)mScrollView == (Object)null)
			{
				((Behaviour)this).enabled = false;
				return;
			}
			if (Object.op_Implicit((Object)(object)mScrollView))
			{
				mScrollView.centerOnChild = this;
				UIScrollView uIScrollView = mScrollView;
				uIScrollView.onDragFinished = (UIScrollView.OnDragNotification)Delegate.Combine(uIScrollView.onDragFinished, new UIScrollView.OnDragNotification(OnDragFinished));
			}
			if ((Object)(object)mScrollView.horizontalScrollBar != (Object)null)
			{
				UIProgressBar horizontalScrollBar = mScrollView.horizontalScrollBar;
				horizontalScrollBar.onDragFinished = (UIProgressBar.OnDragFinished)Delegate.Combine(horizontalScrollBar.onDragFinished, new UIProgressBar.OnDragFinished(OnDragFinished));
			}
			if ((Object)(object)mScrollView.verticalScrollBar != (Object)null)
			{
				UIProgressBar verticalScrollBar = mScrollView.verticalScrollBar;
				verticalScrollBar.onDragFinished = (UIProgressBar.OnDragFinished)Delegate.Combine(verticalScrollBar.onDragFinished, new UIProgressBar.OnDragFinished(OnDragFinished));
			}
		}
		if ((Object)(object)mScrollView.panel == (Object)null)
		{
			return;
		}
		Transform transform = ((Component)this).transform;
		if (transform.childCount == 0)
		{
			return;
		}
		Vector3[] worldCorners = mScrollView.panel.worldCorners;
		Vector3 val = (worldCorners[2] + worldCorners[0]) * 0.5f;
		Vector3 velocity = mScrollView.currentMomentum * mScrollView.momentumAmount;
		Vector3 val2 = NGUIMath.SpringDampen(ref velocity, 9f, 2f);
		Vector3 val3 = val - val2 * 0.01f;
		float num = float.MaxValue;
		Transform target = null;
		int num2 = 0;
		int num3 = 0;
		UIGrid component = ((Component)this).GetComponent<UIGrid>();
		List<Transform> list = null;
		if ((Object)(object)component != (Object)null)
		{
			list = component.GetChildList();
			int i = 0;
			int count = list.Count;
			int num4 = 0;
			for (; i < count; i++)
			{
				Transform val4 = list[i];
				if (((Component)val4).gameObject.activeInHierarchy)
				{
					float num5 = Vector3.SqrMagnitude(val4.position - val3);
					if (num5 < num)
					{
						num = num5;
						target = val4;
						num2 = i;
						num3 = num4;
					}
					num4++;
				}
			}
		}
		else
		{
			int j = 0;
			int childCount = transform.childCount;
			int num6 = 0;
			for (; j < childCount; j++)
			{
				Transform child = transform.GetChild(j);
				if (((Component)child).gameObject.activeInHierarchy)
				{
					float num7 = Vector3.SqrMagnitude(child.position - val3);
					if (num7 < num)
					{
						num = num7;
						target = child;
						num2 = j;
						num3 = num6;
					}
					num6++;
				}
			}
		}
		if (nextPageThreshold > 0f && UICamera.currentTouch != null && (Object)(object)mCenteredObject != (Object)null && (Object)(object)mCenteredObject.transform == (Object)(object)((list == null) ? transform.GetChild(num2) : list[num2]))
		{
			Vector3 val5 = Vector2.op_Implicit(UICamera.currentTouch.totalDelta);
			val5 = ((Component)this).transform.rotation * val5;
			float num8 = 0f;
			num8 = mScrollView.movement switch
			{
				UIScrollView.Movement.Horizontal => val5.x, 
				UIScrollView.Movement.Vertical => val5.y, 
				_ => ((Vector3)(ref val5)).magnitude, 
			};
			if (Mathf.Abs(num8) > nextPageThreshold)
			{
				if (num8 > nextPageThreshold)
				{
					target = ((list != null) ? ((num3 <= 0) ? ((!((Object)(object)((Component)this).GetComponent<UIWrapContent>() == (Object)null)) ? list[list.Count - 1] : list[0]) : list[num3 - 1]) : ((num3 <= 0) ? ((!((Object)(object)((Component)this).GetComponent<UIWrapContent>() == (Object)null)) ? transform.GetChild(transform.childCount - 1) : transform.GetChild(0)) : transform.GetChild(num3 - 1)));
				}
				else if (num8 < 0f - nextPageThreshold)
				{
					target = ((list != null) ? ((num3 >= list.Count - 1) ? ((!((Object)(object)((Component)this).GetComponent<UIWrapContent>() == (Object)null)) ? list[0] : list[list.Count - 1]) : list[num3 + 1]) : ((num3 >= transform.childCount - 1) ? ((!((Object)(object)((Component)this).GetComponent<UIWrapContent>() == (Object)null)) ? transform.GetChild(0) : transform.GetChild(transform.childCount - 1)) : transform.GetChild(num3 + 1)));
				}
			}
		}
		CenterOn(target, val);
	}

	private void CenterOn(Transform target, Vector3 panelCenter)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)target != (Object)null && (Object)(object)mScrollView != (Object)null && (Object)(object)mScrollView.panel != (Object)null)
		{
			Transform cachedTransform = mScrollView.panel.cachedTransform;
			mCenteredObject = ((Component)target).gameObject;
			Vector3 val = cachedTransform.InverseTransformPoint(target.position);
			Vector3 val2 = cachedTransform.InverseTransformPoint(panelCenter);
			Vector3 val3 = val - val2;
			if (!mScrollView.canMoveHorizontally)
			{
				val3.x = 0f;
			}
			if (!mScrollView.canMoveVertically)
			{
				val3.y = 0f;
			}
			val3.z = 0f;
			SpringPanel.Begin(mScrollView.panel.cachedGameObject, cachedTransform.localPosition - val3, springStrength).onFinished = onFinished;
		}
		else
		{
			mCenteredObject = null;
		}
		if (onCenter != null)
		{
			onCenter(mCenteredObject);
		}
	}

	public void CenterOn(Transform target)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mScrollView != (Object)null && (Object)(object)mScrollView.panel != (Object)null)
		{
			Vector3[] worldCorners = mScrollView.panel.worldCorners;
			Vector3 panelCenter = (worldCorners[2] + worldCorners[0]) * 0.5f;
			CenterOn(target, panelCenter);
		}
	}
}
