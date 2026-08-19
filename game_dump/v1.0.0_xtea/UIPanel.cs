using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Panel")]
public class UIPanel : UIRect
{
	public enum RenderQueue
	{
		Automatic,
		StartAt,
		Explicit
	}

	public delegate void OnGeometryUpdated();

	public delegate void OnClippingMoved(UIPanel panel);

	public static List<UIPanel> list = new List<UIPanel>();

	public static Action<UIPanel> onPanelAdded;

	public static Action<UIPanel> onPanelRemoved;

	public Action onDrawcallListUpdated;

	public OnGeometryUpdated onGeometryUpdated;

	public bool showInPanelTool = true;

	public bool generateNormals;

	public bool widgetsAreStatic;

	public bool cullWhileDragging = true;

	public bool alwaysOnScreen;

	public bool anchorOffset;

	public bool softBorderPadding = true;

	public RenderQueue renderQueue;

	public int startingRenderQueue = 3000;

	[NonSerialized]
	public List<UIWidget> widgets = new List<UIWidget>();

	[NonSerialized]
	public List<UIDrawCall> drawCalls = new List<UIDrawCall>();

	[NonSerialized]
	public Matrix4x4 worldToLocal = Matrix4x4.identity;

	[NonSerialized]
	public Vector4 drawCallClipRange = new Vector4(0f, 0f, 1f, 1f);

	public OnClippingMoved onClipMove;

	[SerializeField]
	[HideInInspector]
	private Texture2D mClipTexture;

	[HideInInspector]
	[SerializeField]
	private float mAlpha = 1f;

	[SerializeField]
	[HideInInspector]
	private UIDrawCall.Clipping mClipping;

	[SerializeField]
	[HideInInspector]
	private Vector4 mClipRange = new Vector4(0f, 0f, 300f, 200f);

	[HideInInspector]
	[SerializeField]
	private Vector2 mClipSoftness = new Vector2(4f, 4f);

	[SerializeField]
	[HideInInspector]
	private int mDepth;

	[HideInInspector]
	[SerializeField]
	private int mSortingOrder;

	[HideInInspector]
	[SerializeField]
	private string mSortingLayerName;

	private bool mRebuild;

	private bool mResized;

	[SerializeField]
	private Vector2 mClipOffset = Vector2.zero;

	private int mMatrixFrame = -1;

	private int mAlphaFrameID;

	private int mLayer = -1;

	private static float[] mTemp = new float[4];

	private Vector2 mMin = Vector2.zero;

	private Vector2 mMax = Vector2.zero;

	private bool mHalfPixelOffset;

	private bool mSortWidgets;

	private bool mUpdateScroll;

	private UIPanel mParentPanel;

	private static Vector3[] mCorners = (Vector3[])(object)new Vector3[4];

	private static int mUpdateFrame = -1;

	private UIDrawCall.OnRenderCallback mOnRender;

	private bool mForced;

	public string sortingLayerName
	{
		get
		{
			return mSortingLayerName;
		}
		set
		{
			if (mSortingLayerName != value)
			{
				mSortingLayerName = value;
				UpdateDrawCalls();
			}
		}
	}

