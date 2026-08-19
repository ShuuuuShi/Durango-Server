using System;
using UnityEngine;

[AddComponentMenu("NGUI/UI/NGUI Unity2D Sprite")]
[ExecuteInEditMode]
public class UI2DSprite : UIBasicSprite
{
	[HideInInspector]
	[SerializeField]
	private Sprite mSprite;

	[HideInInspector]
	[SerializeField]
	private Material mMat;

	[SerializeField]
	[HideInInspector]
	private Shader mShader;

	[HideInInspector]
	[SerializeField]
	private Vector4 mBorder = Vector4.zero;

	[SerializeField]
	[HideInInspector]
	private bool mFixedAspect;

	[SerializeField]
	[HideInInspector]
	private float mPixelSize = 1f;

	public Sprite nextSprite;

	[NonSerialized]
	private int mPMA = -1;

	public Sprite sprite2D
	{
		get
		{
			return mSprite;
		}
		set
		{
			if ((Object)(object)mSprite != (Object)(object)value)
			{
				RemoveFromPanel();
				mSprite = value;
				nextSprite = null;
				CreatePanel();
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
				RemoveFromPanel();
				mShader = value;
				if ((Object)(object)mMat == (Object)null)
				{
					mPMA = -1;
					MarkAsChanged();
				}
			}
		}
	}

	public override Texture mainTexture
	{
		get
		{
			if ((Object)(object)mSprite != (Object)null)
			{
				return (Texture)(object)mSprite.texture;
			}
			if ((Object)(object)mMat != (Object)null)
			{
				return mMat.mainTexture;
			}
			return null;
		}
	}

