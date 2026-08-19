using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/NGUI Texture")]
[ExecuteInEditMode]
public class UITexture : UIBasicSprite
{
	[SerializeField]
	[HideInInspector]
	private Rect mRect = new Rect(0f, 0f, 1f, 1f);

	[SerializeField]
	[HideInInspector]
	private Texture mTexture;

	[HideInInspector]
	[SerializeField]
	private Material mMat;

	[HideInInspector]
	[SerializeField]
	private Shader mShader;

	[SerializeField]
	[HideInInspector]
	private Vector4 mBorder = Vector4.zero;

	[HideInInspector]
	[SerializeField]
	private bool mFixedAspect;

	[NonSerialized]
	private int mPMA = -1;

	public override Texture mainTexture
	{
		get
		{
			if ((Object)(object)mTexture != (Object)null)
			{
				return mTexture;
			}
			if ((Object)(object)mMat != (Object)null)
			{
				return mMat.mainTexture;
			}
			return null;
		}
		set
		{
			if ((Object)(object)mTexture != (Object)(object)value)
			{
				if ((Object)(object)drawCall != (Object)null && drawCall.widgetCount == 1 && (Object)(object)mMat == (Object)null)
				{
					mTexture = value;
					drawCall.mainTexture = value;
					return;
				}
				RemoveFromPanel();
				mTexture = value;
				mPMA = -1;
				MarkAsChanged();
			}
		}
	}

	public override Material material
	{
		get
		{
			return mMat;
		}
		set
		{
			if ((Object)(object)mMat != (Object)(object)value)
			{
				RemoveFromPanel();
				mShader = null;
				mMat = value;
				mPMA = -1;
				MarkAsChanged();
			}
		}
	}

	public override Shader shader
	{
		get
		{
			if ((Object)(object)mMat != (Object)null)
			{
				return mMat.shader;
			}
			if ((Object)(object)mShader == (Object)null)
			{
				mShader = Shader.Find("Unlit/Transparent Colored");
			}
			return mShader;
		}
		set
		{
			if ((Object)(object)mShader != (Object)(object)value)
			{
				if ((Object)(object)drawCall != (Object)null && drawCall.widgetCount == 1 && (Object)(object)mMat == (Object)null)
				{
					mShader = value;
					drawCall.shader = value;
					return;
				}
				RemoveFromPanel();
				mShader = value;
				mPMA = -1;
				mMat = null;
				MarkAsChanged();
			}
		}
	}

