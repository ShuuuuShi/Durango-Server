using System;
using System.Diagnostics;
using UnityEngine;

public abstract class UIBasicSprite : UIWidget
{
	public enum Type
	{
		Simple,
		Sliced,
		Tiled,
		Filled,
		Advanced
	}

	public enum FillDirection
	{
		Horizontal,
		Vertical,
		Radial90,
		Radial180,
		Radial360
	}

	public enum AdvancedType
	{
		Invisible,
		Sliced,
		Tiled
	}

	public enum Flip
	{
		Nothing,
		Horizontally,
		Vertically,
		Both
	}

	public enum Rotate
	{
		Nothing,
		Radial90,
		Radial180,
		Radial270
	}

	public enum Fit
	{
		None,
		FitVertically,
		FitHorizontally,
		FitInside,
		FitOutside
	}

	[Serializable]
	protected struct TileOption
	{
		public bool on;

		public int width;

		public int height;
	}

	[HideInInspector]
	[SerializeField]
	protected Type mType;

	[HideInInspector]
	[SerializeField]
	protected FillDirection mFillDirection = FillDirection.Radial360;

	[Range(0f, 1f)]
	[HideInInspector]
	[SerializeField]
	protected float mFillAmount = 1f;

	[HideInInspector]
	[SerializeField]
	protected bool mInvert;

	[HideInInspector]
	[SerializeField]
	protected Flip mFlip;

	[HideInInspector]
	[SerializeField]
	protected Rotate mRotate;

	[HideInInspector]
	[SerializeField]
	protected TileOption mTile;

	[HideInInspector]
	[SerializeField]
	protected bool mApplyGradient;

	[HideInInspector]
	[SerializeField]
	protected Color mGradientTop = Color.white;

	[HideInInspector]
	[SerializeField]
	protected Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	[HideInInspector]
	[SerializeField]
	protected Fit mFit;

	[HideInInspector]
	[SerializeField]
	private float mFitAcpectRatio;

	[NonSerialized]
	private Rect mInnerUV;

	[NonSerialized]
	private Rect mOuterUV;

	[HideInInspector]
	public AdvancedType centerType = AdvancedType.Sliced;

	[HideInInspector]
	public AdvancedType leftType = AdvancedType.Sliced;

	[HideInInspector]
	public AdvancedType rightType = AdvancedType.Sliced;

	[HideInInspector]
	public AdvancedType bottomType = AdvancedType.Sliced;

	[HideInInspector]
	public AdvancedType topType = AdvancedType.Sliced;

	protected static Vector2[] mTempPos = new Vector2[4];

	protected static Vector2[] mTempUVs = new Vector2[4];

	public float fitAcpectRatio
	{
		get
		{
			return mFitAcpectRatio;
		}
		set
		{
			if (mFitAcpectRatio != value)
			{
				mFitAcpectRatio = value;
				MarkAsChanged();
			}
		}
	}

	public virtual Type type
	{
		get
		{
			return mType;
		}
		set
		{
			if (mType != value)
			{
				mType = value;
				MarkAsChanged();
			}
		}
	}

	public Flip flip
	{
		get
		{
			return mFlip;
		}
		set
		{
			if (mFlip != value)
			{
				mFlip = value;
				MarkAsChanged();
			}
		}
	}

	public Rotate rotate
	{
		get
		{
			return mRotate;
		}
		set
		{
			if (mRotate != value)
			{
				mRotate = value;
				MarkAsChanged();
			}
		}
	}

	public FillDirection fillDirection
	{
		get
		{
			return mFillDirection;
		}
		set
		{
			if (mFillDirection != value)
			{
				mFillDirection = value;
				mChanged = true;
			}
		}
	}

	public Fit fit
	{
		get
		{
			return mFit;
		}
		set
		{
			if (mFit != value)
			{
				mFit = value;
				MarkAsChanged();
			}
		}
	}

