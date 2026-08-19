using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("2D Toolkit/UI/tk2dUIScrollableArea")]
public class tk2dUIScrollableArea : MonoBehaviour
{
	public enum Axes
	{
		XAxis,
		YAxis
	}

	private const float SWIPE_SCROLLING_FIRST_SCROLL_THRESHOLD = 0.02f;

	private const float WITHOUT_SCROLLBAR_FIXED_SCROLL_WHEEL_PERCENT = 0.1f;

	[SerializeField]
	private float contentLength = 1f;

	[SerializeField]
	private float visibleAreaLength = 1f;

	public GameObject contentContainer;

	public tk2dUIScrollbar scrollBar;

	public tk2dUIItem backgroundUIItem;

	public Axes scrollAxes = Axes.YAxis;

	public bool allowSwipeScrolling = true;

	public bool allowScrollWheel = true;

	[SerializeField]
	[HideInInspector]
	private tk2dUILayout backgroundLayoutItem;

	[SerializeField]
	[HideInInspector]
	private tk2dUILayoutContainer contentLayoutContainer;

	private bool isBackgroundButtonDown;

	private bool isBackgroundButtonOver;

	private Vector3 swipeScrollingPressDownStartLocalPos = Vector3.zero;

	private Vector3 swipeScrollingContentStartLocalPos = Vector3.zero;

	private Vector3 swipeScrollingContentDestLocalPos = Vector3.zero;

	private bool isSwipeScrollingInProgress;

	private Vector3 swipePrevScrollingContentPressLocalPos = Vector3.zero;

	private float swipeCurrVelocity;

	private float snapBackVelocity;

	public string SendMessageOnScrollMethodName = string.Empty;

	private float percent;

	private static readonly Vector3[] boxExtents = (Vector3[])(object)new Vector3[8]
	{
		new Vector3(-1f, -1f, -1f),
		new Vector3(1f, -1f, -1f),
		new Vector3(-1f, 1f, -1f),
		new Vector3(1f, 1f, -1f),
		new Vector3(-1f, -1f, 1f),
		new Vector3(1f, -1f, 1f),
		new Vector3(-1f, 1f, 1f),
		new Vector3(1f, 1f, 1f)
	};

	public float ContentLength
	{
		get
		{
			return contentLength;
		}
		set
		{
			ContentLengthVisibleAreaLengthChange(contentLength, value, visibleAreaLength, visibleAreaLength);
		}
	}

	public float VisibleAreaLength
	{
		get
		{
			return visibleAreaLength;
		}
		set
		{
			ContentLengthVisibleAreaLengthChange(contentLength, contentLength, visibleAreaLength, value);
		}
	}

	public tk2dUILayout BackgroundLayoutItem
	{
		get
		{
			return backgroundLayoutItem;
		}
		set
		{
			if ((Object)(object)backgroundLayoutItem != (Object)(object)value)
			{
				if ((Object)(object)backgroundLayoutItem != (Object)null)
				{
					backgroundLayoutItem.OnReshape -= LayoutReshaped;
				}
				backgroundLayoutItem = value;
				if ((Object)(object)backgroundLayoutItem != (Object)null)
				{
					backgroundLayoutItem.OnReshape += LayoutReshaped;
				}
			}
		}
	}

	public tk2dUILayoutContainer ContentLayoutContainer
	{
		get
		{
			return contentLayoutContainer;
		}
		set
		{
			if ((Object)(object)contentLayoutContainer != (Object)(object)value)
			{
				if ((Object)(object)contentLayoutContainer != (Object)null)
				{
					contentLayoutContainer.OnChangeContent -= ContentLayoutChangeCallback;
				}
				contentLayoutContainer = value;
				if ((Object)(object)contentLayoutContainer != (Object)null)
				{
					contentLayoutContainer.OnChangeContent += ContentLayoutChangeCallback;
				}
			}
		}
	}

	public GameObject SendMessageTarget
	{
		get
		{
			if ((Object)(object)backgroundUIItem != (Object)null)
			{
				return backgroundUIItem.sendMessageTarget;
			}
			return null;
		}
		set
		{
			if ((Object)(object)backgroundUIItem != (Object)null && (Object)(object)backgroundUIItem.sendMessageTarget != (Object)(object)value)
			{
				backgroundUIItem.sendMessageTarget = value;
			}
		}
	}

