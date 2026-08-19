using System;
using System.Diagnostics;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Widget")]
public class UIWidget : UIRect
{
	public enum Pivot
	{
		TopLeft,
		Top,
		TopRight,
		Left,
		Center,
		Right,
		BottomLeft,
		Bottom,
		BottomRight
	}

	public enum AspectRatioSource
	{
		Free,
		BasedOnWidth,
		BasedOnHeight
	}

	public delegate void OnDimensionsChanged();

	public delegate void OnPostFillCallback(UIWidget widget, int bufferOffset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols);

	public delegate bool HitCheck(Vector3 worldPos);

	[HideInInspector]
	[SerializeField]
	protected Color mColor = Color.white;

	[HideInInspector]
	[SerializeField]
	protected Pivot mPivot = Pivot.Center;

	[HideInInspector]
	[SerializeField]
	protected int mWidth = 100;

	[HideInInspector]
	[SerializeField]
	protected int mHeight = 100;

	[SerializeField]
	[HideInInspector]
	protected int mDepth;

	public OnDimensionsChanged onChange;

	public OnPostFillCallback onPostFill;

	public UIDrawCall.OnRenderCallback mOnRender;

	public bool autoResizeBoxCollider;

	public bool hideIfOffScreen;

	public AspectRatioSource keepAspectRatio;

	public float aspectRatio = 1f;

	public HitCheck hitCheck;

	[NonSerialized]
	public UIPanel panel;

	[NonSerialized]
	public UIGeometry geometry = new UIGeometry();

	[NonSerialized]
	public bool fillGeometry = true;

	[NonSerialized]
	protected bool mPlayMode = true;

	[NonSerialized]
	protected Vector4 mDrawRegion = new Vector4(0f, 0f, 1f, 1f);

	[NonSerialized]
	private Matrix4x4 mLocalToPanel;

	[NonSerialized]
	private bool mIsVisibleByAlpha = true;

	[NonSerialized]
	private bool mIsVisibleByPanel = true;

	[NonSerialized]
	private bool mIsInFront = true;

	[NonSerialized]
	private float mLastAlpha;

	[NonSerialized]
	private bool mMoved;

	[NonSerialized]
	public UIDrawCall drawCall;

	[NonSerialized]
	protected Vector3[] mCorners = (Vector3[])(object)new Vector3[4];

	[NonSerialized]
	private int mAlphaFrameID = -1;

	private int mMatrixFrame = -1;

	private Vector3 mOldV0;

	private Vector3 mOldV1;

	public UIDrawCall.OnRenderCallback onRender
	{
		get
		{
			return mOnRender;
		}
		set
		{
			if (mOnRender != value)
			{
				if ((Object)(object)drawCall != (Object)null && drawCall.onRender != null && mOnRender != null)
				{
					UIDrawCall uIDrawCall = drawCall;
					uIDrawCall.onRender = (UIDrawCall.OnRenderCallback)Delegate.Remove(uIDrawCall.onRender, mOnRender);
				}
				mOnRender = value;
				if ((Object)(object)drawCall != (Object)null)
				{
					UIDrawCall uIDrawCall2 = drawCall;
					uIDrawCall2.onRender = (UIDrawCall.OnRenderCallback)Delegate.Combine(uIDrawCall2.onRender, value);
				}
			}
		}
	}

