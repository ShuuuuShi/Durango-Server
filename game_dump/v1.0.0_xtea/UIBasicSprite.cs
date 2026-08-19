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

	[SerializeField]
	[HideInInspector]
	protected Type mType;

	[SerializeField]
	[HideInInspector]
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

	[SerializeField]
	[HideInInspector]
	protected bool mApplyGradient;

	[HideInInspector]
	[SerializeField]
	protected Color mGradientTop = Color.white;

	[SerializeField]
	[HideInInspector]
	protected Color mGradientBottom = new Color(0.7f, 0.7f, 0.7f);

	[NonSerialized]
	private Rect mInnerUV = default(Rect);

	[NonSerialized]
	private Rect mOuterUV = default(Rect);

	public AdvancedType centerType = AdvancedType.Sliced;

	public AdvancedType leftType = AdvancedType.Sliced;

	public AdvancedType rightType = AdvancedType.Sliced;

	public AdvancedType bottomType = AdvancedType.Sliced;

	public AdvancedType topType = AdvancedType.Sliced;

	protected static Vector2[] mTempPos = (Vector2[])(object)new Vector2[4];

	protected static Vector2[] mTempUVs = (Vector2[])(object)new Vector2[4];

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

	public float fillAmount
	{
		get
		{
			return mFillAmount;
		}
		set
		{
			float num = Mathf.Clamp01(value);
			if (mFillAmount != num)
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
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 val = border * pixelSize;
				int num = Mathf.RoundToInt(val.x + val.z);
				return Mathf.Max(base.minWidth, ((num & 1) != 1) ? num : (num + 1));
			}
			return base.minWidth;
		}
	}

	public override int minHeight
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			if (type == Type.Sliced || type == Type.Advanced)
			{
				Vector4 val = border * pixelSize;
				int num = Mathf.RoundToInt(val.y + val.w);
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
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			Vector4 val = border;
			return val.x != 0f || val.y != 0f || val.z != 0f || val.w != 0f;
		}
	}

	public virtual bool premultipliedAlpha => false;

	public virtual float pixelSize => 1f;

	private Vector4 drawingUVs => (Vector4)(mFlip switch
	{
		Flip.Horizontally => new Vector4(((Rect)(ref mOuterUV)).xMax, ((Rect)(ref mOuterUV)).yMin, ((Rect)(ref mOuterUV)).xMin, ((Rect)(ref mOuterUV)).yMax), 
		Flip.Vertically => new Vector4(((Rect)(ref mOuterUV)).xMin, ((Rect)(ref mOuterUV)).yMax, ((Rect)(ref mOuterUV)).xMax, ((Rect)(ref mOuterUV)).yMin), 
		Flip.Both => new Vector4(((Rect)(ref mOuterUV)).xMax, ((Rect)(ref mOuterUV)).yMax, ((Rect)(ref mOuterUV)).xMin, ((Rect)(ref mOuterUV)).yMin), 
		_ => new Vector4(((Rect)(ref mOuterUV)).xMin, ((Rect)(ref mOuterUV)).yMin, ((Rect)(ref mOuterUV)).xMax, ((Rect)(ref mOuterUV)).yMax), 
	});

	protected Color drawingColor
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			Color val = base.color;
			val.a = finalAlpha;
			if (premultipliedAlpha)
			{
				val = NGUITools.ApplyPMA(val);
			}
			return val;
		}
	}

	protected void Fill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols, Rect outer, Rect inner)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		mOuterUV = outer;
		mInnerUV = inner;
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
			TiledFill(verts, uvs, cols);
			break;
		case Type.Advanced:
			AdvancedFill(verts, uvs, cols);
			break;
		}
	}

	private void SimpleFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = drawingDimensions;
		Vector4 val2 = drawingUVs;
		Color c = drawingColor;
		Color item = c.GammaToLinearSpace();
		verts.Add(new Vector3(val.x, val.y));
		verts.Add(new Vector3(val.x, val.w));
		verts.Add(new Vector3(val.z, val.w));
		verts.Add(new Vector3(val.z, val.y));
		uvs.Add(new Vector2(val2.x, val2.y));
		uvs.Add(new Vector2(val2.x, val2.w));
		uvs.Add(new Vector2(val2.z, val2.w));
		uvs.Add(new Vector2(val2.z, val2.y));
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
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_053b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0568: Unknown result type (might be due to invalid IL or missing references)
		//IL_0595: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		Vector4 val = border * pixelSize;
		if (val.x == 0f && val.y == 0f && val.z == 0f && val.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}
		Color c = drawingColor;
		Color item = c.GammaToLinearSpace();
		Vector4 val2 = drawingDimensions;
		mTempPos[0].x = val2.x;
		mTempPos[0].y = val2.y;
		mTempPos[3].x = val2.z;
		mTempPos[3].y = val2.w;
		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + val.z;
			mTempPos[2].x = mTempPos[3].x - val.x;
			mTempUVs[3].x = ((Rect)(ref mOuterUV)).xMin;
			mTempUVs[2].x = ((Rect)(ref mInnerUV)).xMin;
			mTempUVs[1].x = ((Rect)(ref mInnerUV)).xMax;
			mTempUVs[0].x = ((Rect)(ref mOuterUV)).xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + val.x;
			mTempPos[2].x = mTempPos[3].x - val.z;
			mTempUVs[0].x = ((Rect)(ref mOuterUV)).xMin;
			mTempUVs[1].x = ((Rect)(ref mInnerUV)).xMin;
			mTempUVs[2].x = ((Rect)(ref mInnerUV)).xMax;
			mTempUVs[3].x = ((Rect)(ref mOuterUV)).xMax;
		}
		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + val.w;
			mTempPos[2].y = mTempPos[3].y - val.y;
			mTempUVs[3].y = ((Rect)(ref mOuterUV)).yMin;
			mTempUVs[2].y = ((Rect)(ref mInnerUV)).yMin;
			mTempUVs[1].y = ((Rect)(ref mInnerUV)).yMax;
			mTempUVs[0].y = ((Rect)(ref mOuterUV)).yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + val.y;
			mTempPos[2].y = mTempPos[3].y - val.w;
			mTempUVs[0].y = ((Rect)(ref mOuterUV)).yMin;
			mTempUVs[1].y = ((Rect)(ref mInnerUV)).yMin;
			mTempUVs[2].y = ((Rect)(ref mInnerUV)).yMax;
			mTempUVs[3].y = ((Rect)(ref mOuterUV)).yMax;
		}
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			for (int j = 0; j < 3; j++)
			{
				if (centerType != 0 || i != 1 || j != 1)
				{
					int num2 = j + 1;
					verts.Add(new Vector3(mTempPos[i].x, mTempPos[j].y));
					verts.Add(new Vector3(mTempPos[i].x, mTempPos[num2].y));
					verts.Add(new Vector3(mTempPos[num].x, mTempPos[num2].y));
					verts.Add(new Vector3(mTempPos[num].x, mTempPos[j].y));
					uvs.Add(new Vector2(mTempUVs[i].x, mTempUVs[j].y));
					uvs.Add(new Vector2(mTempUVs[i].x, mTempUVs[num2].y));
					uvs.Add(new Vector2(mTempUVs[num].x, mTempUVs[num2].y));
					uvs.Add(new Vector2(mTempUVs[num].x, mTempUVs[j].y));
					if (!mApplyGradient)
					{
						cols.Add(item);
						cols.Add(item);
						cols.Add(item);
						cols.Add(item);
					}
					else
					{
						AddVertexColours(cols, ref c, i, j);
						AddVertexColours(cols, ref c, i, num2);
						AddVertexColours(cols, ref c, num, num2);
						AddVertexColours(cols, ref c, num, j);
					}
				}
			}
		}
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	private void AddVertexColours(BetterList<Color> cols, ref Color color, int x, int y)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
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

	private void TiledFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
		Texture val = mainTexture;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector(((Rect)(ref mOuterUV)).width * (float)val.width, ((Rect)(ref mOuterUV)).height * (float)val.height);
		val2 *= pixelSize;
		if ((Object)(object)val == (Object)null || val2.x < 2f || val2.y < 2f)
		{
			return;
		}
		Color item = drawingColor.GammaToLinearSpace();
		Vector4 val3 = drawingDimensions;
		Vector4 val4 = default(Vector4);
		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			val4.x = ((Rect)(ref mOuterUV)).xMax;
			val4.z = ((Rect)(ref mOuterUV)).xMin;
		}
		else
		{
			val4.x = ((Rect)(ref mOuterUV)).xMin;
			val4.z = ((Rect)(ref mOuterUV)).xMax;
		}
		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			val4.y = ((Rect)(ref mOuterUV)).yMax;
			val4.w = ((Rect)(ref mOuterUV)).yMin;
		}
		else
		{
			val4.y = ((Rect)(ref mOuterUV)).yMin;
			val4.w = ((Rect)(ref mOuterUV)).yMax;
		}
		float x = val3.x;
		float num = val3.y;
		float x2 = val4.x;
		float y = val4.y;
		for (; num < val3.w; num += val2.y)
		{
			x = val3.x;
			float num2 = num + val2.y;
			float num3 = val4.w;
			if (num2 > val3.w)
			{
				num3 = Mathf.Lerp(val4.y, val4.w, (val3.w - num) / val2.y);
				num2 = val3.w;
			}
			for (; x < val3.z; x += val2.x)
			{
				float num4 = x + val2.x;
				float num5 = val4.z;
				if (num4 > val3.z)
				{
					num5 = Mathf.Lerp(val4.x, val4.z, (val3.z - x) / val2.x);
					num4 = val3.z;
				}
				verts.Add(new Vector3(x, num));
				verts.Add(new Vector3(x, num2));
				verts.Add(new Vector3(num4, num2));
				verts.Add(new Vector3(num4, num));
				uvs.Add(new Vector2(x2, y));
				uvs.Add(new Vector2(x2, num3));
				uvs.Add(new Vector2(num5, num3));
				uvs.Add(new Vector2(num5, y));
				cols.Add(item);
				cols.Add(item);
				cols.Add(item);
				cols.Add(item);
			}
		}
	}

	private void FilledFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_09fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_0314: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_0617: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0639: Unknown result type (might be due to invalid IL or missing references)
		//IL_0975: Unknown result type (might be due to invalid IL or missing references)
		//IL_097a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0991: Unknown result type (might be due to invalid IL or missing references)
		//IL_099c: Unknown result type (might be due to invalid IL or missing references)
		if (mFillAmount < 0.001f)
		{
			return;
		}
		Vector4 val = drawingDimensions;
		Vector4 val2 = drawingUVs;
		Color item = drawingColor.GammaToLinearSpace();
		if (mFillDirection == FillDirection.Horizontal || mFillDirection == FillDirection.Vertical)
		{
			if (mFillDirection == FillDirection.Horizontal)
			{
				float num = (val2.z - val2.x) * mFillAmount;
				if (mInvert)
				{
					val.x = val.z - (val.z - val.x) * mFillAmount;
					val2.x = val2.z - num;
				}
				else
				{
					val.z = val.x + (val.z - val.x) * mFillAmount;
					val2.z = val2.x + num;
				}
			}
			else if (mFillDirection == FillDirection.Vertical)
			{
				float num2 = (val2.w - val2.y) * mFillAmount;
				if (mInvert)
				{
					val.y = val.w - (val.w - val.y) * mFillAmount;
					val2.y = val2.w - num2;
				}
				else
				{
					val.w = val.y + (val.w - val.y) * mFillAmount;
					val2.w = val2.y + num2;
				}
			}
		}
		ref Vector2 reference = ref mTempPos[0];
		reference = new Vector2(val.x, val.y);
		ref Vector2 reference2 = ref mTempPos[1];
		reference2 = new Vector2(val.x, val.w);
		ref Vector2 reference3 = ref mTempPos[2];
		reference3 = new Vector2(val.z, val.w);
		ref Vector2 reference4 = ref mTempPos[3];
		reference4 = new Vector2(val.z, val.y);
		ref Vector2 reference5 = ref mTempUVs[0];
		reference5 = new Vector2(val2.x, val2.y);
		ref Vector2 reference6 = ref mTempUVs[1];
		reference6 = new Vector2(val2.x, val2.w);
		ref Vector2 reference7 = ref mTempUVs[2];
		reference7 = new Vector2(val2.z, val2.w);
		ref Vector2 reference8 = ref mTempUVs[3];
		reference8 = new Vector2(val2.z, val2.y);
		if (mFillAmount < 1f)
		{
			if (mFillDirection == FillDirection.Radial90)
			{
				if (RadialCut(mTempPos, mTempUVs, mFillAmount, mInvert, 0))
				{
					for (int i = 0; i < 4; i++)
					{
						verts.Add(Vector2.op_Implicit(mTempPos[i]));
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
					float num3 = 0f;
					float num4 = 1f;
					float num5;
					float num6;
					if (j == 0)
					{
						num5 = 0f;
						num6 = 0.5f;
					}
					else
					{
						num5 = 0.5f;
						num6 = 1f;
					}
					mTempPos[0].x = Mathf.Lerp(val.x, val.z, num5);
					mTempPos[1].x = mTempPos[0].x;
					mTempPos[2].x = Mathf.Lerp(val.x, val.z, num6);
					mTempPos[3].x = mTempPos[2].x;
					mTempPos[0].y = Mathf.Lerp(val.y, val.w, num3);
					mTempPos[1].y = Mathf.Lerp(val.y, val.w, num4);
					mTempPos[2].y = mTempPos[1].y;
					mTempPos[3].y = mTempPos[0].y;
					mTempUVs[0].x = Mathf.Lerp(val2.x, val2.z, num5);
					mTempUVs[1].x = mTempUVs[0].x;
					mTempUVs[2].x = Mathf.Lerp(val2.x, val2.z, num6);
					mTempUVs[3].x = mTempUVs[2].x;
					mTempUVs[0].y = Mathf.Lerp(val2.y, val2.w, num3);
					mTempUVs[1].y = Mathf.Lerp(val2.y, val2.w, num4);
					mTempUVs[2].y = mTempUVs[1].y;
					mTempUVs[3].y = mTempUVs[0].y;
					float num7 = (mInvert ? (mFillAmount * 2f - (float)(1 - j)) : (fillAmount * 2f - (float)j));
					if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(num7), !mInvert, NGUIMath.RepeatIndex(j + 3, 4)))
					{
						for (int k = 0; k < 4; k++)
						{
							verts.Add(Vector2.op_Implicit(mTempPos[k]));
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
					float num8;
					float num9;
					if (l < 2)
					{
						num8 = 0f;
						num9 = 0.5f;
					}
					else
					{
						num8 = 0.5f;
						num9 = 1f;
					}
					float num10;
					float num11;
					if (l == 0 || l == 3)
					{
						num10 = 0f;
						num11 = 0.5f;
					}
					else
					{
						num10 = 0.5f;
						num11 = 1f;
					}
					mTempPos[0].x = Mathf.Lerp(val.x, val.z, num8);
					mTempPos[1].x = mTempPos[0].x;
					mTempPos[2].x = Mathf.Lerp(val.x, val.z, num9);
					mTempPos[3].x = mTempPos[2].x;
					mTempPos[0].y = Mathf.Lerp(val.y, val.w, num10);
					mTempPos[1].y = Mathf.Lerp(val.y, val.w, num11);
					mTempPos[2].y = mTempPos[1].y;
					mTempPos[3].y = mTempPos[0].y;
					mTempUVs[0].x = Mathf.Lerp(val2.x, val2.z, num8);
					mTempUVs[1].x = mTempUVs[0].x;
					mTempUVs[2].x = Mathf.Lerp(val2.x, val2.z, num9);
					mTempUVs[3].x = mTempUVs[2].x;
					mTempUVs[0].y = Mathf.Lerp(val2.y, val2.w, num10);
					mTempUVs[1].y = Mathf.Lerp(val2.y, val2.w, num11);
					mTempUVs[2].y = mTempUVs[1].y;
					mTempUVs[3].y = mTempUVs[0].y;
					float num12 = ((!mInvert) ? (mFillAmount * 4f - (float)(3 - NGUIMath.RepeatIndex(l + 2, 4))) : (mFillAmount * 4f - (float)NGUIMath.RepeatIndex(l + 2, 4)));
					if (RadialCut(mTempPos, mTempUVs, Mathf.Clamp01(num12), mInvert, NGUIMath.RepeatIndex(l + 2, 4)))
					{
						for (int m = 0; m < 4; m++)
						{
							verts.Add(Vector2.op_Implicit(mTempPos[m]));
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
			verts.Add(Vector2.op_Implicit(mTempPos[n]));
			uvs.Add(mTempUVs[n]);
			cols.Add(item);
		}
	}

	private void AdvancedFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b84: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_09d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fa: Unknown result type (might be due to invalid IL or missing references)
		Texture val = mainTexture;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		Vector4 val2 = border * pixelSize;
		if (val2.x == 0f && val2.y == 0f && val2.z == 0f && val2.w == 0f)
		{
			SimpleFill(verts, uvs, cols);
			return;
		}
		Color col = drawingColor.GammaToLinearSpace();
		Vector4 val3 = drawingDimensions;
		Vector2 val4 = default(Vector2);
		((Vector2)(ref val4))._002Ector(((Rect)(ref mInnerUV)).width * (float)val.width, ((Rect)(ref mInnerUV)).height * (float)val.height);
		val4 *= pixelSize;
		if (val4.x < 1f)
		{
			val4.x = 1f;
		}
		if (val4.y < 1f)
		{
			val4.y = 1f;
		}
		mTempPos[0].x = val3.x;
		mTempPos[0].y = val3.y;
		mTempPos[3].x = val3.z;
		mTempPos[3].y = val3.w;
		if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
		{
			mTempPos[1].x = mTempPos[0].x + val2.z;
			mTempPos[2].x = mTempPos[3].x - val2.x;
			mTempUVs[3].x = ((Rect)(ref mOuterUV)).xMin;
			mTempUVs[2].x = ((Rect)(ref mInnerUV)).xMin;
			mTempUVs[1].x = ((Rect)(ref mInnerUV)).xMax;
			mTempUVs[0].x = ((Rect)(ref mOuterUV)).xMax;
		}
		else
		{
			mTempPos[1].x = mTempPos[0].x + val2.x;
			mTempPos[2].x = mTempPos[3].x - val2.z;
			mTempUVs[0].x = ((Rect)(ref mOuterUV)).xMin;
			mTempUVs[1].x = ((Rect)(ref mInnerUV)).xMin;
			mTempUVs[2].x = ((Rect)(ref mInnerUV)).xMax;
			mTempUVs[3].x = ((Rect)(ref mOuterUV)).xMax;
		}
		if (mFlip == Flip.Vertically || mFlip == Flip.Both)
		{
			mTempPos[1].y = mTempPos[0].y + val2.w;
			mTempPos[2].y = mTempPos[3].y - val2.y;
			mTempUVs[3].y = ((Rect)(ref mOuterUV)).yMin;
			mTempUVs[2].y = ((Rect)(ref mInnerUV)).yMin;
			mTempUVs[1].y = ((Rect)(ref mInnerUV)).yMax;
			mTempUVs[0].y = ((Rect)(ref mOuterUV)).yMax;
		}
		else
		{
			mTempPos[1].y = mTempPos[0].y + val2.y;
			mTempPos[2].y = mTempPos[3].y - val2.w;
			mTempUVs[0].y = ((Rect)(ref mOuterUV)).yMin;
			mTempUVs[1].y = ((Rect)(ref mInnerUV)).yMin;
			mTempUVs[2].y = ((Rect)(ref mInnerUV)).yMax;
			mTempUVs[3].y = ((Rect)(ref mOuterUV)).yMax;
		}
		for (int i = 0; i < 3; i++)
		{
			int num = i + 1;
			for (int j = 0; j < 3; j++)
			{
				if (centerType == AdvancedType.Invisible && i == 1 && j == 1)
				{
					continue;
				}
				int num2 = j + 1;
				if (i == 1 && j == 1)
				{
					if (centerType == AdvancedType.Tiled)
					{
						float x = mTempPos[i].x;
						float x2 = mTempPos[num].x;
						float y = mTempPos[j].y;
						float y2 = mTempPos[num2].y;
						float x3 = mTempUVs[i].x;
						float y3 = mTempUVs[j].y;
						for (float num3 = y; num3 < y2; num3 += val4.y)
						{
							float num4 = x;
							float num5 = mTempUVs[num2].y;
							float num6 = num3 + val4.y;
							if (num6 > y2)
							{
								num5 = Mathf.Lerp(y3, num5, (y2 - num3) / val4.y);
								num6 = y2;
							}
							for (; num4 < x2; num4 += val4.x)
							{
								float num7 = num4 + val4.x;
								float num8 = mTempUVs[num].x;
								if (num7 > x2)
								{
									num8 = Mathf.Lerp(x3, num8, (x2 - num4) / val4.x);
									num7 = x2;
								}
								Fill(verts, uvs, cols, num4, num7, num3, num6, x3, num8, y3, num5, col);
							}
						}
					}
					else if (centerType == AdvancedType.Sliced)
					{
						Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, col);
					}
				}
				else if (i == 1)
				{
					if ((j == 0 && bottomType == AdvancedType.Tiled) || (j == 2 && topType == AdvancedType.Tiled))
					{
						float x4 = mTempPos[i].x;
						float x5 = mTempPos[num].x;
						float y4 = mTempPos[j].y;
						float y5 = mTempPos[num2].y;
						float x6 = mTempUVs[i].x;
						float y6 = mTempUVs[j].y;
						float y7 = mTempUVs[num2].y;
						for (float num9 = x4; num9 < x5; num9 += val4.x)
						{
							float num10 = num9 + val4.x;
							float num11 = mTempUVs[num].x;
							if (num10 > x5)
							{
								num11 = Mathf.Lerp(x6, num11, (x5 - num9) / val4.x);
								num10 = x5;
							}
							Fill(verts, uvs, cols, num9, num10, y4, y5, x6, num11, y6, y7, col);
						}
					}
					else if ((j == 0 && bottomType != 0) || (j == 2 && topType != 0))
					{
						Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, col);
					}
				}
				else if (j == 1)
				{
					if ((i == 0 && leftType == AdvancedType.Tiled) || (i == 2 && rightType == AdvancedType.Tiled))
					{
						float x7 = mTempPos[i].x;
						float x8 = mTempPos[num].x;
						float y8 = mTempPos[j].y;
						float y9 = mTempPos[num2].y;
						float x9 = mTempUVs[i].x;
						float x10 = mTempUVs[num].x;
						float y10 = mTempUVs[j].y;
						for (float num12 = y8; num12 < y9; num12 += val4.y)
						{
							float num13 = mTempUVs[num2].y;
							float num14 = num12 + val4.y;
							if (num14 > y9)
							{
								num13 = Mathf.Lerp(y10, num13, (y9 - num12) / val4.y);
								num14 = y9;
							}
							Fill(verts, uvs, cols, x7, x8, num12, num14, x9, x10, y10, num13, col);
						}
					}
					else if ((i == 0 && leftType != 0) || (i == 2 && rightType != 0))
					{
						Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, col);
					}
				}
				else if ((j == 0 && bottomType != 0) || (j == 2 && topType != 0) || (i == 0 && leftType != 0) || (i == 2 && rightType != 0))
				{
					Fill(verts, uvs, cols, mTempPos[i].x, mTempPos[num].x, mTempPos[j].y, mTempPos[num2].y, mTempUVs[i].x, mTempUVs[num].x, mTempUVs[j].y, mTempUVs[num2].y, col);
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
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
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