	public override bool premultipliedAlpha
	{
		get
		{
			if (mPMA == -1)
			{
				Material val = material;
				mPMA = (((Object)(object)val != (Object)null && (Object)(object)val.shader != (Object)null && ((Object)val.shader).name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public override Vector4 border
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mBorder;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (mBorder != value)
			{
				mBorder = value;
				MarkAsChanged();
			}
		}
	}

	public Rect uvRect
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mRect;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (mRect != value)
			{
				mRect = value;
				MarkAsChanged();
			}
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0173: Unknown result type (might be due to invalid IL or missing references)
			//IL_0178: Unknown result type (might be due to invalid IL or missing references)
			//IL_0202: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = base.pivotOffset;
			float num = (0f - val.x) * (float)mWidth;
			float num2 = (0f - val.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			if ((Object)(object)mTexture != (Object)null && mType != Type.Tiled)
			{
				int num5 = mTexture.width;
				int num6 = mTexture.height;
				int num7 = 0;
				int num8 = 0;
				float num9 = 1f;
				float num10 = 1f;
				if (num5 > 0 && num6 > 0 && (mType == Type.Simple || mType == Type.Filled))
				{
					if (((uint)num5 & (true ? 1u : 0u)) != 0)
					{
						num7++;
					}
					if (((uint)num6 & (true ? 1u : 0u)) != 0)
					{
						num8++;
					}
					num9 = 1f / (float)num5 * (float)mWidth;
					num10 = 1f / (float)num6 * (float)mHeight;
				}
				if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
				{
					num += (float)num7 * num9;
				}
				else
				{
					num3 -= (float)num7 * num9;
				}
				if (mFlip == Flip.Vertically || mFlip == Flip.Both)
				{
					num2 += (float)num8 * num10;
				}
				else
				{
					num4 -= (float)num8 * num10;
				}
			}
			float num11;
			float num12;
			if (mFixedAspect)
			{
				num11 = 0f;
				num12 = 0f;
			}
			else
			{
				Vector4 val2 = border;
				num11 = val2.x + val2.z;
				num12 = val2.y + val2.w;
			}
			float num13 = Mathf.Lerp(num, num3 - num11, mDrawRegion.x);
			float num14 = Mathf.Lerp(num2, num4 - num12, mDrawRegion.y);
			float num15 = Mathf.Lerp(num + num11, num3, mDrawRegion.z);
			float num16 = Mathf.Lerp(num2 + num12, num4, mDrawRegion.w);
			return new Vector4(num13, num14, num15, num16);
		}
	}

	public bool fixedAspect
	{
		get
		{
			return mFixedAspect;
		}
		set
		{
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			if (mFixedAspect != value)
			{
				mFixedAspect = value;
				mDrawRegion = new Vector4(0f, 0f, 1f, 1f);
				MarkAsChanged();
			}
		}
	}

	public override void MakePixelPerfect()
	{
		base.MakePixelPerfect();
		if (mType == Type.Tiled)
		{
			return;
		}
		Texture val = mainTexture;
		if (!((Object)(object)val == (Object)null) && (mType == Type.Simple || mType == Type.Filled || !base.hasBorder) && (Object)(object)val != (Object)null)
		{
			int num = val.width;
			int num2 = val.height;
			if ((num & 1) == 1)
			{
				num++;
			}
			if ((num2 & 1) == 1)
			{
				num2++;
			}
			base.width = num;
			base.height = num2;
		}
	}

	protected override void OnUpdate()
	{
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		base.OnUpdate();
		if (!mFixedAspect)
		{
			return;
		}
		Texture val = mainTexture;
		if ((Object)(object)val != (Object)null)
		{
			int num = val.width;
			int num2 = val.height;
			if ((num & 1) == 1)
			{
				num++;
			}
			if ((num2 & 1) == 1)
			{
				num2++;
			}
			float num3 = mWidth;
			float num4 = mHeight;
			float num5 = num3 / num4;
			float num6 = (float)num / (float)num2;
			if (num6 < num5)
			{
				float num7 = (num3 - num4 * num6) / num3 * 0.5f;
				base.drawRegion = new Vector4(num7, 0f, 1f - num7, 1f);
			}
			else
			{
				float num8 = (num4 - num3 / num6) / num4 * 0.5f;
				base.drawRegion = new Vector4(0f, num8, 1f, 1f - num8);
			}
		}
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		Texture val = mainTexture;
		if (!((Object)(object)val == (Object)null))
		{
			Rect val2 = default(Rect);
			((Rect)(ref val2))._002Ector(((Rect)(ref mRect)).x * (float)val.width, ((Rect)(ref mRect)).y * (float)val.height, (float)val.width * ((Rect)(ref mRect)).width, (float)val.height * ((Rect)(ref mRect)).height);
			Rect inner = val2;
			Vector4 val3 = border;
			((Rect)(ref inner)).xMin = ((Rect)(ref inner)).xMin + val3.x;
			((Rect)(ref inner)).yMin = ((Rect)(ref inner)).yMin + val3.y;
			((Rect)(ref inner)).xMax = ((Rect)(ref inner)).xMax - val3.z;
			((Rect)(ref inner)).yMax = ((Rect)(ref inner)).yMax - val3.w;
			float num = 1f / (float)val.width;
			float num2 = 1f / (float)val.height;
			((Rect)(ref val2)).xMin = ((Rect)(ref val2)).xMin * num;
			((Rect)(ref val2)).xMax = ((Rect)(ref val2)).xMax * num;
			((Rect)(ref val2)).yMin = ((Rect)(ref val2)).yMin * num2;
			((Rect)(ref val2)).yMax = ((Rect)(ref val2)).yMax * num2;
			((Rect)(ref inner)).xMin = ((Rect)(ref inner)).xMin * num;
			((Rect)(ref inner)).xMax = ((Rect)(ref inner)).xMax * num;
			((Rect)(ref inner)).yMin = ((Rect)(ref inner)).yMin * num2;
			((Rect)(ref inner)).yMax = ((Rect)(ref inner)).yMax * num2;
			int size = verts.size;
			Fill(verts, uvs, cols, val2, inner);
			if (onPostFill != null)
			{
				onPostFill(this, size, verts, uvs, cols);
			}
		}
	}
}