	public float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			float num2 = 0f;
			if (num > 0f && num < 1f)
			{
				switch (mFillDirection)
				{
				case FillDirection.Horizontal:
					num2 = 1f / (float)base.width;
					break;
				case FillDirection.Vertical:
					num2 = 1f / (float)base.height;
					break;
				case FillDirection.Radial90:
				case FillDirection.Radial180:
				case FillDirection.Radial360:
					num2 = 0.005f;
					break;
				}
			}
			if (Mathf.Abs(mFillAmount - num) > num2)
			{
				mFillAmount = num;
				mChanged = true;
			}
		}
	}

	public override int minWidth
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 vector = border * pixelSize;
				int num = Mathf.RoundToInt(vector.x + vector.z);
				return Mathf.Max(base.minWidth, ((num & 1) != 1) ? num : (num + 1));
			}
			return base.minWidth;
		}
	}

	public override int minHeight
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 vector = border * pixelSize;
				int num = Mathf.RoundToInt(vector.y + vector.w);
				return Mathf.Max(base.minHeight, ((num & 1) != 1) ? num : (num + 1));
			}
			return base.minHeight;
		}
	}

	public bool invert
	{
		get
		{
			return mInvert;
		}
		set
		{
			if (mInvert != value)
			{
				mInvert = value;
				mChanged = true;
			}
		}
	}

	public bool hasBorder
	{
		get
		{
			Vector4 vector = border;
			if (vector.x == 0f && vector.y == 0f && vector.z == 0f)
			{
				return vector.w != 0f;
			}
			return true;
		}
	}

	public virtual bool premultipliedAlpha => false;

	public virtual float pixelSize => 1f;

	private Vector4 drawingUVs => new Vector4(mOuterUV.xMin, mOuterUV.yMin, mOuterUV.xMax, mOuterUV.yMax);

	protected Color drawingColor
	{
		get
		{
			Color color = this.color;
			color.a = finalAlpha;
			if (premultipliedAlpha)
			{
				color = NGUITools.ApplyPMA(color);
			}
			return color;
		}
	}

	protected void CalcFitArea(ref Vector4 drawingArea, ref Rect outer, ref Rect inner)
	{
		if (mFit == Fit.None)
		{
			return;
		}
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		float num = (float)texture.width * outer.width;
		float num2 = (float)texture.height * outer.height;
		float num3 = drawingArea.z - drawingArea.x;
		float num4 = drawingArea.w - drawingArea.y;
		float num5 = ((mFitAcpectRatio != 0f) ? mFitAcpectRatio : (num / num2));
		float num6 = num3 / num4;
		if (Mathf.Abs(num5 - num6) < 0.001f)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		switch (mFit)
		{
		case Fit.FitVertically:
			flag2 = true;
			break;
		case Fit.FitHorizontally:
			flag = true;
			break;
		case Fit.FitInside:
			if (num5 > num6)
			{
				flag = true;
			}
			else
			{
				flag2 = true;
			}
			break;
		case Fit.FitOutside:
			if (num5 > num6)
			{
				flag2 = true;
			}
			else
			{
				flag = true;
			}
			break;
		}
		Vector3[] array = localCorners;
		Vector2 vector = base.pivotOffset;
		if (base.pivot != Pivot.Center)
		{
			float num7 = num / num3;
			float num8 = num2 / num4;
			if (flag)
			{
				float num9 = (drawingArea.y - array[0].y) * num8 / num7;
				float num10 = (array[2].y - drawingArea.w) * num8 / num7;
				float num11 = Mathf.Lerp(array[0].y + num9 + num4 * 0.5f, array[2].y - num10 - num4 * 0.5f, vector.y) - Mathf.Lerp(drawingArea.y, drawingArea.w, 0.5f);
				drawingArea.y += num11;
				drawingArea.w += num11;
			}
			else if (flag2)
			{
				float num12 = (drawingArea.x - array[0].x) * num7 / num8;
				float num13 = (array[2].x - drawingArea.z) * num7 / num8;
				float num14 = Mathf.Lerp(array[0].x + num12 + num3 * 0.5f, array[2].x - num13 - num3 * 0.5f, vector.x) - Mathf.Lerp(drawingArea.x, drawingArea.z, 0.5f);
				drawingArea.x += num14;
				drawingArea.z += num14;
			}
		}
		if (flag)
		{
			float num15 = num3 / num5;
			float num16 = Mathf.Lerp(drawingArea.y, drawingArea.w, vector.y);
			drawingArea.y = num16 - num15 * vector.y;
			drawingArea.w = num16 + num15 * (1f - vector.y);
		}
		else if (flag2)
		{
			float num17 = num4 * num5;
			float num18 = Mathf.Lerp(drawingArea.x, drawingArea.z, vector.x);
			drawingArea.x = num18 - num17 * vector.x;
			drawingArea.z = num18 + num17 * (1f - vector.x);
		}
		Vector4 vector2 = drawingArea;
		Rect rect = outer;
		Rect rect2 = inner;
		if (drawingArea.x < array[0].x)
		{
			drawingArea.x = array[0].x;
			float num19 = (array[0].x - vector2.x) / (vector2.z - vector2.x);
			outer.xMin += rect.width * num19;
			inner.xMin += rect2.width * num19;
		}
		if (drawingArea.y < array[0].y)
		{
			drawingArea.y = array[0].y;
			float num20 = (array[0].y - vector2.y) / (vector2.w - vector2.y);
			outer.yMin += rect.height * num20;
			inner.yMin += rect2.height * num20;
		}
		if (drawingArea.z > array[2].x)
		{
			drawingArea.z = array[2].x;
			float num21 = (vector2.z - array[2].x) / (vector2.z - vector2.x);
			outer.xMax -= rect.width * num21;
			inner.xMax -= rect2.width * num21;
		}
		if (drawingArea.w > array[2].y)
		{
			drawingArea.w = array[2].y;
			float num22 = (vector2.w - array[2].y) / (vector2.w - vector2.y);
			outer.yMax -= rect.height * num22;
			inner.yMax -= rect2.height * num22;
		}
	}

	protected void Fill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect outer, Rect inner)
	{
		Vector4 drawingArea = drawingDimensions;
		CalcFitArea(ref drawingArea, ref outer, ref inner);
		mOuterUV = outer;
		mInnerUV = inner;
		int size = verts.size;
		switch (type)
		{
		case Type.Simple:
			SimpleFill(verts, uvs, cols);
			break;
		case Type.Sliced:
			SlicedFill(verts, uvs, cols);
			break;
		case Type.Filled:
			FilledFill(verts, uvs, cols);
			break;
		case Type.Tiled:
			SimpleFill(verts, uvs, cols);
			break;
		case Type.Advanced:
			AdvancedFill(verts, uvs, cols);
			break;
		}
		if (mTile.on || type == Type.Tiled)
		{
			TiledFill(size, verts, uvs, cols);
		}
		ref Vector2 reference = ref mTempPos[0];
		reference = new Vector2(drawingArea.x, drawingArea.y);
		ref Vector2 reference2 = ref mTempPos[1];
		reference2 = new Vector2(drawingArea.x, drawingArea.w);
		ref Vector2 reference3 = ref mTempPos[2];
		reference3 = new Vector2(drawingArea.z, drawingArea.w);
		ref Vector2 reference4 = ref mTempPos[3];
		reference4 = new Vector2(drawingArea.z, drawingArea.y);
		switch (mFlip)
		{
		case Flip.Horizontally:
			SwapPos(mTempPos, 0, 3);
			SwapPos(mTempPos, 1, 2);
			break;
		case Flip.Vertically:
			SwapPos(mTempPos, 0, 1);
			SwapPos(mTempPos, 2, 3);
			break;
		case Flip.Both:
			SwapPos(mTempPos, 0, 2);
			SwapPos(mTempPos, 1, 3);
			break;
		}
		int num = (int)mRotate;
		int num2 = (num + 2) % 4;
		int num3 = num % 2;
		int index = (num3 + 1) % 2;
		for (int i = size; i < verts.size; i++)
		{
			verts[i] = new Vector2(Mathf.Lerp(mTempPos[num].x, mTempPos[num2].x, verts[i][num3]), Mathf.Lerp(mTempPos[num].y, mTempPos[num2].y, verts[i][index]));
		}
	}

	private void SwapPos(Vector2[] arr, int from, int to)
	{
		Vector2 vector = arr[from];
		ref Vector2 reference = ref arr[from];
		reference = arr[to];
		arr[to] = vector;
	}

	private void SimpleFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector4 vector = drawingUVs;
		Color c = drawingColor;
		Color item = c.GammaToLinearSpace();
		verts.Add(new Vector3(0f, 0f));
		verts.Add(new Vector3(0f, 1f));
		verts.Add(new Vector3(1f, 1f));
		verts.Add(new Vector3(1f, 0f));
		uvs.Add(new Vector2(vector.x, vector.y));
		uvs.Add(new Vector2(vector.x, vector.w));
		uvs.Add(new Vector2(vector.z, vector.w));
		uvs.Add(new Vector2(vector.z, vector.y));
		if (!mApplyGradient)
		{
			cols.Add(item);
			cols.Add(item);
			cols.Add(item);
			cols.Add(item);
		}
		else
		{
			AddVertexColours(cols, ref c, 1, 1);
			AddVertexColours(cols, ref c, 1, 2);
			AddVertexColours(cols, ref c, 2, 2);
			AddVertexColours(cols, ref c, 2, 1);
		}
	}

	private void SlicedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Vector4 vector = border * pixelSize;
		if (vector.x == 0f && vector.y == 0f && vector.z == 0f && vector.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}
		Color c = drawingColor;
		Color item = c.GammaToLinearSpace();
		Vector2 tileSize = GetTileSize();
		for (int i = 0; i < 4; i++)
		{
			vector[i] /= tileSize[(int)(i + mRotate) % 2];
		}
		mTempPos[0].x = 0f;
		mTempPos[0].y = 0f;
		mTempPos[3].x = 1f;
		mTempPos[3].y = 1f;
		mTempPos[1].x = mTempPos[0].x + vector.x;
		mTempPos[2].x = mTempPos[3].x - vector.z;
		mTempUVs[0].x = mOuterUV.xMin;
		mTempUVs[1].x = mInnerUV.xMin;
		mTempUVs[2].x = mInnerUV.xMax;
		mTempUVs[3].x = mOuterUV.xMax;
		mTempPos[1].y = mTempPos[0].y + vector.y;
		mTempPos[2].y = mTempPos[3].y - vector.w;
		mTempUVs[0].y = mOuterUV.yMin;
		mTempUVs[1].y = mInnerUV.yMin;
		mTempUVs[2].y = mInnerUV.yMax;
		mTempUVs[3].y = mOuterUV.yMax;
		for (int j = 0; j < 3; j++)
		{
			int num = j + 1;
			for (int k = 0; k < 3; k++)
			{
				if (centerType != 0 || j != 1 || k != 1)
				{
					int num2 = k + 1;
					verts.Add(new Vector3(mTempPos[j].x, mTempPos[k].y));
					verts.Add(new Vector3(mTempPos[j].x, mTempPos[num2].y));
					verts.Add(new Vector3(mTempPos[num].x, mTempPos[num2].y));
					verts.Add(new Vector3(mTempPos[num].x, mTempPos[k].y));
					uvs.Add(new Vector2(mTempUVs[j].x, mTempUVs[k].y));
					uvs.Add(new Vector2(mTempUVs[j].x, mTempUVs[num2].y));
					uvs.Add(new Vector2(mTempUVs[num].x, mTempUVs[num2].y));
					uvs.Add(new Vector2(mTempUVs[num].x, mTempUVs[k].y));
					if (!mApplyGradient)
					{
						cols.Add(item);
						cols.Add(item);
						cols.Add(item);
						cols.Add(item);
					}
					else
					{
						AddVertexColours(cols, ref c, j, k);
						AddVertexColours(cols, ref c, j, num2);
						AddVertexColours(cols, ref c, num, num2);
						AddVertexColours(cols, ref c, num, k);
					}
				}
			}
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	private void AddVertexColours(BetterList<Color> cols, ref Color color, int x, int y)
	{
		switch (y)
		{
		case 0:
		case 1:
			cols.Add((color * mGradientBottom).GammaToLinearSpace());
			break;
		case 2:
		case 3:
			cols.Add((color * mGradientTop).GammaToLinearSpace());
			break;
		}
	}

	public Vector2 GetTileSize()
	{
		if ((mTile.on || type == Type.Tiled) && type != Type.Filled)
		{
			Texture texture = mainTexture;
			Vector2 vector = default(Vector2);
			vector.x = ((type != Type.Tiled && mTile.width > 0) ? ((float)mTile.width) : ((!(texture == null)) ? (mOuterUV.width * (float)texture.width) : ((float)base.width)));
			vector.y = ((type != Type.Tiled && mTile.height > 0) ? ((float)mTile.height) : ((!(texture == null)) ? (mOuterUV.height * (float)texture.height) : ((float)base.height)));
			return vector * pixelSize;
		}
		Vector4 vector2 = drawingDimensions;
		return new Vector2(vector2.z - vector2.x, vector2.w - vector2.y);
	}

	private void TiledFill(int offset, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (mainTexture == null)
		{
			return;
		}
		Vector2 tileSize = GetTileSize();
		if (tileSize.x < 2f || tileSize.y < 2f)
		{
			return;
		}
		Vector4 vector = drawingDimensions;
		tileSize.x /= (((int)mRotate % 2 != 0) ? (vector.w - vector.y) : (vector.z - vector.x));
		tileSize.y /= (((int)mRotate % 2 != 0) ? (vector.z - vector.x) : (vector.w - vector.y));
		vector = new Vector4(0f, 0f, 1f, 1f);
		int size = verts.size;
		for (float num = vector.y; num < vector.w; num += tileSize.y)
		{
			float num2 = vector.x;
			float num3 = num + tileSize.y;
			float num4 = 1f;
			if (num3 > vector.w)
			{
				num4 -= (num3 - vector.w) / tileSize.y;
			}
			for (; num2 < vector.z; num2 += tileSize.x)
			{
				float num5 = num2 + tileSize.x;
				float num6 = 1f;
				if (num5 > vector.z)
				{
					num6 -= (num5 - vector.z) / tileSize.x;
				}
				for (int i = offset; i < size; i += 4)
				{
					Vector4 zero = Vector4.zero;
					Vector4 zero2 = Vector4.zero;
					zero.x = verts[i].x;
					zero.y = verts[i].y;
					zero.z = verts[i + 2].x;
					zero.w = verts[i + 2].y;
					zero2.x = uvs[i].x;
					zero2.y = uvs[i].y;
					zero2.z = uvs[i + 2].x;
					zero2.w = uvs[i + 2].y;
					if (!(zero.x > num6) && !(zero.y > num4))
					{
						if (zero.z > num6)
						{
							zero2.z = Mathf.Lerp(zero2.x, zero2.z, (num6 - zero.x) / (zero.z - zero.x));
							zero.z = num6;
						}
						if (zero.w > num4)
						{
							zero2.w = Mathf.Lerp(zero2.y, zero2.w, (num4 - zero.y) / (zero.w - zero.y));
							zero.w = num4;
						}
						zero.x = Mathf.Lerp(num2, num5, zero.x);
						zero.y = Mathf.Lerp(num, num3, zero.y);
						zero.z = Mathf.Lerp(num2, num5, zero.z);
						zero.w = Mathf.Lerp(num, num3, zero.w);
						verts.Add(new Vector3(zero.x, zero.y));
						verts.Add(new Vector3(zero.x, zero.w));
						verts.Add(new Vector3(zero.z, zero.w));
						verts.Add(new Vector3(zero.z, zero.y));
						uvs.Add(new Vector2(zero2.x, zero2.y));
						uvs.Add(new Vector2(zero2.x, zero2.w));
						uvs.Add(new Vector2(zero2.z, zero2.w));
						uvs.Add(new Vector2(zero2.z, zero2.y));
						cols.Add(cols[i]);
						cols.Add(cols[i + 1]);
						cols.Add(cols[i + 2]);
						cols.Add(cols[i + 3]);
					}
				}
			}
		}
		verts.RemoveRange(offset, size - offset);
		uvs.RemoveRange(offset, size - offset);
		cols.RemoveRange(offset, size - offset);
	}

	private void FilledFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (mFillAmount < 0.001f)
		{
			return;
		}
		Vector4 vector = new Vector4(0f, 0f, 1f, 1f);
		Vector4 vector2 = drawingUVs;
		Color item = drawingColor.GammaToLinearSpace();
		if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
		{
			if (mFillDirection == FillDirection.Horizontal)
			{
				float num = (vector2.z - vector2.x) * mFillAmount;
				if (mInvert)
				{
					vector.x = vector.z - (vector.z - vector.x) * mFillAmount;
					vector2.x = vector2.z - num;
				}
				else
				{
					vector.z = vector.x + (vector.z - vector.x) * mFillAmount;
					vector2.z = vector2.x + num;
				}
			}
			else if (mFillDirection == FillDirection.Vertical)
			{
				float num2 = (vector2.w - vector2.y) * mFillAmount;
				if (mInvert)
				{
					vector.y = vector.w - (vector.w - vector.y) * mFillAmount;
					vector2.y = vector2.w - num2;
				}
				else
				{
					vector.w = vector.y + (vector.w - vector.y) * mFillAmount;
					vector2.w = vector2.y + num2;
				}
			}
		}
		ref Vector2 reference = ref mTempPos[0];
		reference = new Vector2(vector.x, vector.y);
		ref Vector2 reference2 = ref mTempPos[1];
		reference2 = new Vector2(vector.x, vector.w);
		ref Vector2 reference3 = ref mTempPos[2];
		reference3 = new Vector2(vector.z, vector.w);
		ref Vector2 reference4 = ref mTempPos[3];
		reference4 = new Vector2(vector.z, vector.y);
		ref Vector2 reference5 = ref mTempUVs[0];
		reference5 = new Vector2(vector2.x, vector2.y);
		ref Vector2 reference6 = ref mTempUVs[1];
		reference6 = new Vector2(vector2.x, vector2.w);
		ref Vector2 reference7 = ref mTempUVs[2];
		reference7 = new Vector2(vector2.z, vector2.w);
		ref Vector2 reference8 = ref mTempUVs[3];
		reference8 = new Vector2(vector2.z, vector2.y);
		if (mFillAmount < 1f)
		{
			if (mFillDirection == FillDirection.Radial90)
			{
				if (RadialCut(mTempPos, mTempUVs, mFillAmount, mInvert, 0))
				{
					for (int i = 0; i < 4; i++)
					{
						verts.Add(mTempPos[i]);
						uvs.Add(mTempUVs[i]);
						cols.Add(item);
					}
				}
				return;
			}
			if (mFillDirection == FillDirection.Radial180)
			{
				for (int j = 0; j < 2; j++)
				{
					float t = 0f;
					float t2 = 1f;
					float t3;
					float t4;
					if (j == 0)
					{
						t3 = 0f;
						t4 = 0.5f;
					}
					else
					{
						t3 = 0.5f;
						t4 = 1f;
					}
					mTempPos[0].x = Mathf.Lerp(vector.x, vector.z, t3);
					mTempPos[1].x = mTempPos[0].x;
					mTempPos[2].x = Mathf.Lerp(vector.x, vector.z, t4);
					mTempPos[3].x = mTempPos[2].x;
					mTempPos[0].y = Mathf.Lerp(vector.y, vector.w, t);
					mTempPos[1].y = Mathf.Lerp(vector.y, vector.w, t2);
					mTempPos[2].y = mTempPos[1].y;
					mTempPos[3].y = mTempPos[0].y;
					mTempUVs[0].x = Mathf.Lerp(vector2.x, vector2.z, t3);
					mTempUVs[1].x = mTempUVs[0].x;
					mTempUVs[2].x = Mathf.Lerp(vector2.x, vector2.z, t4);
					mTempUVs[3].x = mTempUVs[2].x;
					mTempUVs[0].y = Mathf.Lerp(vector2.y, vector2.w, t);
					mTempUVs[1].y = Mathf.Lerp(vector2.y, vector2.w, t2);
					mTempUVs[2].y = mTempUVs[1].y;
					mTempUVs[3].y = mTempUVs[0].y;
					float value = (mInvert ? (mFillAmount * 2f - (float)(1 - j)) : (fillAmount * 2f - (float)j));
					if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(value), !mInvert, NGUIMath.RepeatIndex(j + 3, 4)))
					{
						for (int k = 0; k < 4; k++)
						{
							verts.Add(mTempPos[k]);
							uvs.Add(mTempUVs[k]);
							cols.Add(item);
						}
					}
				}
				return;
			}
			if (mFillDirection == FillDirection.Radial360)
			{
				for (int l = 0; l < 4; l++)
				{
					float t5;
					float t6;
					if (l < 2)
					{
						t5 = 0f;
						t6 = 0.5f;
					}
					else
					{
						t5 = 0.5f;
						t6 = 1f;
					}
					float t7;
					float t8;
					if (l == 0 || l == 3)
					{
						t7 = 0f;
						t8 = 0.5f;
					}
					else
					{
						t7 = 0.5f;
						t8 = 1f;
					}
					mTempPos[0].x = Mathf.Lerp(vector.x, vector.z, t5);
					mTempPos[1].x = mTempPos[0].x;
					mTempPos[2].x = Mathf.Lerp(vector.x, vector.z, t6);
					mTempPos[3].x = mTempPos[2].x;
					mTempPos[0].y = Mathf.Lerp(vector.y, vector.w, t7);
					mTempPos[1].y = Mathf.Lerp(vector.y, vector.w, t8);
					mTempPos[2].y = mTempPos[1].y;
					mTempPos[3].y = mTempPos[0].y;
					mTempUVs[0].x = Mathf.Lerp(vector2.x, vector2.z, t5);
					mTempUVs[1].x = mTempUVs[0].x;
					mTempUVs[2].x = Mathf.Lerp(vector2.x, vector2.z, t6);
					mTempUVs[3].x = mTempUVs[2].x;
					mTempUVs[0].y = Mathf.Lerp(vector2.y, vector2.w, t7);
					mTempUVs[1].y = Mathf.Lerp(vector2.y, vector2.w, t8);
					mTempUVs[2].y = mTempUVs[1].y;
					mTempUVs[3].y = mTempUVs[0].y;
					float value2 = ((!mInvert) ? (mFillAmount * 4f - (float)(3 - NGUIMath.RepeatIndex(l + 2, 4))) : (mFillAmount * 4f - (float)NGUIMath.RepeatIndex(l + 2, 4)));
					if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(value2), mInvert, NGUIMath.RepeatIndex(l + 2, 4)))
					{
						for (int m = 0; m < 4; m++)
						{
							verts.Add(mTempPos[m]);
							uvs.Add(mTempUVs[m]);
							cols.Add(item);
						}
					}
				}
				return;
			}
		}
		for (int n = 0; n < 4; n++)
		{
			verts.Add(mTempPos[n]);
			uvs.Add(mTempUVs[n]);
			cols.Add(item);
		}
	}

	private void AdvancedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		Texture texture = mainTexture;
		if (texture == null)
		{
			return;
		}
		Vector4 vector = border * pixelSize;
		if (vector.x == 0f && vector.y == 0f && vector.z == 0f && vector.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}
		Color col = drawingColor.GammaToLinearSpace();
		Vector2 vector2 = new Vector2(mInnerUV.width * (float)texture.width, mInnerUV.height * (float)texture.height);
		vector2 *= pixelSize;
		if (vector2.x < 1f)
		{
			vector2.x = 1f;
		}
		if (vector2.y < 1f)
		{
			vector2.y = 1f;
		}
		Vector2 tileSize = GetTileSize();
		for (int i = 0; i < 4; i++)
		{
			float num = tileSize[(int)(i + mRotate) % 2];
			vector[i] /= num;
			if (i < 2)
			{
				vector2[i] /= num;
			}
		}
		mTempPos[0].x = 0f;
		mTempPos[0].y = 0f;
		mTempPos[3].x = 1f;
		mTempPos[3].y = 1f;
		mTempPos[1].x = mTempPos[0].x + vector.x;
		mTempPos[2].x = mTempPos[3].x - vector.z;
		mTempUVs[0].x = mOuterUV.xMin;
		mTempUVs[1].x = mInnerUV.xMin;
		mTempUVs[2].x = mInnerUV.xMax;
		mTempUVs[3].x = mOuterUV.xMax;
		mTempPos[1].y = mTempPos[0].y + vector.y;
		mTempPos[2].y = mTempPos[3].y - vector.w;
		mTempUVs[0].y = mOuterUV.yMin;
		mTempUVs[1].y = mInnerUV.yMin;
		mTempUVs[2].y = mInnerUV.yMax;
		mTempUVs[3].y = mOuterUV.yMax;
		for (int j = 0; j < 3; j++)
		{
			int num2 = j + 1;
			for (int k = 0; k < 3; k++)
			{
				if (centerType == AdvancedType.Invisible && j == 1 && k == 1)
				{
					continue;
				}
				int num3 = k + 1;
				if (j == 1 && k == 1)
				{
					if (centerType == AdvancedType.Tiled)
					{
						float x = mTempPos[j].x;
						float x2 = mTempPos[num2].x;
						float y = mTempPos[k].y;
						float y2 = mTempPos[num3].y;
						float x3 = mTempUVs[j].x;
						float y3 = mTempUVs[k].y;
						for (float num4 = y; num4 < y2; num4 += vector2.y)
						{
							float num5 = x;
							float num6 = mTempUVs[num3].y;
							float num7 = num4 + vector2.y;
							if (num7 > y2)
							{
								num6 = Mathf.Lerp(y3, num6, (y2 - num4) / vector2.y);
								num7 = y2;
							}
							for (; num5 < x2; num5 += vector2.x)
							{
								float num8 = num5 + vector2.x;
								float num9 = mTempUVs[num2].x;
								if (num8 > x2)
								{
									num9 = Mathf.Lerp(x3, num9, (x2 - num5) / vector2.x);
									num8 = x2;
								}
								Fill(verts, uvs, cols, num5, num8, num4, num7, x3, num9, y3, num6, col);
							}
						}
					}
					else if (centerType == AdvancedType.Sliced)
					{
						Fill(verts, uvs, cols, mTempPos[j].x, mTempPos[num2].x, mTempPos[k].y, mTempPos[num3].y, mTempUVs[j].x, mTempUVs[num2].x, mTempUVs[k].y, mTempUVs[num3].y, col);
					}
				}
				else if (j == 1)
				{
					if ((k == 0 && bottomType == AdvancedType.Tiled) || (k == 2 && topType == AdvancedType.Tiled))
					{
						float x4 = mTempPos[j].x;
						float x5 = mTempPos[num2].x;
						float y4 = mTempPos[k].y;
						float y5 = mTempPos[num3].y;
						float x6 = mTempUVs[j].x;
						float y6 = mTempUVs[k].y;
						float y7 = mTempUVs[num3].y;
						for (float num10 = x4; num10 < x5; num10 += vector2.x)
						{
							float num11 = num10 + vector2.x;
							float num12 = mTempUVs[num2].x;
							if (num11 > x5)
							{
								num12 = Mathf.Lerp(x6, num12, (x5 - num10) / vector2.x);
								num11 = x5;
							}
							Fill(verts, uvs, cols, num10, num11, y4, y5, x6, num12, y6, y7, col);
						}
					}
					else if ((k == 0 && bottomType != 0) || (k == 2 && topType != 0))
					{
						Fill(verts, uvs, cols, mTempPos[j].x, mTempPos[num2].x, mTempPos[k].y, mTempPos[num3].y, mTempUVs[j].x, mTempUVs[num2].x, mTempUVs[k].y, mTempUVs[num3].y, col);
					}
				}
				else if (k == 1)
				{
					if ((j == 0 && leftType == AdvancedType.Tiled) || (j == 2 && rightType == AdvancedType.Tiled))
					{
						float x7 = mTempPos[j].x;
						float x8 = mTempPos[num2].x;
						float y8 = mTempPos[k].y;
						float y9 = mTempPos[num3].y;
						float x9 = mTempUVs[j].x;
						float x10 = mTempUVs[num2].x;
						float y10 = mTempUVs[k].y;
						for (float num13 = y8; num13 < y9; num13 += vector2.y)
						{
							float num14 = mTempUVs[num3].y;
							float num15 = num13 + vector2.y;
							if (num15 > y9)
							{
								num14 = Mathf.Lerp(y10, num14, (y9 - num13) / vector2.y);
								num15 = y9;
							}
							Fill(verts, uvs, cols, x7, x8, num13, num15, x9, x10, y10, num14, col);
						}
					}
					else if ((j == 0 && leftType != 0) || (j == 2 && rightType != 0))
					{
						Fill(verts, uvs, cols, mTempPos[j].x, mTempPos[num2].x, mTempPos[k].y, mTempPos[num3].y, mTempUVs[j].x, mTempUVs[num2].x, mTempUVs[k].y, mTempUVs[num3].y, col);
					}
				}
				else if ((k == 0 && bottomType != 0) || (k == 2 && topType != 0) || (j == 0 && leftType != 0) || (j == 2 && rightType != 0))
				{
					Fill(verts, uvs, cols, mTempPos[j].x, mTempPos[num2].x, mTempPos[k].y, mTempPos[num3].y, mTempUVs[j].x, mTempUVs[num2].x, mTempUVs[k].y, mTempUVs[num3].y, col);
				}
			}
		}
	}

	private static bool RadialCut(Vector2[] xy, Vector2[] uv, float fill, bool invert, int corner)
	{
		if (fill < 0.001f)
		{
			return false;
		}
		if ((corner & 1) == 1)
		{
			invert = !invert;
		}
		if (!invert && fill > 0.999f)
		{
			return true;
		}
		float num = Mathf.Clamp01(fill);
		if (invert)
		{
			num = 1f - num;
		}
		num *= (float)Math.PI / 2f;
		float cos = Mathf.Cos(num);
		float sin = Mathf.Sin(num);
		RadialCut(xy, cos, sin, invert, corner);
		RadialCut(uv, cos, sin, invert, corner);
		return true;
	}

	private static void RadialCut(Vector2[] xy, float cos, float sin, bool invert, int corner)
	{
		int num = NGUIMath.RepeatIndex(corner + 1, 4);
		int num2 = NGUIMath.RepeatIndex(corner + 2, 4);
		int num3 = NGUIMath.RepeatIndex(corner + 3, 4);
		if ((corner & 1) == 1)
		{
			if (sin > cos)
			{
				cos /= sin;
				sin = 1f;
				if (invert)
				{
					xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
					xy[num2].x = xy[num].x;
				}
			}
			else if (cos > sin)
			{
				sin /= cos;
				cos = 1f;
				if (!invert)
				{
					xy[num2].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
					xy[num3].y = xy[num2].y;
				}
			}
			else
			{
				cos = 1f;
				sin = 1f;
			}
			if (!invert)
			{
				xy[num3].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
			}
			else
			{
				xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
			}
			return;
		}
		if (cos > sin)
		{
			sin /= cos;
			cos = 1f;
			if (!invert)
			{
				xy[num].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
				xy[num2].y = xy[num].y;
			}
		}
		else if (sin > cos)
		{
			cos /= sin;
			sin = 1f;
			if (invert)
			{
				xy[num2].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
				xy[num3].x = xy[num2].x;
			}
		}
		else
		{
			cos = 1f;
			sin = 1f;
		}
		if (invert)
		{
			xy[num3].y = Mathf.Lerp(xy[corner].y, xy[num2].y, sin);
		}
		else
		{
			xy[num].x = Mathf.Lerp(xy[corner].x, xy[num2].x, cos);
		}
	}

	private static void Fill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, float v0x, float v1x, float v0y, float v1y, float u0x, float u1x, float u0y, float u1y, Color col)
	{
		verts.Add(new Vector3(v0x, v0y));
		verts.Add(new Vector3(v0x, v1y));
		verts.Add(new Vector3(v1x, v1y));
		verts.Add(new Vector3(v1x, v0y));
		uvs.Add(new Vector2(u0x, u0y));
		uvs.Add(new Vector2(u0x, u1y));
		uvs.Add(new Vector2(u1x, u1y));
		uvs.Add(new Vector2(u1x, u0y));
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
	}
}