	public Vector4 drawRegion
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mDrawRegion;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (mDrawRegion != value)
			{
				mDrawRegion = value;
				if (autoResizeBoxCollider)
				{
					ResizeCollider();
				}
				MarkAsChanged();
			}
		}
	}

	public Vector2 pivotOffset => NGUIMath.GetPivotOffset(pivot);

	public int width
	{
		get
		{
			return mWidth;
		}
		set
		{
			int num = minWidth;
			if (value < num)
			{
				value = num;
			}
			if (mWidth == value || keepAspectRatio == AspectRatioSource.BasedOnHeight)
			{
				return;
			}
			if (isAnchoredHorizontally)
			{
				if ((Object)(object)leftAnchor.target != (Object)null && (Object)(object)rightAnchor.target != (Object)null)
				{
					if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Left || mPivot == Pivot.TopLeft)
					{
						NGUIMath.AdjustWidget(this, 0f, 0f, value - mWidth, 0f);
						return;
					}
					if (mPivot == Pivot.BottomRight || mPivot == Pivot.Right || mPivot == Pivot.TopRight)
					{
						NGUIMath.AdjustWidget(this, mWidth - value, 0f, 0f, 0f);
						return;
					}
					int num2 = value - mWidth;
					num2 -= num2 & 1;
					if (num2 != 0)
					{
						NGUIMath.AdjustWidget(this, (float)(-num2) * 0.5f, 0f, (float)num2 * 0.5f, 0f);
					}
				}
				else if ((Object)(object)leftAnchor.target != (Object)null)
				{
					NGUIMath.AdjustWidget(this, 0f, 0f, value - mWidth, 0f);
				}
				else
				{
					NGUIMath.AdjustWidget(this, mWidth - value, 0f, 0f, 0f);
				}
			}
			else
			{
				SetDimensions(value, mHeight);
			}
		}
	}

	public int height
	{
		get
		{
			return mHeight;
		}
		set
		{
			int num = minHeight;
			if (value < num)
			{
				value = num;
			}
			if (mHeight == value || keepAspectRatio == AspectRatioSource.BasedOnWidth)
			{
				return;
			}
			if (isAnchoredVertically)
			{
				if ((Object)(object)bottomAnchor.target != (Object)null && (Object)(object)topAnchor.target != (Object)null)
				{
					if (mPivot == Pivot.BottomLeft || mPivot == Pivot.Bottom || mPivot == Pivot.BottomRight)
					{
						NGUIMath.AdjustWidget(this, 0f, 0f, 0f, value - mHeight);
						return;
					}
					if (mPivot == Pivot.TopLeft || mPivot == Pivot.Top || mPivot == Pivot.TopRight)
					{
						NGUIMath.AdjustWidget(this, 0f, mHeight - value, 0f, 0f);
						return;
					}
					int num2 = value - mHeight;
					num2 -= num2 & 1;
					if (num2 != 0)
					{
						NGUIMath.AdjustWidget(this, 0f, (float)(-num2) * 0.5f, 0f, (float)num2 * 0.5f);
					}
				}
				else if ((Object)(object)bottomAnchor.target != (Object)null)
				{
					NGUIMath.AdjustWidget(this, 0f, 0f, 0f, value - mHeight);
				}
				else
				{
					NGUIMath.AdjustWidget(this, 0f, mHeight - value, 0f, 0f);
				}
			}
			else
			{
				SetDimensions(mWidth, value);
			}
		}
	}

	public Color color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			if (mColor != value)
			{
				bool includeChildren = mColor.a != value.a;
				mColor = value;
				Invalidate(includeChildren);
			}
		}
	}

	public override float alpha
	{
		get
		{
			return mColor.a;
		}
		set
		{
			if (mColor.a != value)
			{
				mColor.a = value;
				Invalidate(includeChildren: true);
			}
		}
	}

	public bool isVisible => mIsVisibleByPanel && mIsVisibleByAlpha && mIsInFront && finalAlpha > 0.001f && NGUITools.GetActive((Behaviour)(object)this);

	public bool hasVertices => geometry != null && geometry.hasVertices;

	public Pivot rawPivot
	{
		get
		{
			return mPivot;
		}
		set
		{
			if (mPivot != value)
			{
				mPivot = value;
				if (autoResizeBoxCollider)
				{
					ResizeCollider();
				}
				MarkAsChanged();
			}
		}
	}

	public Pivot pivot
	{
		get
		{
			return mPivot;
		}
		set
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0052: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			if (mPivot != value)
			{
				Vector3 val = worldCorners[0];
				mPivot = value;
				mChanged = true;
				Vector3 val2 = worldCorners[0];
				Transform val3 = base.cachedTransform;
				Vector3 position = val3.position;
				float z = val3.localPosition.z;
				position.x += val.x - val2.x;
				position.y += val.y - val2.y;
				base.cachedTransform.position = position;
				position = base.cachedTransform.localPosition;
				position.x = Mathf.Round(position.x);
				position.y = Mathf.Round(position.y);
				position.z = z;
				base.cachedTransform.localPosition = position;
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
			if (mDepth == value)
			{
				return;
			}
			if ((Object)(object)panel != (Object)null)
			{
				panel.RemoveWidget(this);
			}
			mDepth = value;
			if ((Object)(object)panel != (Object)null)
			{
				panel.AddWidget(this);
				if (!Application.isPlaying)
				{
					panel.SortWidgets();
					panel.RebuildAllDrawCalls();
				}
			}
		}
	}

	public int raycastDepth
	{
		get
		{
			if ((Object)(object)panel == (Object)null)
			{
				CreatePanel();
			}
			return (!((Object)(object)panel != (Object)null)) ? mDepth : (mDepth + panel.depth * 1000);
		}
	}

	public override Vector3[] localCorners
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_007e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = pivotOffset;
			float num = (0f - val.x) * (float)mWidth;
			float num2 = (0f - val.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			ref Vector3 reference = ref mCorners[0];
			reference = new Vector3(num, num2);
			ref Vector3 reference2 = ref mCorners[1];
			reference2 = new Vector3(num, num4);
			ref Vector3 reference3 = ref mCorners[2];
			reference3 = new Vector3(num3, num4);
			ref Vector3 reference4 = ref mCorners[3];
			reference4 = new Vector3(num3, num2);
			return mCorners;
		}
	}

	public virtual Vector2 localSize
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			Vector3[] array = localCorners;
			return Vector2.op_Implicit(array[2] - array[0]);
		}
	}

	public Vector3 localCenter
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			Vector3[] array = localCorners;
			return Vector3.Lerp(array[0], array[2], 0.5f);
		}
	}

	public override Vector3[] worldCorners
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = pivotOffset;
			float num = (0f - val.x) * (float)mWidth;
			float num2 = (0f - val.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			Transform val2 = base.cachedTransform;
			ref Vector3 reference = ref mCorners[0];
			reference = val2.TransformPoint(num, num2, 0f);
			ref Vector3 reference2 = ref mCorners[1];
			reference2 = val2.TransformPoint(num, num4, 0f);
			ref Vector3 reference3 = ref mCorners[2];
			reference3 = val2.TransformPoint(num3, num4, 0f);
			ref Vector3 reference4 = ref mCorners[3];
			reference4 = val2.TransformPoint(num3, num2, 0f);
			return mCorners;
		}
	}

	public Vector3 worldCenter => base.cachedTransform.TransformPoint(localCenter);

	public virtual Vector4 drawingDimensions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = pivotOffset;
			float num = (0f - val.x) * (float)mWidth;
			float num2 = (0f - val.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			return new Vector4((mDrawRegion.x != 0f) ? Mathf.Lerp(num, num3, mDrawRegion.x) : num, (mDrawRegion.y != 0f) ? Mathf.Lerp(num2, num4, mDrawRegion.y) : num2, (mDrawRegion.z != 1f) ? Mathf.Lerp(num, num3, mDrawRegion.z) : num3, (mDrawRegion.w != 1f) ? Mathf.Lerp(num2, num4, mDrawRegion.w) : num4);
		}
	}

	public virtual Material material
	{
		get
		{
			return null;
		}
		set
		{
			throw new NotImplementedException(string.Concat(((object)this).GetType(), " has no material setter"));
		}
	}

	public virtual Texture mainTexture
	{
		get
		{
			Material val = material;
			return (!((Object)(object)val != (Object)null)) ? null : val.mainTexture;
		}
		set
		{
			throw new NotImplementedException(string.Concat(((object)this).GetType(), " has no mainTexture setter"));
		}
	}

	public virtual Shader shader
	{
		get
		{
			Material val = material;
			return (!((Object)(object)val != (Object)null)) ? null : val.shader;
		}
		set
		{
			throw new NotImplementedException(string.Concat(((object)this).GetType(), " has no shader setter"));
		}
	}

	[Obsolete("There is no relative scale anymore. Widgets now have width and height instead")]
	public Vector2 relativeSize => Vector2.one;

	public bool hasBoxCollider
	{
		get
		{
			Collider component = ((Component)this).GetComponent<Collider>();
			BoxCollider val = (BoxCollider)(object)((component is BoxCollider) ? component : null);
			if ((Object)(object)val != (Object)null)
			{
				return true;
			}
			return (Object)(object)((Component)this).GetComponent<BoxCollider2D>() != (Object)null;
		}
	}

	public virtual int minWidth => 2;

	public virtual int minHeight => 2;

	public virtual Vector4 border
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Vector4.zero;
		}
		set
		{
		}
	}

	public void SetDimensions(int w, int h)
	{
		if (mWidth != w || mHeight != h)
		{
			mWidth = w;
			mHeight = h;
			if (keepAspectRatio == AspectRatioSource.BasedOnWidth)
			{
				mHeight = Mathf.RoundToInt((float)mWidth / aspectRatio);
			}
			else if (keepAspectRatio == AspectRatioSource.BasedOnHeight)
			{
				mWidth = Mathf.RoundToInt((float)mHeight * aspectRatio);
			}
			else if (keepAspectRatio == AspectRatioSource.Free)
			{
				aspectRatio = (float)mWidth / (float)mHeight;
			}
			mMoved = true;
			if (autoResizeBoxCollider)
			{
				ResizeCollider();
			}
			MarkAsChanged();
		}
	}

	public override Vector3[] GetSides(Transform relativeTo)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = pivotOffset;
		float num = (0f - val.x) * (float)mWidth;
		float num2 = (0f - val.y) * (float)mHeight;
		float num3 = num + (float)mWidth;
		float num4 = num2 + (float)mHeight;
		float num5 = (num + num3) * 0.5f;
		float num6 = (num2 + num4) * 0.5f;
		Transform val2 = base.cachedTransform;
		ref Vector3 reference = ref mCorners[0];
		reference = val2.TransformPoint(num, num6, 0f);
		ref Vector3 reference2 = ref mCorners[1];
		reference2 = val2.TransformPoint(num5, num4, 0f);
		ref Vector3 reference3 = ref mCorners[2];
		reference3 = val2.TransformPoint(num3, num6, 0f);
		ref Vector3 reference4 = ref mCorners[3];
		reference4 = val2.TransformPoint(num5, num2, 0f);
		if ((Object)(object)relativeTo != (Object)null)
		{
			for (int i = 0; i < 4; i++)
			{
				ref Vector3 reference5 = ref mCorners[i];
				reference5 = relativeTo.InverseTransformPoint(mCorners[i]);
			}
		}
		return mCorners;
	}

	public override float CalculateFinalAlpha(int frameID)
	{
		if (mAlphaFrameID != frameID)
		{
			mAlphaFrameID = frameID;
			UpdateFinalAlpha(frameID);
		}
		return finalAlpha;
	}

	protected void UpdateFinalAlpha(int frameID)
	{
		if (!mIsVisibleByAlpha || !mIsInFront)
		{
			finalAlpha = 0f;
			return;
		}
		UIRect uIRect = base.parent;
		finalAlpha = ((!((Object)(object)uIRect != (Object)null)) ? mColor.a : (uIRect.CalculateFinalAlpha(frameID) * mColor.a));
	}

	public override void Invalidate(bool includeChildren)
	{
		mChanged = true;
		mAlphaFrameID = -1;
		if ((Object)(object)panel != (Object)null)
		{
			bool visibleByPanel = (!hideIfOffScreen && !panel.hasCumulativeClipping) || panel.IsVisible(this);
			UpdateVisibility(CalculateCumulativeAlpha(Time.frameCount) > 0.001f, visibleByPanel);
			UpdateFinalAlpha(Time.frameCount);
			if (includeChildren)
			{
				base.Invalidate(includeChildren: true);
			}
		}
	}

	public float CalculateCumulativeAlpha(int frameID)
	{
		UIRect uIRect = base.parent;
		return (!((Object)(object)uIRect != (Object)null)) ? mColor.a : (uIRect.CalculateFinalAlpha(frameID) * mColor.a);
	}

	public override void SetRect(float x, float y, float width, float height)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = pivotOffset;
		float num = Mathf.Lerp(x, x + width, val.x);
		float num2 = Mathf.Lerp(y, y + height, val.y);
		int num3 = Mathf.FloorToInt(width + 0.5f);
		int num4 = Mathf.FloorToInt(height + 0.5f);
		if (val.x == 0.5f)
		{
			num3 = num3 >> 1 << 1;
		}
		if (val.y == 0.5f)
		{
			num4 = num4 >> 1 << 1;
		}
		Transform val2 = base.cachedTransform;
		Vector3 localPosition = val2.localPosition;
		localPosition.x = Mathf.Floor(num + 0.5f);
		localPosition.y = Mathf.Floor(num2 + 0.5f);
		if (num3 < minWidth)
		{
			num3 = minWidth;
		}
		if (num4 < minHeight)
		{
			num4 = minHeight;
		}
		val2.localPosition = localPosition;
		this.width = num3;
		this.height = num4;
		if (base.isAnchored)
		{
			val2 = val2.parent;
			if (Object.op_Implicit((Object)(object)leftAnchor.target))
			{
				leftAnchor.SetHorizontal(val2, x);
			}
			if (Object.op_Implicit((Object)(object)rightAnchor.target))
			{
				rightAnchor.SetHorizontal(val2, x + width);
			}
			if (Object.op_Implicit((Object)(object)bottomAnchor.target))
			{
				bottomAnchor.SetVertical(val2, y);
			}
			if (Object.op_Implicit((Object)(object)topAnchor.target))
			{
				topAnchor.SetVertical(val2, y + height);
			}
		}
	}

	public void ResizeCollider()
	{
		if (NGUITools.GetActive((Behaviour)(object)this))
		{
			NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
		}
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static int FullCompareFunc(UIWidget left, UIWidget right)
	{
		int num = UIPanel.CompareFunc(left.panel, right.panel);
		return (num != 0) ? num : PanelCompareFunc(left, right);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static int PanelCompareFunc(UIWidget left, UIWidget right)
	{
		if (left.mDepth < right.mDepth)
		{
			return -1;
		}
		if (left.mDepth > right.mDepth)
		{
			return 1;
		}
		Material val = left.material;
		Material val2 = right.material;
		if ((Object)(object)val == (Object)(object)val2)
		{
			return 0;
		}
		if ((Object)(object)val == (Object)null)
		{
			return 1;
		}
		if ((Object)(object)val2 == (Object)null)
		{
			return -1;
		}
		return (((Object)val).GetInstanceID() >= ((Object)val2).GetInstanceID()) ? 1 : (-1);
	}

	public Bounds CalculateBounds()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return CalculateBounds(null);
	}

	public Bounds CalculateBounds(Transform relativeParent)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)relativeParent == (Object)null)
		{
			Vector3[] array = localCorners;
			Bounds result = default(Bounds);
			((Bounds)(ref result))._002Ector(array[0], Vector3.zero);
			for (int i = 1; i < 4; i++)
			{
				((Bounds)(ref result)).Encapsulate(array[i]);
			}
			return result;
		}
		Matrix4x4 worldToLocalMatrix = relativeParent.worldToLocalMatrix;
		Vector3[] array2 = worldCorners;
		Bounds result2 = default(Bounds);
		((Bounds)(ref result2))._002Ector(((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(array2[0]), Vector3.zero);
		for (int j = 1; j < 4; j++)
		{
			((Bounds)(ref result2)).Encapsulate(((Matrix4x4)(ref worldToLocalMatrix)).MultiplyPoint3x4(array2[j]));
		}
		return result2;
	}

	public void SetDirty()
	{
		if ((Object)(object)drawCall != (Object)null)
		{
			drawCall.isDirty = true;
		}
		else if (isVisible && hasVertices)
		{
			CreatePanel();
		}
	}

	public void RemoveFromPanel()
	{
		if ((Object)(object)panel != (Object)null)
		{
			panel.RemoveWidget(this);
			panel = null;
		}
		drawCall = null;
	}

	public virtual void MarkAsChanged()
	{
		if (NGUITools.GetActive((Behaviour)(object)this))
		{
			mChanged = true;
			if ((Object)(object)panel != (Object)null && ((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject) && !mPlayMode)
			{
				SetDirty();
				CheckLayer();
			}
		}
	}

	public UIPanel CreatePanel()
	{
		if (mStarted && (Object)(object)panel == (Object)null && ((Behaviour)this).enabled && NGUITools.GetActive(((Component)this).gameObject))
		{
			panel = UIPanel.Find(base.cachedTransform, createIfMissing: true, base.cachedGameObject.layer);
			if ((Object)(object)panel != (Object)null)
			{
				mParentFound = false;
				panel.AddWidget(this);
				CheckLayer();
				Invalidate(includeChildren: true);
			}
		}
		return panel;
	}

	public void CheckLayer()
	{
		if ((Object)(object)panel != (Object)null && ((Component)panel).gameObject.layer != ((Component)this).gameObject.layer)
		{
			((Component)this).gameObject.layer = ((Component)panel).gameObject.layer;
		}
	}

	public override void ParentHasChanged()
	{
		base.ParentHasChanged();
		if ((Object)(object)panel != (Object)null)
		{
			UIPanel uIPanel = UIPanel.Find(base.cachedTransform, createIfMissing: true, base.cachedGameObject.layer);
			if ((Object)(object)panel != (Object)(object)uIPanel)
			{
				RemoveFromPanel();
				CreatePanel();
			}
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mPlayMode = Application.isPlaying;
	}

	protected override void OnInit()
	{
		base.OnInit();
		RemoveFromPanel();
		mMoved = true;
		Update();
	}

	protected virtual void UpgradeFrom265()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localScale = base.cachedTransform.localScale;
		mWidth = Mathf.Abs(Mathf.RoundToInt(localScale.x));
		mHeight = Mathf.Abs(Mathf.RoundToInt(localScale.y));
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject, considerInactive: true);
	}

	protected override void OnStart()
	{
		CreatePanel();
	}

	protected override void OnAnchor()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0339: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d7: Unknown result type (might be due to invalid IL or missing references)
		Transform val = base.cachedTransform;
		Transform val2 = val.parent;
		Vector3 localPosition = val.localPosition;
		Vector2 val3 = pivotOffset;
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
				mIsInFront = true;
			}
			else
			{
				Vector3 localPos = GetLocalPos(leftAnchor, val2);
				num = localPos.x + (float)leftAnchor.absolute;
				num3 = localPos.y + (float)bottomAnchor.absolute;
				num2 = localPos.x + (float)rightAnchor.absolute;
				num4 = localPos.y + (float)topAnchor.absolute;
				mIsInFront = !hideIfOffScreen || localPos.z >= 0f;
			}
		}
		else
		{
			mIsInFront = true;
			if (Object.op_Implicit((Object)(object)leftAnchor.target))
			{
				Vector3[] sides2 = leftAnchor.GetSides(val2);
				num = ((sides2 == null) ? (GetLocalPos(leftAnchor, val2).x + (float)leftAnchor.absolute) : (NGUIMath.Lerp(sides2[0].x, sides2[2].x, leftAnchor.relative) + (float)leftAnchor.absolute));
			}
			else
			{
				num = localPosition.x - val3.x * (float)mWidth;
			}
			if (Object.op_Implicit((Object)(object)rightAnchor.target))
			{
				Vector3[] sides3 = rightAnchor.GetSides(val2);
				num2 = ((sides3 == null) ? (GetLocalPos(rightAnchor, val2).x + (float)rightAnchor.absolute) : (NGUIMath.Lerp(sides3[0].x, sides3[2].x, rightAnchor.relative) + (float)rightAnchor.absolute));
			}
			else
			{
				num2 = localPosition.x - val3.x * (float)mWidth + (float)mWidth;
			}
			if (Object.op_Implicit((Object)(object)bottomAnchor.target))
			{
				Vector3[] sides4 = bottomAnchor.GetSides(val2);
				num3 = ((sides4 == null) ? (GetLocalPos(bottomAnchor, val2).y + (float)bottomAnchor.absolute) : (NGUIMath.Lerp(sides4[3].y, sides4[1].y, bottomAnchor.relative) + (float)bottomAnchor.absolute));
			}
			else
			{
				num3 = localPosition.y - val3.y * (float)mHeight;
			}
			if (Object.op_Implicit((Object)(object)topAnchor.target))
			{
				Vector3[] sides5 = topAnchor.GetSides(val2);
				num4 = ((sides5 == null) ? (GetLocalPos(topAnchor, val2).y + (float)topAnchor.absolute) : (NGUIMath.Lerp(sides5[3].y, sides5[1].y, topAnchor.relative) + (float)topAnchor.absolute));
			}
			else
			{
				num4 = localPosition.y - val3.y * (float)mHeight + (float)mHeight;
			}
		}
		Vector3 val4 = default(Vector3);
		((Vector3)(ref val4))._002Ector(Mathf.Lerp(num, num2, val3.x), Mathf.Lerp(num3, num4, val3.y), localPosition.z);
		val4.x = Mathf.Round(val4.x);
		val4.y = Mathf.Round(val4.y);
		int num5 = Mathf.FloorToInt(num2 - num + 0.5f);
		int num6 = Mathf.FloorToInt(num4 - num3 + 0.5f);
		if (keepAspectRatio != 0 && aspectRatio != 0f)
		{
			if (keepAspectRatio == AspectRatioSource.BasedOnHeight)
			{
				num5 = Mathf.RoundToInt((float)num6 * aspectRatio);
			}
			else
			{
				num6 = Mathf.RoundToInt((float)num5 / aspectRatio);
			}
		}
		if (num5 < minWidth)
		{
			num5 = minWidth;
		}
		if (num6 < minHeight)
		{
			num6 = minHeight;
		}
		if (Vector3.SqrMagnitude(localPosition - val4) > 0.001f)
		{
			base.cachedTransform.localPosition = val4;
			if (mIsInFront)
			{
				mChanged = true;
			}
		}
		if (mWidth != num5 || mHeight != num6)
		{
			mWidth = num5;
			mHeight = num6;
			if (mIsInFront)
			{
				mChanged = true;
			}
			if (autoResizeBoxCollider)
			{
				NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
			}
		}
	}

	protected override void OnUpdate()
	{
		if ((Object)(object)panel == (Object)null)
		{
			CreatePanel();
		}
	}

	private void OnApplicationPause(bool paused)
	{
		if (!paused)
		{
			MarkAsChanged();
		}
	}

	protected override void OnDisable()
	{
		RemoveFromPanel();
		base.OnDisable();
	}

	private void OnDestroy()
	{
		RemoveFromPanel();
	}

	public bool UpdateVisibility(bool visibleByAlpha, bool visibleByPanel)
	{
		if (mIsVisibleByAlpha != visibleByAlpha || mIsVisibleByPanel != visibleByPanel)
		{
			mChanged = true;
			mIsVisibleByAlpha = visibleByAlpha;
			mIsVisibleByPanel = visibleByPanel;
			return true;
		}
		return false;
	}

	public bool UpdateTransform(int frame)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		Transform val = base.cachedTransform;
		mPlayMode = Application.isPlaying;
		if (mMoved)
		{
			mMoved = true;
			mMatrixFrame = -1;
			val.hasChanged = false;
			Vector2 val2 = pivotOffset;
			float num = (0f - val2.x) * (float)mWidth;
			float num2 = (0f - val2.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			mOldV0 = ((Matrix4x4)(ref panel.worldToLocal)).MultiplyPoint3x4(val.TransformPoint(num, num2, 0f));
			mOldV1 = ((Matrix4x4)(ref panel.worldToLocal)).MultiplyPoint3x4(val.TransformPoint(num3, num4, 0f));
		}
		else if (!panel.widgetsAreStatic && val.hasChanged)
		{
			mMatrixFrame = -1;
			val.hasChanged = false;
			Vector2 val3 = pivotOffset;
			float num5 = (0f - val3.x) * (float)mWidth;
			float num6 = (0f - val3.y) * (float)mHeight;
			float num7 = num5 + (float)mWidth;
			float num8 = num6 + (float)mHeight;
			Vector3 val4 = ((Matrix4x4)(ref panel.worldToLocal)).MultiplyPoint3x4(val.TransformPoint(num5, num6, 0f));
			Vector3 val5 = ((Matrix4x4)(ref panel.worldToLocal)).MultiplyPoint3x4(val.TransformPoint(num7, num8, 0f));
			if (Vector3.SqrMagnitude(mOldV0 - val4) > 1E-06f || Vector3.SqrMagnitude(mOldV1 - val5) > 1E-06f)
			{
				mMoved = true;
				mOldV0 = val4;
				mOldV1 = val5;
			}
		}
		if (mMoved && onChange != null)
		{
			onChange();
		}
		return mMoved || mChanged;
	}

	public bool UpdateGeometry(int frame)
	{
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		float num = CalculateFinalAlpha(frame);
		if (mIsVisibleByAlpha && mLastAlpha != num)
		{
			mChanged = true;
		}
		mLastAlpha = num;
		if (mChanged)
		{
			if (mIsVisibleByAlpha && num > 0.001f && (Object)(object)shader != (Object)null)
			{
				bool result = geometry.hasVertices;
				if (fillGeometry)
				{
					geometry.Clear();
					OnFill(geometry.verts, geometry.uvs, geometry.cols);
				}
				if (geometry.hasVertices)
				{
					if (mMatrixFrame != frame)
					{
						mLocalToPanel = panel.worldToLocal * base.cachedTransform.localToWorldMatrix;
						mMatrixFrame = frame;
					}
					geometry.ApplyTransform(mLocalToPanel, panel.generateNormals);
					mMoved = false;
					mChanged = false;
					return true;
				}
				mChanged = false;
				return result;
			}
			if (geometry.hasVertices)
			{
				if (fillGeometry)
				{
					geometry.Clear();
				}
				mMoved = false;
				mChanged = false;
				return true;
			}
		}
		else if (mMoved && geometry.hasVertices)
		{
			if (mMatrixFrame != frame)
			{
				mLocalToPanel = panel.worldToLocal * base.cachedTransform.localToWorldMatrix;
				mMatrixFrame = frame;
			}
			geometry.ApplyTransform(mLocalToPanel, panel.generateNormals);
			mMoved = false;
			mChanged = false;
			return true;
		}
		mMoved = false;
		mChanged = false;
		return false;
	}

	public void WriteToBuffers(BetterList<Vector3> v, BetterList<Vector2> u, BetterList<Color> c, BetterList<Vector3> n, BetterList<Vector4> t)
	{
		geometry.WriteToBuffers(v, u, c, n, t);
	}

	public virtual void MakePixelPerfect()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		Vector3 localPosition = base.cachedTransform.localPosition;
		localPosition.z = Mathf.Round(localPosition.z);
		localPosition.x = Mathf.Round(localPosition.x);
		localPosition.y = Mathf.Round(localPosition.y);
		base.cachedTransform.localPosition = localPosition;
		Vector3 localScale = base.cachedTransform.localScale;
		base.cachedTransform.localScale = new Vector3(Mathf.Sign(localScale.x), Mathf.Sign(localScale.y), 1f);
	}

	public virtual void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
	}
}