	public override bool premultipliedAlpha
	{
		get
		{
			if (mPMA == -1)
			{
				Shader val = shader;
				mPMA = (((Object)(object)val != (Object)null && ((Object)val).name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public override float pixelSize => mPixelSize;

	public override Vector4 drawingDimensions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_026f: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_027f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0097: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0112: Unknown result type (might be due to invalid IL or missing references)
			//IL_0117: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012b: Unknown result type (might be due to invalid IL or missing references)
			//IL_013b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0140: Unknown result type (might be due to invalid IL or missing references)
			//IL_0309: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = base.pivotOffset;
			float num = (0f - val.x) * (float)mWidth;
			float num2 = (0f - val.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			if ((Object)(object)mSprite != (Object)null && mType != Type.Tiled)
			{
				Rect rect = mSprite.rect;
				int num5 = Mathf.RoundToInt(((Rect)(ref rect)).width);
				Rect rect2 = mSprite.rect;
				int num6 = Mathf.RoundToInt(((Rect)(ref rect2)).height);
				int num7 = Mathf.RoundToInt(mSprite.textureRectOffset.x);
				int num8 = Mathf.RoundToInt(mSprite.textureRectOffset.y);
				Rect rect3 = mSprite.rect;
				float num9 = ((Rect)(ref rect3)).width;
				Rect textureRect = mSprite.textureRect;
				int num10 = Mathf.RoundToInt(num9 - ((Rect)(ref textureRect)).width - mSprite.textureRectOffset.x);
				Rect rect4 = mSprite.rect;
				float num11 = ((Rect)(ref rect4)).height;
				Rect textureRect2 = mSprite.textureRect;
				int num12 = Mathf.RoundToInt(num11 - ((Rect)(ref textureRect2)).height - mSprite.textureRectOffset.y);
				float num13 = 1f;
				float num14 = 1f;
				if (num5 > 0 && num6 > 0 && (mType == Type.Simple || mType == Type.Filled))
				{
					if (((uint)num5 & (true ? 1u : 0u)) != 0)
					{
						num10++;
					}
					if (((uint)num6 & (true ? 1u : 0u)) != 0)
					{
						num12++;
					}
					num13 = 1f / (float)num5 * (float)mWidth;
					num14 = 1f / (float)num6 * (float)mHeight;
				}
				if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
				{
					num += (float)num10 * num13;
					num3 -= (float)num7 * num13;
				}
				else
				{
					num += (float)num7 * num13;
					num3 -= (float)num10 * num13;
				}
				if (mFlip == Flip.Vertically || mFlip == Flip.Both)
				{
					num2 += (float)num12 * num14;
					num4 -= (float)num8 * num14;
				}
				else
				{
					num2 += (float)num8 * num14;
					num4 -= (float)num12 * num14;
				}
			}
			float num15;
			float num16;
			if (mFixedAspect)
			{
				num15 = 0f;
				num16 = 0f;
			}
			else
			{
				Vector4 val2 = border * pixelSize;
				num15 = val2.x + val2.z;
				num16 = val2.y + val2.w;
			}
			float num17 = Mathf.Lerp(num, num3 - num15, mDrawRegion.x);
			float num18 = Mathf.Lerp(num2, num4 - num16, mDrawRegion.y);
			float num19 = Mathf.Lerp(num + num15, num3, mDrawRegion.z);
			float num20 = Mathf.Lerp(num2 + num16, num4, mDrawRegion.w);
			return new Vector4(num17, num18, num19, num20);
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

	protected override void OnUpdate()
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)nextSprite != (Object)null)
		{
			if ((Object)(object)nextSprite != (Object)(object)mSprite)
			{
				sprite2D = nextSprite;
			}
			nextSprite = null;
		}
		base.OnUpdate();
		if (!mFixedAspect)
		{
			return;
		}
		Texture val = mainTexture;
		if ((Object)(object)val != (Object)null)
		{
			Rect rect = mSprite.rect;
			int num = Mathf.RoundToInt(((Rect)(ref rect)).width);
			Rect rect2 = mSprite.rect;
			int num2 = Mathf.RoundToInt(((Rect)(ref rect2)).height);
			int num3 = Mathf.RoundToInt(mSprite.textureRectOffset.x);
			int num4 = Mathf.RoundToInt(mSprite.textureRectOffset.y);
			Rect rect3 = mSprite.rect;
			float num5 = ((Rect)(ref rect3)).width;
			Rect textureRect = mSprite.textureRect;
			int num6 = Mathf.RoundToInt(num5 - ((Rect)(ref textureRect)).width - mSprite.textureRectOffset.x);
			Rect rect4 = mSprite.rect;
			float num7 = ((Rect)(ref rect4)).height;
			Rect textureRect2 = mSprite.textureRect;
			int num8 = Mathf.RoundToInt(num7 - ((Rect)(ref textureRect2)).height - mSprite.textureRectOffset.y);
			num += num3 + num6;
			num2 += num8 + num4;
			float num9 = mWidth;
			float num10 = mHeight;
			float num11 = num9 / num10;
			float num12 = (float)num / (float)num2;
			if (num12 < num11)
			{
				float num13 = (num9 - num10 * num12) / num9 * 0.5f;
				base.drawRegion = new Vector4(num13, 0f, 1f - num13, 1f);
			}
			else
			{
				float num14 = (num10 - num9 / num12) / num10 * 0.5f;
				base.drawRegion = new Vector4(0f, num14, 1f, 1f - num14);
			}
		}
	}

	public override void MakePixelPerfect()
	{
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		base.MakePixelPerfect();
		if (mType == Type.Tiled)
		{
			return;
		}
		Texture val = mainTexture;
		if (!((Object)(object)val == (Object)null) && (mType == Type.Simple || mType == Type.Filled || !base.hasBorder) && (Object)(object)val != (Object)null)
		{
			Rect rect = mSprite.rect;
			int num = Mathf.RoundToInt(((Rect)(ref rect)).width);
			int num2 = Mathf.RoundToInt(((Rect)(ref rect)).height);
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

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		Texture val = mainTexture;
		if (!((Object)(object)val == (Object)null))
		{
			Rect val2 = (Rect)((!((Object)(object)mSprite != (Object)null)) ? new Rect(0f, 0f, (float)val.width, (float)val.height) : mSprite.textureRect);
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
