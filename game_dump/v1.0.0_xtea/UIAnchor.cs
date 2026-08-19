using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/Anchor")]
public class UIAnchor : MonoBehaviour
{
	public enum Side
	{
		BottomLeft,
		Left,
		TopLeft,
		Top,
		TopRight,
		Right,
		BottomRight,
		Bottom,
		Center
	}

	public Camera uiCamera;

	public GameObject container;

	public Side side = Side.Center;

	public bool runOnlyOnce = true;

	public Vector2 relativeOffset = Vector2.zero;

	public Vector2 pixelOffset = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private UIWidget widgetContainer;

	private Transform mTrans;

	private Animation mAnim;

	private Rect mRect = default(Rect);

	private UIRoot mRoot;

	private bool mStarted;

	private void OnEnable()
	{
		mTrans = ((Component)this).transform;
		mAnim = ((Component)this).GetComponent<Animation>();
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void OnDisable()
	{
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Remove(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void ScreenSizeChanged()
	{
		if (mStarted && runOnlyOnce)
		{
			Update();
		}
	}

	private void Start()
	{
		if ((Object)(object)container == (Object)null && (Object)(object)widgetContainer != (Object)null)
		{
			container = ((Component)widgetContainer).gameObject;
			widgetContainer = null;
		}
		mRoot = NGUITools.FindInParents<UIRoot>(((Component)this).gameObject);
		if ((Object)(object)uiCamera == (Object)null)
		{
			uiCamera = NGUITools.FindCameraForLayer(((Component)this).gameObject.layer);
		}
		Update();
		mStarted = true;
	}

	private void Update()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0272: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0599: Unknown result type (might be due to invalid IL or missing references)
		//IL_059b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_053a: Unknown result type (might be due to invalid IL or missing references)
		//IL_053f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0553: Unknown result type (might be due to invalid IL or missing references)
		//IL_0555: Unknown result type (might be due to invalid IL or missing references)
		//IL_055a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05db: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0694: Unknown result type (might be due to invalid IL or missing references)
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_066b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0670: Unknown result type (might be due to invalid IL or missing references)
		//IL_0682: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mAnim != (Object)null && ((Behaviour)mAnim).enabled && mAnim.isPlaying)
		{
			return;
		}
		bool flag = false;
		UIWidget uIWidget = ((!((Object)(object)container == (Object)null)) ? container.GetComponent<UIWidget>() : null);
		UIPanel uIPanel = ((!((Object)(object)container == (Object)null) || !((Object)(object)uIWidget == (Object)null)) ? container.GetComponent<UIPanel>() : null);
		if ((Object)(object)uIWidget != (Object)null)
		{
			Bounds val = uIWidget.CalculateBounds(container.transform.parent);
			((Rect)(ref mRect)).x = ((Bounds)(ref val)).min.x;
			((Rect)(ref mRect)).y = ((Bounds)(ref val)).min.y;
			((Rect)(ref mRect)).width = ((Bounds)(ref val)).size.x;
			((Rect)(ref mRect)).height = ((Bounds)(ref val)).size.y;
		}
		else if ((Object)(object)uIPanel != (Object)null)
		{
			if (uIPanel.clipping == UIDrawCall.Clipping.None)
			{
				float num = ((!((Object)(object)mRoot != (Object)null)) ? 0.5f : ((float)mRoot.activeHeight / (float)Screen.height * 0.5f));
				((Rect)(ref mRect)).xMin = (float)(-Screen.width) * num;
				((Rect)(ref mRect)).yMin = (float)(-Screen.height) * num;
				((Rect)(ref mRect)).xMax = 0f - ((Rect)(ref mRect)).xMin;
				((Rect)(ref mRect)).yMax = 0f - ((Rect)(ref mRect)).yMin;
			}
			else
			{
				Vector4 finalClipRegion = uIPanel.finalClipRegion;
				((Rect)(ref mRect)).x = finalClipRegion.x - finalClipRegion.z * 0.5f;
				((Rect)(ref mRect)).y = finalClipRegion.y - finalClipRegion.w * 0.5f;
				((Rect)(ref mRect)).width = finalClipRegion.z;
				((Rect)(ref mRect)).height = finalClipRegion.w;
			}
		}
		else if ((Object)(object)container != (Object)null)
		{
			Transform parent = container.transform.parent;
			Bounds val2 = ((!((Object)(object)parent != (Object)null)) ? NGUIMath.CalculateRelativeWidgetBounds(container.transform) : NGUIMath.CalculateRelativeWidgetBounds(parent, container.transform));
			((Rect)(ref mRect)).x = ((Bounds)(ref val2)).min.x;
			((Rect)(ref mRect)).y = ((Bounds)(ref val2)).min.y;
			((Rect)(ref mRect)).width = ((Bounds)(ref val2)).size.x;
			((Rect)(ref mRect)).height = ((Bounds)(ref val2)).size.y;
		}
		else
		{
			if (!((Object)(object)uiCamera != (Object)null))
			{
				return;
			}
			flag = true;
			mRect = uiCamera.pixelRect;
		}
		float num2 = (((Rect)(ref mRect)).xMin + ((Rect)(ref mRect)).xMax) * 0.5f;
		float num3 = (((Rect)(ref mRect)).yMin + ((Rect)(ref mRect)).yMax) * 0.5f;
		Vector3 val3 = default(Vector3);
		((Vector3)(ref val3))._002Ector(num2, num3, 0f);
		if (side != Side.Center)
		{
			if (side == Side.Right || side == Side.TopRight || side == Side.BottomRight)
			{
				val3.x = ((Rect)(ref mRect)).xMax;
			}
			else if (side == Side.Top || side == Side.Center || side == Side.Bottom)
			{
				val3.x = num2;
			}
			else
			{
				val3.x = ((Rect)(ref mRect)).xMin;
			}
			if (side == Side.Top || side == Side.TopRight || side == Side.TopLeft)
			{
				val3.y = ((Rect)(ref mRect)).yMax;
			}
			else if (side == Side.Left || side == Side.Center || side == Side.Right)
			{
				val3.y = num3;
			}
			else
			{
				val3.y = ((Rect)(ref mRect)).yMin;
			}
		}
		float width = ((Rect)(ref mRect)).width;
		float height = ((Rect)(ref mRect)).height;
		val3.x += pixelOffset.x + relativeOffset.x * width;
		val3.y += pixelOffset.y + relativeOffset.y * height;
		if (flag)
		{
			if (uiCamera.orthographic)
			{
				val3.x = Mathf.Round(val3.x);
				val3.y = Mathf.Round(val3.y);
			}
			val3.z = uiCamera.WorldToScreenPoint(mTrans.position).z;
			val3 = uiCamera.ScreenToWorldPoint(val3);
		}
		else
		{
			val3.x = Mathf.Round(val3.x);
			val3.y = Mathf.Round(val3.y);
			if ((Object)(object)uIPanel != (Object)null)
			{
				val3 = uIPanel.cachedTransform.TransformPoint(val3);
			}
			else if ((Object)(object)container != (Object)null)
			{
				Transform parent2 = container.transform.parent;
				if ((Object)(object)parent2 != (Object)null)
				{
					val3 = parent2.TransformPoint(val3);
				}
			}
			val3.z = mTrans.position.z;
		}
		if (flag && uiCamera.orthographic && (Object)(object)mTrans.parent != (Object)null)
		{
			val3 = mTrans.parent.InverseTransformPoint(val3);
			val3.x = Mathf.RoundToInt(val3.x);
			val3.y = Mathf.RoundToInt(val3.y);
			if (mTrans.localPosition != val3)
			{
				mTrans.localPosition = val3;
			}
		}
		else if (mTrans.position != val3)
		{
			mTrans.position = val3;
		}
		if (runOnlyOnce && Application.isPlaying)
		{
			((Behaviour)this).enabled = false;
		}
	}
}
