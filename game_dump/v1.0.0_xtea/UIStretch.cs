using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Stretch")]
[ExecuteInEditMode]
public class UIStretch : MonoBehaviour
{
	public enum Style
	{
		None,
		Horizontal,
		Vertical,
		Both,
		BasedOnHeight,
		FillKeepingRatio,
		FitInternalKeepingRatio
	}

	public Camera uiCamera;

	public GameObject container;

	public Style style;

	public bool runOnlyOnce = true;

	public Vector2 relativeSize = Vector2.one;

	public Vector2 initialSize = Vector2.one;

	public Vector2 borderPadding = Vector2.zero;

	[HideInInspector]
	[SerializeField]
	private UIWidget widgetContainer;

	private Transform mTrans;

	private UIWidget mWidget;

	private UISprite mSprite;

	private UIPanel mPanel;

	private UIRoot mRoot;

	private Animation mAnim;

	private Rect mRect;

	private bool mStarted;

	private void Awake()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		mAnim = ((Component)this).GetComponent<Animation>();
		mRect = default(Rect);
		mTrans = ((Component)this).transform;
		mWidget = ((Component)this).GetComponent<UIWidget>();
		mSprite = ((Component)this).GetComponent<UISprite>();
		mPanel = ((Component)this).GetComponent<UIPanel>();
		UICamera.onScreenResize = (UICamera.OnScreenResize)Delegate.Combine(UICamera.onScreenResize, new UICamera.OnScreenResize(ScreenSizeChanged));
	}

	private void OnDestroy()
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
		if ((Object)(object)uiCamera == (Object)null)
		{
			uiCamera = NGUITools.FindCameraForLayer(((Component)this).gameObject.layer);
		}
		mRoot = NGUITools.FindInParents<UIRoot>(((Component)this).gameObject);
		Update();
		mStarted = true;
	}

	private void Update()
	{
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0311: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0281: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_06db: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0797: Unknown result type (might be due to invalid IL or missing references)
		//IL_079c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_073b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0740: Unknown result type (might be due to invalid IL or missing references)
		//IL_063d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0642: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ae: Unknown result type (might be due to invalid IL or missing references)
		if (((Object)(object)mAnim != (Object)null && mAnim.isPlaying) || style == Style.None)
		{
			return;
		}
		UIWidget uIWidget = ((!((Object)(object)container == (Object)null)) ? container.GetComponent<UIWidget>() : null);
		UIPanel uIPanel = ((!((Object)(object)container == (Object)null) || !((Object)(object)uIWidget == (Object)null)) ? container.GetComponent<UIPanel>() : null);
		float num = 1f;
		if ((Object)(object)uIWidget != (Object)null)
		{
			Bounds val = uIWidget.CalculateBounds(((Component)this).transform.parent);
			((Rect)(ref mRect)).x = ((Bounds)(ref val)).min.x;
			((Rect)(ref mRect)).y = ((Bounds)(ref val)).min.y;
			((Rect)(ref mRect)).width = ((Bounds)(ref val)).size.x;
			((Rect)(ref mRect)).height = ((Bounds)(ref val)).size.y;
		}
		else if ((Object)(object)uIPanel != (Object)null)
		{
			if (uIPanel.clipping == UIDrawCall.Clipping.None)
			{
				float num2 = ((!((Object)(object)mRoot != (Object)null)) ? 0.5f : ((float)mRoot.activeHeight / (float)Screen.height * 0.5f));
				((Rect)(ref mRect)).xMin = (float)(-Screen.width) * num2;
				((Rect)(ref mRect)).yMin = (float)(-Screen.height) * num2;
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
			Transform parent = ((Component)this).transform.parent;
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
			mRect = uiCamera.pixelRect;
			if ((Object)(object)mRoot != (Object)null)
			{
				num = mRoot.pixelSizeAdjustment;
			}
		}
		float num3 = ((Rect)(ref mRect)).width;
		float num4 = ((Rect)(ref mRect)).height;
		if (num != 1f && num4 > 1f)
		{
			float num5 = (float)mRoot.activeHeight / num4;
			num3 *= num5;
			num4 *= num5;
		}
		Vector3 val3 = (Vector3)((!((Object)(object)mWidget != (Object)null)) ? mTrans.localScale : new Vector3((float)mWidget.width, (float)mWidget.height));
		if (style == Style.BasedOnHeight)
		{
			val3.x = relativeSize.x * num4;
			val3.y = relativeSize.y * num4;
		}
		else if (style == Style.FillKeepingRatio)
		{
			float num6 = num3 / num4;
			float num7 = initialSize.x / initialSize.y;
			if (num7 < num6)
			{
				float num8 = num3 / initialSize.x;
				val3.x = num3;
				val3.y = initialSize.y * num8;
			}
			else
			{
				float num9 = num4 / initialSize.y;
				val3.x = initialSize.x * num9;
				val3.y = num4;
			}
		}
		else if (style == Style.FitInternalKeepingRatio)
		{
			float num10 = num3 / num4;
			float num11 = initialSize.x / initialSize.y;
			if (num11 > num10)
			{
				float num12 = num3 / initialSize.x;
				val3.x = num3;
				val3.y = initialSize.y * num12;
			}
			else
			{
				float num13 = num4 / initialSize.y;
				val3.x = initialSize.x * num13;
				val3.y = num4;
			}
		}
		else
		{
			if (style != Style.Vertical)
			{
				val3.x = relativeSize.x * num3;
			}
			if (style != Style.Horizontal)
			{
				val3.y = relativeSize.y * num4;
			}
		}
		if ((Object)(object)mSprite != (Object)null)
		{
			float num14 = ((!((Object)(object)mSprite.atlas != (Object)null)) ? 1f : mSprite.atlas.pixelSize);
			val3.x -= borderPadding.x * num14;
			val3.y -= borderPadding.y * num14;
			if (style != Style.Vertical)
			{
				mSprite.width = Mathf.RoundToInt(val3.x);
			}
			if (style != Style.Horizontal)
			{
				mSprite.height = Mathf.RoundToInt(val3.y);
			}
			val3 = Vector3.one;
		}
		else if ((Object)(object)mWidget != (Object)null)
		{
			if (style != Style.Vertical)
			{
				mWidget.width = Mathf.RoundToInt(val3.x - borderPadding.x);
			}
			if (style != Style.Horizontal)
			{
				mWidget.height = Mathf.RoundToInt(val3.y - borderPadding.y);
			}
			val3 = Vector3.one;
		}
		else if ((Object)(object)mPanel != (Object)null)
		{
			Vector4 baseClipRegion = mPanel.baseClipRegion;
			if (style != Style.Vertical)
			{
				baseClipRegion.z = val3.x - borderPadding.x;
			}
			if (style != Style.Horizontal)
			{
				baseClipRegion.w = val3.y - borderPadding.y;
			}
			mPanel.baseClipRegion = baseClipRegion;
			val3 = Vector3.one;
		}
		else
		{
			if (style != Style.Vertical)
			{
				val3.x -= borderPadding.x;
			}
			if (style != Style.Horizontal)
			{
				val3.y -= borderPadding.y;
			}
		}
		if (mTrans.localScale != val3)
		{
			mTrans.localScale = val3;
		}
		if (runOnlyOnce && Application.isPlaying)
		{
			((Behaviour)this).enabled = false;
		}
	}
}
