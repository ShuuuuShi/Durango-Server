using System;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Sprite")]
public class UISprite : UIBasicSprite
{
	[HideInInspector]
	[SerializeField]
	private UIAtlas[] mAtlases;

	[SerializeField]
	[HideInInspector]
	private UIAtlas mAtlas;

	[HideInInspector]
	[SerializeField]
	private string mSpriteName;

	[SerializeField]
	[HideInInspector]
	private bool mFillCenter = true;

	[NonSerialized]
	protected UISpriteData mSprite;

	[NonSerialized]
	private bool mSpriteSet;

	public override Material material => (!((Object)(object)mAtlas != (Object)null)) ? null : mAtlas.spriteMaterial;

	public UIAtlas atlas
	{
		get
		{
			return mAtlas;
		}
		set
		{
			if ((Object)(object)mAtlas != (Object)(object)value)
			{
				RemoveFromPanel();
				mAtlas = value;
				mSpriteSet = false;
				mSprite = null;
				if (string.IsNullOrEmpty(mSpriteName) && (Object)(object)mAtlas != (Object)null && mAtlas.spriteList.Count > 0)
				{
					SetAtlasSprite(mAtlas.spriteList[0]);
					mSpriteName = mSprite.name;
				}
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					string text = mSpriteName;
					mSpriteName = string.Empty;
					spriteName = text;
					MarkAsChanged();
				}
			}
		}
	}

	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			if (string.IsNullOrEmpty(value))
			{
				if (!string.IsNullOrEmpty(mSpriteName))
				{
					mSpriteName = string.Empty;
					mSprite = null;
					mChanged = true;
					mSpriteSet = false;
				}
			}
			else
			{
				if (!(mSpriteName != value))
				{
					return;
				}
				UIAtlas uIAtlas = mAtlas;
				int num = ((mAtlases != null) ? mAtlases.Length : 0);
				if (num > 1)
				{
					for (int i = 0; i < num; i++)
					{
						if (mAtlases[i].GetSprite(value) != null)
						{
							uIAtlas = mAtlases[i];
							break;
						}
					}
				}
				else if (num == 1)
				{
					uIAtlas = mAtlases[0];
				}
				mSpriteName = value;
				mSprite = null;
				mChanged = true;
				mSpriteSet = false;
				atlas = uIAtlas;
			}
		}
	}

	public bool isValid => GetAtlasSprite() != null;

	[Obsolete("Use 'centerType' instead")]
	public bool fillCenter
	{
		get
		{
			return centerType != AdvancedType.Invisible;
		}
		set
		{
			if (value != (centerType != AdvancedType.Invisible))
			{
				centerType = (value ? AdvancedType.Sliced : AdvancedType.Invisible);
				MarkAsChanged();
			}
		}
	}

	public bool applyGradient
	{
		get
		{
			return mApplyGradient;
		}
		set
		{
			if (mApplyGradient != value)
			{
				mApplyGradient = value;
				MarkAsChanged();
			}
		}
	}

	public Color gradientTop
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mGradientTop;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (mGradientTop != value)
			{
				mGradientTop = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public Color gradientBottom
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return mGradientBottom;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			if (mGradientBottom != value)
			{
				mGradientBottom = value;
				if (mApplyGradient)
				{
					MarkAsChanged();
				}
			}
		}
	}

	public override Vector4 border
	{
		get
		{
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			UISpriteData atlasSprite = GetAtlasSprite();
			if (atlasSprite == null)
			{
				return base.border;
			}
			return new Vector4((float)atlasSprite.borderLeft, (float)atlasSprite.borderBottom, (float)atlasSprite.borderRight, (float)atlasSprite.borderTop);
		}
	}

	public override float pixelSize => (!((Object)(object)mAtlas != (Object)null)) ? 1f : mAtlas.pixelSize;

	public override int minWidth
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (type == Type.Sliced || type == Type.Advanced)
			{
				float num = pixelSize;
				Vector4 val = border * pixelSize;
				int num2 = Mathf.RoundToInt(val.x + val.z);
				UISpriteData atlasSprite = GetAtlasSprite();
				if (atlasSprite != null)
				{
					num2 += Mathf.RoundToInt(num * (float)(atlasSprite.paddingLeft + atlasSprite.paddingRight));
				}
				return Mathf.Max(base.minWidth, ((num2 & 1) != 1) ? num2 : (num2 + 1));
			}
			return base.minWidth;
		}
	}

	public override int minHeight
	{
		get
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if (type == Type.Sliced || type == Type.Advanced)
			{
				float num = pixelSize;
				Vector4 val = border * pixelSize;
				int num2 = Mathf.RoundToInt(val.y + val.w);
				UISpriteData atlasSprite = GetAtlasSprite();
				if (atlasSprite != null)
				{
					num2 += Mathf.RoundToInt(num * (float)(atlasSprite.paddingTop + atlasSprite.paddingBottom));
				}
				return Mathf.Max(base.minHeight, ((num2 & 1) != 1) ? num2 : (num2 + 1));
			}
			return base.minHeight;
		}
	}

	public override Vector4 drawingDimensions
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0244: Unknown result type (might be due to invalid IL or missing references)
			//IL_022f: Unknown result type (might be due to invalid IL or missing references)
			//IL_023a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0249: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
			Vector2 val = base.pivotOffset;
			float num = (0f - val.x) * (float)mWidth;
			float num2 = (0f - val.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			if (GetAtlasSprite() != null && mType != Type.Tiled)
			{
				int num5 = mSprite.paddingLeft;
				int num6 = mSprite.paddingBottom;
				int num7 = mSprite.paddingRight;
				int num8 = mSprite.paddingTop;
				if (mType != 0)
				{
					float num9 = pixelSize;
					if (num9 != 1f)
					{
						num5 = Mathf.RoundToInt(num9 * (float)num5);
						num6 = Mathf.RoundToInt(num9 * (float)num6);
						num7 = Mathf.RoundToInt(num9 * (float)num7);
						num8 = Mathf.RoundToInt(num9 * (float)num8);
					}
				}
				int num10 = mSprite.width + num5 + num7;
				int num11 = mSprite.height + num6 + num8;
				float num12 = 1f;
				float num13 = 1f;
				if (num10 > 0 && num11 > 0 && (mType == Type.Simple || mType == Type.Filled || (mType == Type.Sliced && !mSprite.hasBorder)))
				{
					if (((uint)num10 & (true ? 1u : 0u)) != 0)
					{
						num7++;
					}
					if (((uint)num11 & (true ? 1u : 0u)) != 0)
					{
						num8++;
					}
					num12 = 1f / (float)num10 * (float)mWidth;
					num13 = 1f / (float)num11 * (float)mHeight;
				}
				if (mFlip == Flip.Horizontally || mFlip == Flip.Both)
				{
					num += (float)num7 * num12;
					num3 -= (float)num5 * num12;
				}
				else
				{
					num += (float)num5 * num12;
					num3 -= (float)num7 * num12;
				}
				if (mFlip == Flip.Vertically || mFlip == Flip.Both)
				{
					num2 += (float)num8 * num13;
					num4 -= (float)num6 * num13;
				}
				else
				{
					num2 += (float)num6 * num13;
					num4 -= (float)num8 * num13;
				}
			}
			Vector4 val2 = ((!((Object)(object)mAtlas != (Object)null)) ? Vector4.zero : (border * pixelSize));
			float num14 = val2.x + val2.z;
			float num15 = val2.y + val2.w;
			float num16 = Mathf.Lerp(num, num3 - num14, mDrawRegion.x);
			float num17 = Mathf.Lerp(num2, num4 - num15, mDrawRegion.y);
			float num18 = Mathf.Lerp(num + num14, num3, mDrawRegion.z);
			float num19 = Mathf.Lerp(num2 + num15, num4, mDrawRegion.w);
			return new Vector4(num16, num17, num18, num19);
		}
	}

	public override bool premultipliedAlpha => (Object)(object)mAtlas != (Object)null && mAtlas.premultipliedAlpha;

	public UISpriteData GetAtlasSprite()
	{
		if (!mSpriteSet)
		{
			mSprite = null;
		}
		if (mSprite == null && (Object)(object)mAtlas != (Object)null)
		{
			if (!string.IsNullOrEmpty(mSpriteName))
			{
				UISpriteData sprite = mAtlas.GetSprite(mSpriteName);
				if (sprite == null)
				{
					return null;
				}
				SetAtlasSprite(sprite);
			}
			if (mSprite == null && mAtlas.spriteList.Count > 0)
			{
				UISpriteData uISpriteData = mAtlas.spriteList[0];
				if (uISpriteData == null)
				{
					return null;
				}
				SetAtlasSprite(uISpriteData);
				if (mSprite == null)
				{
					Debug.LogError((object)(((Object)mAtlas).name + " seems to have a null sprite!"));
					return null;
				}
				mSpriteName = mSprite.name;
			}
		}
		return mSprite;
	}

	protected void SetAtlasSprite(UISpriteData sp)
	{
		mChanged = true;
		mSpriteSet = true;
		if (sp != null)
		{
			mSprite = sp;
			mSpriteName = mSprite.name;
		}
		else
		{
			mSpriteName = ((mSprite == null) ? string.Empty : mSprite.name);
			mSprite = sp;
		}
	}

	public override void MakePixelPerfect()
	{
		if (!isValid)
		{
			return;
		}
		base.MakePixelPerfect();
		if (mType == Type.Tiled)
		{
			return;
		}
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null)
		{
			return;
		}
		Texture val = mainTexture;
		if (!((Object)(object)val == (Object)null) && (mType == Type.Simple || mType == Type.Filled || !atlasSprite.hasBorder) && (Object)(object)val != (Object)null)
		{
			int num = Mathf.RoundToInt(pixelSize * (float)(atlasSprite.width + atlasSprite.paddingLeft + atlasSprite.paddingRight));
			int num2 = Mathf.RoundToInt(pixelSize * (float)(atlasSprite.height + atlasSprite.paddingTop + atlasSprite.paddingBottom));
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

	protected override void OnInit()
	{
		if (!mFillCenter)
		{
			mFillCenter = true;
			centerType = AdvancedType.Invisible;
		}
		base.OnInit();
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (mChanged || !mSpriteSet)
		{
			mSpriteSet = true;
			mSprite = null;
			mChanged = true;
		}
	}

	public override void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		Texture val = mainTexture;
		if ((Object)(object)val == (Object)null)
		{
			return;
		}
		if (mSprite == null)
		{
			mSprite = atlas.GetSprite(spriteName);
		}
		if (mSprite != null)
		{
			Rect val2 = default(Rect);
			((Rect)(ref val2))._002Ector((float)mSprite.x, (float)mSprite.y, (float)mSprite.width, (float)mSprite.height);
			Rect val3 = default(Rect);
			((Rect)(ref val3))._002Ector((float)(mSprite.x + mSprite.borderLeft), (float)(mSprite.y + mSprite.borderTop), (float)(mSprite.width - mSprite.borderLeft - mSprite.borderRight), (float)(mSprite.height - mSprite.borderBottom - mSprite.borderTop));
			val2 = NGUIMath.ConvertToTexCoords(val2, val.width, val.height);
			val3 = NGUIMath.ConvertToTexCoords(val3, val.width, val.height);
			int size = verts.size;
			Fill(verts, uvs, cols, val2, val3);
			if (onPostFill != null)
			{
				onPostFill(this, size, verts, uvs, cols);
			}
		}
	}
}