	public float Value
	{
		get
		{
			return Mathf.Clamp01(percent);
		}
		set
		{
			value = Mathf.Clamp(value, 0f, 1f);
			if (value != percent)
			{
				UnpressAllUIItemChildren();
				percent = value;
				if (this.OnScroll != null)
				{
					this.OnScroll(this);
				}
				if (isBackgroundButtonDown || isSwipeScrollingInProgress)
				{
					if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
					{
						tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
					}
					isBackgroundButtonDown = false;
					isSwipeScrollingInProgress = false;
				}
				TargetOnScrollCallback();
			}
			if ((Object)(object)scrollBar != (Object)null)
			{
				scrollBar.SetScrollPercentWithoutEvent(percent);
			}
			SetContentPosition();
		}
	}

	private Vector3 ContentContainerOffset
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.Scale(new Vector3(-1f, 1f, 1f), contentContainer.transform.localPosition);
		}
		set
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			contentContainer.transform.localPosition = Vector3.Scale(new Vector3(-1f, 1f, 1f), value);
		}
	}

	public event Action<tk2dUIScrollableArea> OnScroll;

	public void SetScrollPercentWithoutEvent(float newScrollPercent)
	{
		percent = Mathf.Clamp(newScrollPercent, 0f, 1f);
		UnpressAllUIItemChildren();
		if ((Object)(object)scrollBar != (Object)null)
		{
			scrollBar.SetScrollPercentWithoutEvent(percent);
		}
		SetContentPosition();
	}

	public float MeasureContentLength()
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector(float.MinValue, float.MinValue, float.MinValue);
		Vector3 val2 = default(Vector3);
		((Vector3)(ref val2))._002Ector(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3[] array = (Vector3[])(object)new Vector3[2] { val2, val };
		Transform transform = contentContainer.transform;
		GetRendererBoundsInChildren(transform.worldToLocalMatrix, array, transform);
		if (array[0] != val2 && array[1] != val)
		{
			ref Vector3 reference = ref array[0];
			reference = Vector3.Min(array[0], Vector3.zero);
			ref Vector3 reference2 = ref array[1];
			reference2 = Vector3.Max(array[1], Vector3.zero);
			return (scrollAxes != Axes.YAxis) ? (array[1].x - array[0].x) : (array[1].y - array[0].y);
		}
		Debug.LogError((object)"Unable to measure content length");
		return VisibleAreaLength * 0.9f;
	}

	private void OnEnable()
	{
		if ((Object)(object)scrollBar != (Object)null)
		{
			scrollBar.OnScroll += ScrollBarMove;
		}
		if ((Object)(object)backgroundUIItem != (Object)null)
		{
			backgroundUIItem.OnDown += BackgroundButtonDown;
			backgroundUIItem.OnRelease += BackgroundButtonRelease;
			backgroundUIItem.OnHoverOver += BackgroundButtonHoverOver;
			backgroundUIItem.OnHoverOut += BackgroundButtonHoverOut;
		}
		if ((Object)(object)backgroundLayoutItem != (Object)null)
		{
			backgroundLayoutItem.OnReshape += LayoutReshaped;
		}
		if ((Object)(object)contentLayoutContainer != (Object)null)
		{
			contentLayoutContainer.OnChangeContent += ContentLayoutChangeCallback;
		}
	}

	private void OnDisable()
	{
		if ((Object)(object)scrollBar != (Object)null)
		{
			scrollBar.OnScroll -= ScrollBarMove;
		}
		if ((Object)(object)backgroundUIItem != (Object)null)
		{
			backgroundUIItem.OnDown -= BackgroundButtonDown;
			backgroundUIItem.OnRelease -= BackgroundButtonRelease;
			backgroundUIItem.OnHoverOver -= BackgroundButtonHoverOver;
			backgroundUIItem.OnHoverOut -= BackgroundButtonHoverOut;
		}
		if (isBackgroundButtonOver)
		{
			if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
			{
				tk2dUIManager.Instance.OnScrollWheelChange -= BackgroundHoverOverScrollWheelChange;
			}
			isBackgroundButtonOver = false;
		}
		if (isBackgroundButtonDown || isSwipeScrollingInProgress)
		{
			if ((Object)(object)tk2dUIManager.Instance__NoCreate != (Object)null)
			{
				tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
			}
			isBackgroundButtonDown = false;
			isSwipeScrollingInProgress = false;
		}
		if ((Object)(object)backgroundLayoutItem != (Object)null)
		{
			backgroundLayoutItem.OnReshape -= LayoutReshaped;
		}
		if ((Object)(object)contentLayoutContainer != (Object)null)
		{
			contentLayoutContainer.OnChangeContent -= ContentLayoutChangeCallback;
		}
		swipeCurrVelocity = 0f;
	}

	private void Start()
	{
		UpdateScrollbarActiveState();
	}

	private void BackgroundHoverOverScrollWheelChange(float mouseWheelChange)
	{
		if (mouseWheelChange > 0f)
		{
			if (Object.op_Implicit((Object)(object)scrollBar))
			{
				scrollBar.ScrollUpFixed();
			}
			else
			{
				Value -= 0.1f;
			}
		}
		else if (mouseWheelChange < 0f)
		{
			if (Object.op_Implicit((Object)(object)scrollBar))
			{
				scrollBar.ScrollDownFixed();
			}
			else
			{
				Value += 0.1f;
			}
		}
	}

	private void ScrollBarMove(tk2dUIScrollbar scrollBar)
	{
		Value = scrollBar.Value;
		isSwipeScrollingInProgress = false;
		if (isBackgroundButtonDown)
		{
			BackgroundButtonRelease();
		}
	}

	private void SetContentPosition()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		Vector3 contentContainerOffset = ContentContainerOffset;
		float num = (contentLength - visibleAreaLength) * Value;
		if (num < 0f)
		{
			num = 0f;
		}
		if (scrollAxes == Axes.XAxis)
		{
			contentContainerOffset.x = num;
		}
		else if (scrollAxes == Axes.YAxis)
		{
			contentContainerOffset.y = num;
		}
		ContentContainerOffset = contentContainerOffset;
	}

	private void BackgroundButtonDown()
	{
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (allowSwipeScrolling && contentLength > visibleAreaLength)
		{
			if (!isBackgroundButtonDown && !isSwipeScrollingInProgress)
			{
				tk2dUIManager.Instance.OnInputUpdate += BackgroundOverUpdate;
			}
			swipeScrollingPressDownStartLocalPos = ((Component)this).transform.InverseTransformPoint(CalculateClickWorldPos(backgroundUIItem));
			swipePrevScrollingContentPressLocalPos = swipeScrollingPressDownStartLocalPos;
			swipeScrollingContentStartLocalPos = ContentContainerOffset;
			swipeScrollingContentDestLocalPos = swipeScrollingContentStartLocalPos;
			isBackgroundButtonDown = true;
			swipeCurrVelocity = 0f;
		}
	}

	private void BackgroundOverUpdate()
	{
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		if (isBackgroundButtonDown)
		{
			UpdateSwipeScrollDestintationPosition();
		}
		if (!isSwipeScrollingInProgress)
		{
			return;
		}
		float num = percent;
		float num2 = 0f;
		if (scrollAxes == Axes.XAxis)
		{
			num2 = swipeScrollingContentDestLocalPos.x;
		}
		else if (scrollAxes == Axes.YAxis)
		{
			num2 = swipeScrollingContentDestLocalPos.y;
		}
		float num3 = 0f;
		float num4 = contentLength - visibleAreaLength;
		if (isBackgroundButtonDown)
		{
			if (num2 < num3)
			{
				num2 += (0f - num2) / visibleAreaLength / 2f;
				if (num2 > num3)
				{
					num2 = num3;
				}
			}
			else if (num2 > num4)
			{
				num2 -= (num2 - num4) / visibleAreaLength / 2f;
				if (num2 < num4)
				{
					num2 = num4;
				}
			}
			if (scrollAxes == Axes.XAxis)
			{
				swipeScrollingContentDestLocalPos.x = num2;
			}
			else if (scrollAxes == Axes.YAxis)
			{
				swipeScrollingContentDestLocalPos.y = num2;
			}
			num = ((!(contentLength - visibleAreaLength > Mathf.Epsilon)) ? 0f : (num2 / (contentLength - visibleAreaLength)));
		}
		else
		{
			float num5 = visibleAreaLength * 0.001f;
			if (num2 < num3 || num2 > num4)
			{
				float num6 = ((!(num2 < num3)) ? num4 : num3);
				num2 = Mathf.SmoothDamp(num2, num6, ref snapBackVelocity, 0.05f, float.PositiveInfinity, tk2dUITime.deltaTime);
				if (Mathf.Abs(snapBackVelocity) < num5)
				{
					num2 = num6;
					snapBackVelocity = 0f;
				}
				swipeCurrVelocity = 0f;
			}
			else if (swipeCurrVelocity != 0f)
			{
				num2 += swipeCurrVelocity * tk2dUITime.deltaTime * 20f;
				if (swipeCurrVelocity > num5 || swipeCurrVelocity < 0f - num5)
				{
					swipeCurrVelocity = Mathf.Lerp(swipeCurrVelocity, 0f, tk2dUITime.deltaTime * 2.5f);
				}
				else
				{
					swipeCurrVelocity = 0f;
				}
			}
			else
			{
				isSwipeScrollingInProgress = false;
				tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
			}
			if (scrollAxes == Axes.XAxis)
			{
				swipeScrollingContentDestLocalPos.x = num2;
			}
			else if (scrollAxes == Axes.YAxis)
			{
				swipeScrollingContentDestLocalPos.y = num2;
			}
			num = num2 / (contentLength - visibleAreaLength);
		}
		if (num != percent)
		{
			percent = num;
			ContentContainerOffset = swipeScrollingContentDestLocalPos;
			if (this.OnScroll != null)
			{
				this.OnScroll(this);
			}
			TargetOnScrollCallback();
		}
		if ((Object)(object)scrollBar != (Object)null)
		{
			float scrollPercentWithoutEvent = percent;
			if (scrollAxes == Axes.XAxis)
			{
				scrollPercentWithoutEvent = ContentContainerOffset.x / (contentLength - visibleAreaLength);
			}
			else if (scrollAxes == Axes.YAxis)
			{
				scrollPercentWithoutEvent = ContentContainerOffset.y / (contentLength - visibleAreaLength);
			}
			scrollBar.SetScrollPercentWithoutEvent(scrollPercentWithoutEvent);
		}
	}

	private void UpdateSwipeScrollDestintationPosition()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Component)this).transform.InverseTransformPoint(CalculateClickWorldPos(backgroundUIItem));
		Vector3 val2 = val - swipeScrollingPressDownStartLocalPos;
		val2.x *= -1f;
		float num = 0f;
		if (scrollAxes == Axes.XAxis)
		{
			num = val2.x;
			swipeCurrVelocity = 0f - (val.x - swipePrevScrollingContentPressLocalPos.x);
		}
		else if (scrollAxes == Axes.YAxis)
		{
			num = val2.y;
			swipeCurrVelocity = val.y - swipePrevScrollingContentPressLocalPos.y;
		}
		if (!isSwipeScrollingInProgress && Mathf.Abs(num) > 0.02f)
		{
			isSwipeScrollingInProgress = true;
			tk2dUIManager.Instance.OverrideClearAllChildrenPresses(backgroundUIItem);
		}
		if (isSwipeScrollingInProgress)
		{
			Vector3 val3 = swipeScrollingContentStartLocalPos + val2;
			val3.z = ContentContainerOffset.z;
			if (scrollAxes == Axes.XAxis)
			{
				val3.y = ContentContainerOffset.y;
			}
			else if (scrollAxes == Axes.YAxis)
			{
				val3.x = ContentContainerOffset.x;
			}
			val3.z = ContentContainerOffset.z;
			swipeScrollingContentDestLocalPos = val3;
			swipePrevScrollingContentPressLocalPos = val;
		}
	}

	private void BackgroundButtonRelease()
	{
		if (allowSwipeScrolling)
		{
			if (isBackgroundButtonDown && !isSwipeScrollingInProgress)
			{
				tk2dUIManager.Instance.OnInputUpdate -= BackgroundOverUpdate;
			}
			isBackgroundButtonDown = false;
		}
	}

	private void BackgroundButtonHoverOver()
	{
		if (allowScrollWheel)
		{
			if (!isBackgroundButtonOver)
			{
				tk2dUIManager.Instance.OnScrollWheelChange += BackgroundHoverOverScrollWheelChange;
			}
			isBackgroundButtonOver = true;
		}
	}

	private void BackgroundButtonHoverOut()
	{
		if (isBackgroundButtonOver)
		{
			tk2dUIManager.Instance.OnScrollWheelChange -= BackgroundHoverOverScrollWheelChange;
		}
		isBackgroundButtonOver = false;
	}

	private Vector3 CalculateClickWorldPos(tk2dUIItem btn)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		Vector2 position = btn.Touch.position;
		Camera uICameraForControl = tk2dUIManager.Instance.GetUICameraForControl(((Component)this).gameObject);
		Vector3 result = uICameraForControl.ScreenToWorldPoint(new Vector3(position.x, position.y, ((Component)btn).transform.position.z - ((Component)uICameraForControl).transform.position.z));
		result.z = ((Component)btn).transform.position.z;
		return result;
	}

	private void UpdateScrollbarActiveState()
	{
		bool flag = contentLength > visibleAreaLength;
		if ((Object)(object)scrollBar != (Object)null && ((Component)scrollBar).gameObject.activeSelf != flag)
		{
			tk2dUIBaseItemControl.ChangeGameObjectActiveState(((Component)scrollBar).gameObject, flag);
		}
	}

	private void ContentLengthVisibleAreaLengthChange(float prevContentLength, float newContentLength, float prevVisibleAreaLength, float newVisibleAreaLength)
	{
		float value = ((newContentLength - visibleAreaLength == 0f) ? 0f : ((prevContentLength - prevVisibleAreaLength) * Value / (newContentLength - newVisibleAreaLength)));
		contentLength = newContentLength;
		visibleAreaLength = newVisibleAreaLength;
		UpdateScrollbarActiveState();
		Value = value;
	}

	private void UnpressAllUIItemChildren()
	{
	}

	private void TargetOnScrollCallback()
	{
		if ((Object)(object)SendMessageTarget != (Object)null && SendMessageOnScrollMethodName.Length > 0)
		{
			SendMessageTarget.SendMessage(SendMessageOnScrollMethodName, (object)this, (SendMessageOptions)0);
		}
	}

	private static void GetRendererBoundsInChildren(Matrix4x4 rootWorldToLocal, Vector3[] minMax, Transform t)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		MeshFilter component = ((Component)t).GetComponent<MeshFilter>();
		if ((Object)(object)component != (Object)null && (Object)(object)component.sharedMesh != (Object)null)
		{
			Bounds bounds = component.sharedMesh.bounds;
			Matrix4x4 val = rootWorldToLocal * t.localToWorldMatrix;
			for (int i = 0; i < 8; i++)
			{
				Vector3 val2 = ((Bounds)(ref bounds)).center + Vector3.Scale(((Bounds)(ref bounds)).extents, boxExtents[i]);
				Vector3 val3 = ((Matrix4x4)(ref val)).MultiplyPoint(val2);
				ref Vector3 reference = ref minMax[0];
				reference = Vector3.Min(minMax[0], val3);
				ref Vector3 reference2 = ref minMax[1];
				reference2 = Vector3.Max(minMax[1], val3);
			}
		}
		int childCount = t.childCount;
		for (int j = 0; j < childCount; j++)
		{
			Transform child = t.GetChild(j);
			if (((Component)t).gameObject.activeSelf)
			{
				GetRendererBoundsInChildren(rootWorldToLocal, minMax, child);
			}
		}
	}

	private void LayoutReshaped(Vector3 dMin, Vector3 dMax)
	{
		VisibleAreaLength += ((scrollAxes != 0) ? (dMax.y - dMin.y) : (dMax.x - dMin.x));
	}

	private void ContentLayoutChangeCallback()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)contentLayoutContainer != (Object)null)
		{
			Vector2 innerSize = contentLayoutContainer.GetInnerSize();
			ContentLength = ((scrollAxes != 0) ? innerSize.y : innerSize.x);
		}
	}
}