	public static int nextUnusedDepth
	{
		get
		{
			int num = int.MinValue;
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				num = Mathf.Max(num, list[i].depth);
			}
			return (num != int.MinValue) ? (num + 1) : 0;
		}
	}

	public override bool canBeAnchored => mClipping != UIDrawCall.Clipping.None;

	public override float alpha
	{
		get
		{
			return mAlpha;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mAlpha != num)
			{
				bool flag = mAlpha > 0.001f;
				mAlphaFrameID = -1;
				mResized = true;
				mAlpha = num;
				int i = 0;
				for (int count = drawCalls.Count; i < count; i++)
				{
					drawCalls[i].isDirty = true;
				}
				Invalidate(flag != mAlpha > 0.001f);
			}
		}
	}

	public int depth
	{
		get
		{
			return mDepth;
		}
		set
		{
			if (mDepth != value)
			{
				mDepth = value;
				list.Sort(CompareFunc);
			}
		}
	}

	public int sortingOrder
	{
		get
		{
			return mSortingOrder;
		}
		set
		{
			if (mSortingOrder != value)
			{
				mSortingOrder = value;
				UpdateDrawCalls();
			}
		}
	}

	public float width => GetViewSize().x;

	public float height => GetViewSize().y;

	public bool halfPixelOffset => mHalfPixelOffset;

	public bool usedForUI => (Object)(object)base.anchorCamera != (Object)null && mCam.orthographic;

	public Vector3 drawCallOffset
	{
		get
		{
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			if (mHalfPixelOffset && (Object)(object)base.anchorCamera != (Object)null && mCam.orthographic)
			{
				Vector2 windowSize = GetWindowSize();
				float num = ((!((Object)(object)base.root != (Object)null)) ? 1f : base.root.pixelSizeAdjustment);
				float num2 = num / windowSize.y / mCam.orthographicSize;
				return new Vector3(0f - num2, num2);
			}
			return Vector3.zero;
		}
	}

	public UIDrawCall.Clipping clipping
	{
		get
		{
			return mClipping;
		}
		set
		{
			if (mClipping != value)
			{
				mResized = true;
				mClipping = value;
				mMatrixFrame = -1;
			}
		}
	}

	public UIPanel parentPanel => mParentPanel;

	public int clipCount
	{
		get
		{
			int num = 0;
			UIPanel uIPanel = this;
			while ((Object)(object)uIPanel != (Object)null)
			{
				if (uIPanel.mClipping == UIDrawCall.Clipping.SoftClip || uIPanel.mClipping == UIDrawCall.Clipping.TextureMask)
				{
					num++;
				}
				uIPanel = uIPanel.mParentPanel;
			}
			return num;
		}
	}

	public bool hasClipping => mClipping == UIDrawCall.Clipping.SoftClip || mClipping == UIDrawCall.Clipping.TextureMask;

	public bool hasCumulativeClipping => clipCount != 0;

	[Obsolete("Use 'hasClipping' or 'hasCumulativeClipping' instead")]
	public bool clipsChildren => hasCumulativeClipping;

	public Vector2 clipOffset
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mClipOffset;
		}
		set
		{
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			if (Mathf.Abs(mClipOffset.x - value.x) > 0.001f || Mathf.Abs(mClipOffset.y - value.y) > 0.001f)
			{
				mClipOffset = value;
				InvalidateClipping();
				if (onClipMove != null)
				{
					onClipMove(this);
				}
			}
		}
	}

	public Texture2D clipTexture
	{
		get
		{
			return mClipTexture;
		}
		set
		{
			if ((Object)(object)mClipTexture != (Object)(object)value)
			{
				mClipTexture = value;
			}
		}
	}

	[Obsolete("Use 'finalClipRegion' or 'baseClipRegion' instead")]
	public Vector4 clipRange
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return baseClipRegion;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			baseClipRegion = value;
		}
	}

	public Vector4 baseClipRegion
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mClipRange;
		}
		set
		{
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			if (Mathf.Abs(mClipRange.x - value.x) > 0.001f || Mathf.Abs(mClipRange.y - value.y) > 0.001f || Mathf.Abs(mClipRange.z - value.z) > 0.001f || Mathf.Abs(mClipRange.w - value.w) > 0.001f)
			{
				mResized = true;
				mClipRange = value;
				mMatrixFrame = -1;
				UIScrollView component = ((Component)this).GetComponent<UIScrollView>();
				if ((Object)(object)component != (Object)null)
				{
					component.UpdatePosition();
				}
				if (onClipMove != null)
				{
					onClipMove(this);
				}
			}
		}
	}

	public Vector4 finalClipRegion
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			Vector2 viewSize = GetViewSize();
			if (mClipping != 0)
			{
				return new Vector4(mClipRange.x + mClipOffset.x, mClipRange.y + mClipOffset.y, viewSize.x, viewSize.y);
			}
			return new Vector4(0f, 0f, viewSize.x, viewSize.y);
		}
	}

	public Vector2 clipSoftness
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mClipSoftness;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (mClipSoftness != value)
			{
				mClipSoftness = value;
			}
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0103: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_011c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			if (mClipping == UIDrawCall.Clipping.None)
			{
				Vector3[] array = worldCorners;
				Transform val = base.cachedTransform;
				for (int i = 0; i < 4; i++)
				{
					ref Vector3 reference = ref array[i];
					reference = val.InverseTransformPoint(array[i]);
				}
				return array;
			}
			float num = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
			float num2 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
			float num3 = num + mClipRange.z;
			float num4 = num2 + mClipRange.w;
			ref Vector3 reference2 = ref mCorners[0];
			reference2 = new Vector3(num, num2);
			ref Vector3 reference3 = ref mCorners[1];
			reference3 = new Vector3(num, num4);
			ref Vector3 reference4 = ref mCorners[2];
			reference4 = new Vector3(num3, num4);
			ref Vector3 reference5 = ref mCorners[3];
			reference5 = new Vector3(num3, num2);
			return mCorners;
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_0128: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			//IL_0179: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_0227: Unknown result type (might be due to invalid IL or missing references)
			//IL_022c: Unknown result type (might be due to invalid IL or missing references)
			//IL_022e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0233: Unknown result type (might be due to invalid IL or missing references)
			if (mClipping != 0)
			{
				float num = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
				float num2 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
				float num3 = num + mClipRange.z;
				float num4 = num2 + mClipRange.w;
				Transform val = base.cachedTransform;
				ref Vector3 reference = ref mCorners[0];
				reference = val.TransformPoint(num, num2, 0f);
				ref Vector3 reference2 = ref mCorners[1];
				reference2 = val.TransformPoint(num, num4, 0f);
				ref Vector3 reference3 = ref mCorners[2];
				reference3 = val.TransformPoint(num3, num4, 0f);
				ref Vector3 reference4 = ref mCorners[3];
				reference4 = val.TransformPoint(num3, num2, 0f);
			}
			else
			{
				if ((Object)(object)base.anchorCamera != (Object)null)
				{
					return mCam.GetWorldCorners(base.cameraRayDistance);
				}
				Vector2 viewSize = GetViewSize();
				float num5 = -0.5f * viewSize.x;
				float num6 = -0.5f * viewSize.y;
				float num7 = num5 + viewSize.x;
				float num8 = num6 + viewSize.y;
				ref Vector3 reference5 = ref mCorners[0];
				reference5 = new Vector3(num5, num6);
				ref Vector3 reference6 = ref mCorners[1];
				reference6 = new Vector3(num5, num8);
				ref Vector3 reference7 = ref mCorners[2];
				reference7 = new Vector3(num7, num8);
				ref Vector3 reference8 = ref mCorners[3];
				reference8 = new Vector3(num7, num6);
				if (anchorOffset && ((Object)(object)mCam == (Object)null || (Object)(object)((Component)mCam).transform.parent != (Object)(object)base.cachedTransform))
				{
					Vector3 position = base.cachedTransform.position;
					for (int i = 0; i < 4; i++)
					{
						ref Vector3 reference9 = ref mCorners[i];
						reference9 += position;
					}
				}
			}
			return mCorners;
		}
	}

	public static int CompareFunc(UIPanel a, UIPanel b)
	{
		if ((Object)(object)a != (Object)(object)b && (Object)(object)a != (Object)null && (Object)(object)b != (Object)null)
		{
			if (a.mDepth < b.mDepth)
			{
				return -1;
			}
			if (a.mDepth > b.mDepth)
			{
				return 1;
			}
			return (((Object)a).GetInstanceID() >= ((Object)b).GetInstanceID()) ? 1 : (-1);
		}
		return 0;
	}

	private void InvalidateClipping()
	{
		mResized = true;
		mMatrixFrame = -1;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			UIPanel uIPanel = list[i];
			if ((Object)(object)uIPanel != (Object)(object)this && (Object)(object)uIPanel.parentPanel == (Object)(object)this)
			{
				uIPanel.InvalidateClipping();
			}
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		if (mClipping != 0)
		{
			float num = mClipOffset.x + mClipRange.x - 0.5f * mClipRange.z;
			float num2 = mClipOffset.y + mClipRange.y - 0.5f * mClipRange.w;
			float num3 = num + mClipRange.z;
			float num4 = num2 + mClipRange.w;
			float num5 = (num + num3) * 0.5f;
			float num6 = (num2 + num4) * 0.5f;
			Transform val = base.cachedTransform;
			ref Vector3 reference = ref UIRect.mSides[0];
			reference = val.TransformPoint(num, num6, 0f);
			ref Vector3 reference2 = ref UIRect.mSides[1];
			reference2 = val.TransformPoint(num5, num4, 0f);
			ref Vector3 reference3 = ref UIRect.mSides[2];
			reference3 = val.TransformPoint(num3, num6, 0f);
			ref Vector3 reference4 = ref UIRect.mSides[3];
			reference4 = val.TransformPoint(num5, num2, 0f);
			if ((Object)(object)relativeTo != (Object)null)
			{
				for (int i = 0; i < 4; i++)
				{
					ref Vector3 reference5 = ref UIRect.mSides[i];
					reference5 = relativeTo.InverseTransformPoint(UIRect.mSides[i]);
				}
			}
			return UIRect.mSides;
		}
		if ((Object)(object)base.anchorCamera != (Object)null && anchorOffset)
		{
			Vector3[] sides = mCam.GetSides(base.cameraRayDistance);
			Vector3 position = base.cachedTransform.position;
			for (int j = 0; j < 4; j++)
			{
				ref Vector3 reference6 = ref sides[j];
				reference6 += position;
			}
			if ((Object)(object)relativeTo != (Object)null)
			{
				for (int k = 0; k < 4; k++)
				{
					ref Vector3 reference7 = ref sides[k];
					reference7 = relativeTo.InverseTransformPoint(sides[k]);
				}
			}
			return sides;
		}
		return base.GetSides(relativeTo);
	}

	public override void Invalidate(bool includeChildren)
	{
		mAlphaFrameID = -1;
		base.Invalidate(includeChildren);
	}

	public override float CalculateFinalAlpha(int frameID)
	{
		if (mAlphaFrameID != frameID)
		{
			mAlphaFrameID = frameID;
			UIRect uIRect = base.parent;
			finalAlpha = ((!((Object)(object)base.parent != (Object)null)) ? mAlpha : (uIRect.CalculateFinalAlpha(frameID) * mAlpha));
		}
		return finalAlpha;
	}

	public override void SetRect(float x, float y, float width, float height)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt(width + 0.5f);
		int num2 = Mathf.FloorToInt(height + 0.5f);
		num = num >> 1 << 1;
		num2 = num2 >> 1 << 1;
		Transform val = base.cachedTransform;
		Vector3 localPosition = val.localPosition;
		localPosition.x = Mathf.Floor(x + 0.5f);
		localPosition.y = Mathf.Floor(y + 0.5f);
		if (num < 2)
		{
			num = 2;
		}
		if (num2 < 2)
		{
			num2 = 2;
		}
		baseClipRegion = new Vector4(localPosition.x, localPosition.y, (float)num, (float)num2);
		if (base.isAnchored)
		{
			val = val.parent;
			if (Object.op_Implicit((Object)(object)leftAnchor.target))
			{
				leftAnchor.SetHorizontal(val, x);
			}
			if (Object.op_Implicit((Object)(object)rightAnchor.target))
			{
				rightAnchor.SetHorizontal(val, x + width);
			}
			if (Object.op_Implicit((Object)(object)bottomAnchor.target))
			{
				bottomAnchor.SetVertical(val, y);
			}
			if (Object.op_Implicit((Object)(object)topAnchor.target))
			{
				topAnchor.SetVertical(val, y + height);
			}
		}
	}

	public bool IsVisible(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		UpdateTransformMatrix();
		a = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(a);
		b = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(b);
		c = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(c);
		d = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(d);
		mTemp[0] = a.x;
		mTemp[1] = b.x;
		mTemp[2] = c.x;
		mTemp[3] = d.x;
		float num = Mathf.Min(mTemp);
		float num2 = Mathf.Max(mTemp);
		mTemp[0] = a.y;
		mTemp[1] = b.y;
		mTemp[2] = c.y;
		mTemp[3] = d.y;
		float num3 = Mathf.Min(mTemp);
		float num4 = Mathf.Max(mTemp);
		if (num2 < mMin.x)
		{
			return false;
		}
		if (num4 < mMin.y)
		{
			return false;
		}
		if (num > mMax.x)
		{
			return false;
		}
		if (num3 > mMax.y)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible(Vector3 worldPos)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		if (mAlpha < 0.001f)
		{
			return false;
		}
		if (mClipping == UIDrawCall.Clipping.None || mClipping == UIDrawCall.Clipping.ConstrainButDontClip)
		{
			return true;
		}
		UpdateTransformMatrix();
		Vector3 val = ((Matrix4x4)(ref worldToLocal)).MultiplyPoint3x4(worldPos);
		if (val.x < mMin.x)
		{
			return false;
		}
		if (val.y < mMin.y)
		{
			return false;
		}
		if (val.x > mMax.x)
		{
			return false;
		}
		if (val.y > mMax.y)
		{
			return false;
		}
		return true;
	}

	public bool IsVisible(UIWidget w)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		UIPanel uIPanel = this;
		Vector3[] array = null;
		while ((Object)(object)uIPanel != (Object)null)
		{
			if ((uIPanel.mClipping == UIDrawCall.Clipping.None || uIPanel.mClipping == UIDrawCall.Clipping.ConstrainButDontClip) && !w.hideIfOffScreen)
			{
				uIPanel = uIPanel.mParentPanel;
				continue;
			}
			if (array == null)
			{
				array = w.worldCorners;
			}
			if (!uIPanel.IsVisible(array[0], array[1], array[2], array[3]))
			{
				return false;
			}
			uIPanel = uIPanel.mParentPanel;
		}
		return true;
	}

	public bool Affects(UIWidget w)
	{
		if ((Object)(object)w == (Object)null)
		{
			return false;
		}
		UIPanel panel = w.panel;
		if ((Object)(object)panel == (Object)null)
		{
			return false;
		}
		UIPanel uIPanel = this;
		while ((Object)(object)uIPanel != (Object)null)
		{
			if ((Object)(object)uIPanel == (Object)(object)panel)
			{
				return true;
			}
			if (!uIPanel.hasCumulativeClipping)
			{
				return false;
			}
			uIPanel = uIPanel.mParentPanel;
		}
		return false;
	}

	[ContextMenu("Force Refresh")]
	public void RebuildAllDrawCalls()
	{
		mRebuild = true;
	}

	public void SetDirty()
	{
		int i = 0;
		for (int count = drawCalls.Count; i < count; i++)
		{
			drawCalls[i].isDirty = true;
		}
		Invalidate(includeChildren: true);
	}

	protected override void Awake()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Invalid comparison between Unknown and I4
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Invalid comparison between Unknown and I4
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Invalid comparison between Unknown and I4
		base.Awake();
		if ((int)Application.platform == 7)
		{
			mHalfPixelOffset = true;
		}
		else
		{
			mHalfPixelOffset = ((int)Application.platform == 2 || (int)Application.platform == 10) && SystemInfo.graphicsDeviceVersion.Contains("Direct3D") && SystemInfo.graphicsShaderLevel < 40;
		}
	}

	private void FindParent()
	{
		Transform val = base.cachedTransform.parent;
		mParentPanel = ((!((Object)(object)val != (Object)null)) ? null : NGUITools.FindInParents<UIPanel>(((Component)val).gameObject));
	}

	public override void ParentHasChanged()
	{
		base.ParentHasChanged();
		FindParent();
	}

	protected override void OnStart()
	{
		mLayer = base.cachedGameObject.layer;
	}

	protected override void OnEnable()
	{
		mRebuild = true;
		mAlphaFrameID = -1;
		mMatrixFrame = -1;
		OnStart();
		base.OnEnable();
		mMatrixFrame = -1;
	}

	protected override void OnInit()
	{
		if (list.Contains(this))
		{
			return;
		}
		base.OnInit();
		FindParent();
		if ((Object)(object)((Component)this).GetComponent<Rigidbody>() == (Object)null && (Object)(object)mParentPanel == (Object)null)
		{
			UICamera uICamera = ((!((Object)(object)base.anchorCamera != (Object)null)) ? null : ((Component)mCam).GetComponent<UICamera>());
			if ((Object)(object)uICamera != (Object)null && (uICamera.eventType == UICamera.EventType.UI_3D || uICamera.eventType == UICamera.EventType.World_3D))
			{
				Rigidbody val = ((Component)this).gameObject.AddComponent<Rigidbody>();
				val.isKinematic = true;
				val.useGravity = false;
			}
		}
		mRebuild = true;
		mAlphaFrameID = -1;
		mMatrixFrame = -1;
		list.Add(this);
		list.Sort(CompareFunc);
		if (onPanelAdded != null)
		{
			onPanelAdded(this);
		}
	}

	protected override void OnDisable()
	{
		int i = 0;
		for (int count = drawCalls.Count; i < count; i++)
		{
			UIDrawCall uIDrawCall = drawCalls[i];
			if ((Object)(object)uIDrawCall != (Object)null)
			{
				UIDrawCall.Destroy(uIDrawCall);
			}
		}
		drawCalls.Clear();
		list.Remove(this);
		mAlphaFrameID = -1;
		mMatrixFrame = -1;
		if (list.Count == 0)
		{
			UIDrawCall.ReleaseAll();
			mUpdateFrame = -1;
		}
		base.OnDisable();
		if (onDrawcallListUpdated != null)
		{
			onDrawcallListUpdated();
		}
		if (onPanelRemoved != null)
		{
			onPanelRemoved(this);
		}
	}

	private void UpdateTransformMatrix()
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		int frameCount = Time.frameCount;
		if (base.cachedTransform.hasChanged)
		{
			mTrans.hasChanged = false;
			mMatrixFrame = -1;
		}
		if (mMatrixFrame != frameCount)
		{
			mMatrixFrame = frameCount;
			worldToLocal = mTrans.worldToLocalMatrix;
			Vector2 val = GetViewSize() * 0.5f;
			float num = mClipOffset.x + mClipRange.x;
			float num2 = mClipOffset.y + mClipRange.y;
			mMin.x = num - val.x;
			mMin.y = num2 - val.y;
			mMax.x = num + val.x;
			mMax.y = num2 + val.y;
		}
	}

	protected override void OnAnchor()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0271: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_058f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0473: Unknown result type (might be due to invalid IL or missing references)
		//IL_0478: Unknown result type (might be due to invalid IL or missing references)
		if (mClipping == UIDrawCall.Clipping.None)
		{
			return;
		}
		Transform val = base.cachedTransform;
		Transform val2 = val.parent;
		Vector2 viewSize = GetViewSize();
		Vector2 val3 = Vector2.op_Implicit(val.localPosition);
		float num;
		float num2;
		float num3;
		float num4;
		if ((Object)(object)leftAnchor.target == (Object)(object)bottomAnchor.target && (Object)(object)leftAnchor.target == (Object)(object)rightAnchor.target && (Object)(object)leftAnchor.target == (Object)(object)topAnchor.target)
		{
			Vector3[] sides = leftAnchor.GetSides(val2);
			if (sides != null)
			{
				num = NGUIMath.Lerp(sides[0].x, sides[2].x, leftAnchor.relative) + (float)leftAnchor.absolute;
				num2 = NGUIMath.Lerp(sides[0].x, sides[2].x, rightAnchor.relative) + (float)rightAnchor.absolute;
				num3 = NGUIMath.Lerp(sides[3].y, sides[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute;
				num4 = NGUIMath.Lerp(sides[3].y, sides[1].y, topAnchor.relative) + (float)topAnchor.absolute;
			}
			else
			{
				Vector2 val4 = Vector2.op_Implicit(GetLocalPos(leftAnchor, val2));
				num = val4.x + (float)leftAnchor.absolute;
				num3 = val4.y + (float)bottomAnchor.absolute;
				num2 = val4.x + (float)rightAnchor.absolute;
				num4 = val4.y + (float)topAnchor.absolute;
			}
		}
		else
		{
			if (Object.op_Implicit((Object)(object)leftAnchor.target))
			{
				Vector3[] sides2 = leftAnchor.GetSides(val2);
				num = ((sides2 == null) ? (GetLocalPos(leftAnchor, val2).x + (float)leftAnchor.absolute) : (NGUIMath.Lerp(sides2[0].x, sides2[2].x, leftAnchor.relative) + (float)leftAnchor.absolute));
			}
			else
			{
				num = mClipRange.x - 0.5f * viewSize.x;
			}
			if (Object.op_Implicit((Object)(object)rightAnchor.target))
			{
				Vector3[] sides3 = rightAnchor.GetSides(val2);
				num2 = ((sides3 == null) ? (GetLocalPos(rightAnchor, val2).x + (float)rightAnchor.absolute) : (NGUIMath.Lerp(sides3[0].x, sides3[2].x, rightAnchor.relative) + (float)rightAnchor.absolute));
			}
			else
			{
				num2 = mClipRange.x + 0.5f * viewSize.x;
			}
			if (Object.op_Implicit((Object)(object)bottomAnchor.target))
			{
				Vector3[] sides4 = bottomAnchor.GetSides(val2);
				num3 = ((sides4 == null) ? (GetLocalPos(bottomAnchor, val2).y + (float)bottomAnchor.absolute) : (NGUIMath.Lerp(sides4[3].y, sides4[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute));
			}
			else
			{
				num3 = mClipRange.y - 0.5f * viewSize.y;
			}
			if (Object.op_Implicit((Object)(object)topAnchor.target))
			{
				Vector3[] sides5 = topAnchor.GetSides(val2);
				num4 = ((sides5 == null) ? (GetLocalPos(topAnchor, val2).y + (float)topAnchor.absolute) : (NGUIMath.Lerp(sides5[3].y, sides5[1].y, topAnchor.relative) + (float)topAnchor.absolute));
			}
			else
			{
				num4 = mClipRange.y + 0.5f * viewSize.y;
			}
		}
		num -= val3.x + mClipOffset.x;
		num2 -= val3.x + mClipOffset.x;
		num3 -= val3.y + mClipOffset.y;
		num4 -= val3.y + mClipOffset.y;
		float num5 = Mathf.Lerp(num, num2, 0.5f);
		float num6 = Mathf.Lerp(num3, num4, 0.5f);
		float num7 = num2 - num;
		float num8 = num4 - num3;
		float num9 = Mathf.Max(2f, mClipSoftness.x);
		float num10 = Mathf.Max(2f, mClipSoftness.y);
		if (num7 < num9)
		{
			num7 = num9;
		}
		if (num8 < num10)
		{
			num8 = num10;
		}
		baseClipRegion = new Vector4(num5, num6, num7, num8);
	}

	private void LateUpdate()
	{
		if (mUpdateFrame == Time.frameCount)
		{
			return;
		}
		mUpdateFrame = Time.frameCount;
		int i = 0;
		for (int count = list.Count; i < count; i++)
		{
			list[i].UpdateSelf();
		}
		int num = 3000;
		int j = 0;
		for (int count2 = list.Count; j < count2; j++)
		{
			UIPanel uIPanel = list[j];
			if (uIPanel.renderQueue == RenderQueue.Automatic)
			{
				uIPanel.startingRenderQueue = num;
				uIPanel.UpdateDrawCalls();
				num += uIPanel.drawCalls.Count;
			}
			else if (uIPanel.renderQueue == RenderQueue.StartAt)
			{
				uIPanel.UpdateDrawCalls();
				if (uIPanel.drawCalls.Count != 0)
				{
					num = Mathf.Max(num, uIPanel.startingRenderQueue + uIPanel.drawCalls.Count);
				}
			}
			else
			{
				uIPanel.UpdateDrawCalls();
				if (uIPanel.drawCalls.Count != 0)
				{
					num = Mathf.Max(num, uIPanel.startingRenderQueue + 1);
				}
			}
		}
	}

	private void UpdateSelf()
	{
		UpdateTransformMatrix();
		UpdateLayers();
		UpdateWidgets();
		if (mRebuild)
		{
			mRebuild = false;
			FillAllDrawCalls();
		}
		else
		{
			bool flag = false;
			int num = 0;
			while (num < drawCalls.Count)
			{
				UIDrawCall uIDrawCall = drawCalls[num];
				if (uIDrawCall.isDirty && !FillDrawCall(uIDrawCall))
				{
					uIDrawCall.isDirty = false;
					UIDrawCall.Destroy(uIDrawCall);
					drawCalls.RemoveAt(num);
					flag = true;
				}
				else
				{
					uIDrawCall.isDirty = false;
					num++;
				}
			}
			if (flag && onDrawcallListUpdated != null)
			{
				onDrawcallListUpdated();
			}
		}
		if (mUpdateScroll)
		{
			mUpdateScroll = false;
			UIScrollView component = ((Component)this).GetComponent<UIScrollView>();
			if ((Object)(object)component != (Object)null)
			{
				component.UpdateScrollbars();
			}
		}
	}

	public void SortWidgets()
	{
		mSortWidgets = false;
		widgets.Sort(UIWidget.PanelCompareFunc);
	}

	private void FillAllDrawCalls()
	{
		for (int i = 0; i < drawCalls.Count; i++)
		{
			UIDrawCall.Destroy(drawCalls[i]);
		}
		drawCalls.Clear();
		Material val = null;
		Texture val2 = null;
		Shader val3 = null;
		UIDrawCall uIDrawCall = null;
		int num = 0;
		if (mSortWidgets)
		{
			SortWidgets();
		}
		for (int j = 0; j < widgets.Count; j++)
		{
			UIWidget uIWidget = widgets[j];
			if (uIWidget.isVisible && uIWidget.hasVertices)
			{
				Material material = uIWidget.material;
				Texture mainTexture = uIWidget.mainTexture;
				Shader shader = uIWidget.shader;
				if ((Object)(object)val != (Object)(object)material || (Object)(object)val2 != (Object)(object)mainTexture || (Object)(object)val3 != (Object)(object)shader)
				{
					if ((Object)(object)uIDrawCall != (Object)null && uIDrawCall.verts.size != 0)
					{
						drawCalls.Add(uIDrawCall);
						uIDrawCall.UpdateGeometry(num);
						uIDrawCall.onRender = mOnRender;
						mOnRender = null;
						num = 0;
						uIDrawCall = null;
					}
					val = material;
					val2 = mainTexture;
					val3 = shader;
				}
				if (!((Object)(object)val != (Object)null) && !((Object)(object)val3 != (Object)null) && !((Object)(object)val2 != (Object)null))
				{
					continue;
				}
				if ((Object)(object)uIDrawCall == (Object)null)
				{
					uIDrawCall = UIDrawCall.Create(this, val, val2, val3);
					uIDrawCall.depthStart = uIWidget.depth;
					uIDrawCall.depthEnd = uIDrawCall.depthStart;
					uIDrawCall.panel = this;
				}
				else
				{
					int num2 = uIWidget.depth;
					if (num2 < uIDrawCall.depthStart)
					{
						uIDrawCall.depthStart = num2;
					}
					if (num2 > uIDrawCall.depthEnd)
					{
						uIDrawCall.depthEnd = num2;
					}
				}
				uIWidget.drawCall = uIDrawCall;
				num++;
				if (generateNormals)
				{
					uIWidget.WriteToBuffers(uIDrawCall.verts, uIDrawCall.uvs, uIDrawCall.cols, uIDrawCall.norms, uIDrawCall.tans);
				}
				else
				{
					uIWidget.WriteToBuffers(uIDrawCall.verts, uIDrawCall.uvs, uIDrawCall.cols, null, null);
				}
				if (uIWidget.mOnRender != null)
				{
					if (mOnRender == null)
					{
						mOnRender = uIWidget.mOnRender;
					}
					else
					{
						mOnRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(mOnRender, uIWidget.mOnRender);
					}
				}
			}
			else
			{
				uIWidget.drawCall = null;
			}
		}
		if ((Object)(object)uIDrawCall != (Object)null && uIDrawCall.verts.size != 0)
		{
			drawCalls.Add(uIDrawCall);
			uIDrawCall.UpdateGeometry(num);
			uIDrawCall.onRender = mOnRender;
			mOnRender = null;
		}
		if (onDrawcallListUpdated != null)
		{
			onDrawcallListUpdated();
		}
	}

	public bool FillDrawCall(UIDrawCall dc)
	{
		if ((Object)(object)dc != (Object)null)
		{
			int num = 0;
			int num2 = 0;
			while (num2 < widgets.Count)
			{
				UIWidget uIWidget = widgets[num2];
				if ((Object)(object)uIWidget == (Object)null)
				{
					widgets.RemoveAt(num2);
					continue;
				}
				if ((Object)(object)uIWidget.drawCall == (Object)(object)dc)
				{
					if (uIWidget.isVisible && uIWidget.hasVertices)
					{
						num++;
						if (generateNormals)
						{
							uIWidget.WriteToBuffers(dc.verts, dc.uvs, dc.cols, dc.norms, dc.tans);
						}
						else
						{
							uIWidget.WriteToBuffers(dc.verts, dc.uvs, dc.cols, null, null);
						}
						if (uIWidget.mOnRender != null)
						{
							if (mOnRender == null)
							{
								mOnRender = uIWidget.mOnRender;
							}
							else
							{
								mOnRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(mOnRender, uIWidget.mOnRender);
							}
						}
					}
					else
					{
						uIWidget.drawCall = null;
					}
				}
				num2++;
			}
			if (dc.verts.size != 0)
			{
				dc.UpdateGeometry(num);
				dc.onRender = mOnRender;
				mOnRender = null;
				return true;
			}
		}
		return false;
	}

	private void UpdateDrawCalls()
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0178: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		Transform val = base.cachedTransform;
		bool flag = usedForUI;
		if (clipping != 0)
		{
			drawCallClipRange = finalClipRegion;
			ref Vector4 reference = ref drawCallClipRange;
			reference.z *= 0.5f;
			ref Vector4 reference2 = ref drawCallClipRange;
			reference2.w *= 0.5f;
		}
		else
		{
			drawCallClipRange = Vector4.zero;
		}
		int num = Screen.width;
		int num2 = Screen.height;
		if (drawCallClipRange.z == 0f)
		{
			drawCallClipRange.z = (float)num * 0.5f;
		}
		if (drawCallClipRange.w == 0f)
		{
			drawCallClipRange.w = (float)num2 * 0.5f;
		}
		if (halfPixelOffset)
		{
			ref Vector4 reference3 = ref drawCallClipRange;
			reference3.x -= 0.5f;
			ref Vector4 reference4 = ref drawCallClipRange;
			reference4.y += 0.5f;
		}
		Vector3 val3;
		if (flag)
		{
			Transform val2 = base.cachedTransform.parent;
			val3 = base.cachedTransform.localPosition;
			if (clipping != 0)
			{
				val3.x = Mathf.RoundToInt(val3.x);
				val3.y = Mathf.RoundToInt(val3.y);
			}
			if ((Object)(object)val2 != (Object)null)
			{
				val3 = val2.TransformPoint(val3);
			}
			val3 += drawCallOffset;
		}
		else
		{
			val3 = val.position;
		}
		Quaternion rotation = val.rotation;
		Vector3 lossyScale = val.lossyScale;
		for (int i = 0; i < drawCalls.Count; i++)
		{
			UIDrawCall uIDrawCall = drawCalls[i];
			Transform val4 = uIDrawCall.cachedTransform;
			val4.position = val3;
			val4.rotation = rotation;
			val4.localScale = lossyScale;
			uIDrawCall.renderQueue = ((renderQueue != RenderQueue.Explicit) ? (startingRenderQueue + i) : startingRenderQueue);
			uIDrawCall.alwaysOnScreen = alwaysOnScreen && (mClipping == UIDrawCall.Clipping.None || mClipping == UIDrawCall.Clipping.ConstrainButDontClip);
			uIDrawCall.sortingOrder = mSortingOrder;
			uIDrawCall.clipTexture = mClipTexture;
		}
	}

	private void UpdateLayers()
	{
		if (mLayer == base.cachedGameObject.layer)
		{
			return;
		}
		mLayer = mGo.layer;
		int i = 0;
		for (int count = widgets.Count; i < count; i++)
		{
			UIWidget uIWidget = widgets[i];
			if (Object.op_Implicit((Object)(object)uIWidget) && (Object)(object)uIWidget.parent == (Object)(object)this)
			{
				((Component)uIWidget).gameObject.layer = mLayer;
			}
		}
		ResetAnchors();
		for (int j = 0; j < drawCalls.Count; j++)
		{
			((Component)drawCalls[j]).gameObject.layer = mLayer;
		}
	}

	private void UpdateWidgets()
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = hasCumulativeClipping;
		if (!cullWhileDragging)
		{
			for (int i = 0; i < UIScrollView.list.size; i++)
			{
				UIScrollView uIScrollView = UIScrollView.list[i];
				if ((Object)(object)uIScrollView.panel == (Object)(object)this && uIScrollView.isDragging)
				{
					flag2 = true;
				}
			}
		}
		if (mForced != flag2)
		{
			mForced = flag2;
			mResized = true;
		}
		int frameCount = Time.frameCount;
		int j = 0;
		for (int count = widgets.Count; j < count; j++)
		{
			UIWidget uIWidget = widgets[j];
			if (!((Object)(object)uIWidget.panel == (Object)(object)this) || !((Behaviour)uIWidget).enabled)
			{
				continue;
			}
			if (uIWidget.UpdateTransform(frameCount) || mResized)
			{
				bool visibleByAlpha = flag2 || uIWidget.CalculateCumulativeAlpha(frameCount) > 0.001f;
				uIWidget.UpdateVisibility(visibleByAlpha, flag2 || (!flag3 && !uIWidget.hideIfOffScreen) || IsVisible(uIWidget));
			}
			if (!uIWidget.UpdateGeometry(frameCount))
			{
				continue;
			}
			flag = true;
			if (!mRebuild)
			{
				if ((Object)(object)uIWidget.drawCall != (Object)null)
				{
					uIWidget.drawCall.isDirty = true;
				}
				else
				{
					FindDrawCall(uIWidget);
				}
			}
		}
		if (flag && onGeometryUpdated != null)
		{
			onGeometryUpdated();
		}
		mResized = false;
	}

	public UIDrawCall FindDrawCall(UIWidget w)
	{
		Material material = w.material;
		Texture mainTexture = w.mainTexture;
		int num = w.depth;
		for (int i = 0; i < drawCalls.Count; i++)
		{
			UIDrawCall uIDrawCall = drawCalls[i];
			int num2 = ((i != 0) ? (drawCalls[i - 1].depthEnd + 1) : int.MinValue);
			int num3 = ((i + 1 != drawCalls.Count) ? (drawCalls[i + 1].depthStart - 1) : int.MaxValue);
			if (num2 > num || num3 < num)
			{
				continue;
			}
			if ((Object)(object)uIDrawCall.baseMaterial == (Object)(object)material && (Object)(object)uIDrawCall.mainTexture == (Object)(object)mainTexture)
			{
				if (w.isVisible)
				{
					w.drawCall = uIDrawCall;
					if (w.hasVertices)
					{
						uIDrawCall.isDirty = true;
					}
					return uIDrawCall;
				}
			}
			else
			{
				mRebuild = true;
			}
			return null;
		}
		mRebuild = true;
		return null;
	}

	public void AddWidget(UIWidget w)
	{
		mUpdateScroll = true;
		if (widgets.Count == 0)
		{
			widgets.Add(w);
		}
		else if (mSortWidgets)
		{
			widgets.Add(w);
			SortWidgets();
		}
		else if (UIWidget.PanelCompareFunc(w, widgets[0]) == -1)
		{
			widgets.Insert(0, w);
		}
		else
		{
			int num = widgets.Count;
			while (num > 0)
			{
				if (UIWidget.PanelCompareFunc(w, widgets[--num]) == -1)
				{
					continue;
				}
				widgets.Insert(num + 1, w);
				break;
			}
		}
		FindDrawCall(w);
	}

	public void RemoveWidget(UIWidget w)
	{
		if (widgets.Remove(w) && (Object)(object)w.drawCall != (Object)null)
		{
			int num = w.depth;
			if (num == w.drawCall.depthStart || num == w.drawCall.depthEnd)
			{
				mRebuild = true;
			}
			w.drawCall.isDirty = true;
			w.drawCall = null;
		}
	}

	public void Refresh()
	{
		mRebuild = true;
		mUpdateFrame = -1;
		if (list.Count > 0)
		{
			list[0].LateUpdate();
		}
	}

	public virtual Vector3 CalculateConstrainOffset(Vector2 min, Vector2 max)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = finalClipRegion;
		float num = val.z * 0.5f;
		float num2 = val.w * 0.5f;
		Vector2 minRect = default(Vector2);
		((Vector2)(ref minRect))._002Ector(min.x, min.y);
		Vector2 maxRect = default(Vector2);
		((Vector2)(ref maxRect))._002Ector(max.x, max.y);
		Vector2 minArea = default(Vector2);
		((Vector2)(ref minArea))._002Ector(val.x - num, val.y - num2);
		Vector2 maxArea = default(Vector2);
		((Vector2)(ref maxArea))._002Ector(val.x + num, val.y + num2);
		if (softBorderPadding && clipping == UIDrawCall.Clipping.SoftClip)
		{
			minArea.x += mClipSoftness.x;
			minArea.y += mClipSoftness.y;
			maxArea.x -= mClipSoftness.x;
			maxArea.y -= mClipSoftness.y;
		}
		return Vector2.op_Implicit(NGUIMath.ConstrainRect(minRect, maxRect, minArea, maxArea));
	}

	public bool ConstrainTargetToBounds(Transform target, ref Bounds targetBounds, bool immediate)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ((Bounds)(ref targetBounds)).min;
		Vector3 val2 = ((Bounds)(ref targetBounds)).max;
		float num = 1f;
		if (mClipping == UIDrawCall.Clipping.None)
		{
			UIRoot uIRoot = base.root;
			if ((Object)(object)uIRoot != (Object)null)
			{
				num = uIRoot.pixelSizeAdjustment;
			}
		}
		if (num != 1f)
		{
			val /= num;
			val2 /= num;
		}
		Vector3 val3 = CalculateConstrainOffset(Vector2.op_Implicit(val), Vector2.op_Implicit(val2)) * num;
		if (((Vector3)(ref val3)).sqrMagnitude > 0f)
		{
			if (immediate)
			{
				target.localPosition += val3;
				((Bounds)(ref targetBounds)).center = ((Bounds)(ref targetBounds)).center + val3;
				SpringPosition component = ((Component)target).GetComponent<SpringPosition>();
				if ((Object)(object)component != (Object)null)
				{
					((Behaviour)component).enabled = false;
				}
			}
			else
			{
				SpringPosition springPosition = SpringPosition.Begin(((Component)target).gameObject, target.localPosition + val3, 13f);
				springPosition.ignoreTimeScale = true;
				springPosition.worldSpace = false;
			}
			return true;
		}
		return false;
	}

	public bool ConstrainTargetToBounds(Transform target, bool immediate)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Bounds targetBounds = NGUIMath.CalculateRelativeWidgetBounds(base.cachedTransform, target);
		return ConstrainTargetToBounds(target, ref targetBounds, immediate);
	}

	public static UIPanel Find(Transform trans)
	{
		return Find(trans, createIfMissing: false, -1);
	}

	public static UIPanel Find(Transform trans, bool createIfMissing)
	{
		return Find(trans, createIfMissing, -1);
	}

	public static UIPanel Find(Transform trans, bool createIfMissing, int layer)
	{
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(trans);
		if ((Object)(object)uIPanel != (Object)null)
		{
			return uIPanel;
		}
		while ((Object)(object)trans.parent != (Object)null)
		{
			trans = trans.parent;
		}
		return (!createIfMissing) ? null : NGUITools.CreateUI(trans, advanced3D: false, layer);
	}

	public Vector2 GetWindowSize()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		UIRoot uIRoot = base.root;
		Vector2 val = NGUITools.screenSize;
		if ((Object)(object)uIRoot != (Object)null)
		{
			val *= uIRoot.GetPixelSizeAdjustment(Mathf.RoundToInt(val.y));
		}
		return val;
	}

	public Vector2 GetViewSize()
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		if (mClipping != 0)
		{
			return new Vector2(mClipRange.z, mClipRange.w);
		}
		return NGUITools.screenSize;
	}
}
