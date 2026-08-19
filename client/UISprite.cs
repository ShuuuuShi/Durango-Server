using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Sprite")]
public class UISprite : UIBasicSprite, ITextLink
{
	[HideInInspector]
	[SerializeField]
	private string mSpriteName;

	[HideInInspector]
	[SerializeField]
	private Rect mRect = new Rect(0f, 0f, 1f, 1f);

	[HideInInspector]
	[SerializeField]
	private bool mFillCenter = true;

	private bool _isPrefabLink;

	[HideInInspector]
	[SerializeField]
	private UIWidget _linkedObject;

	[NonSerialized]
	private UISpriteData mSprite;

	[NonSerialized]
	private bool mIsValidSpriteData;

	private float mPixelSize = 1f;

	private bool mPremultipliedAlpha;

	private Material mMaterial;

	public override Material material => mMaterial;

	public string spriteName
	{
		get
		{
			return mSpriteName;
		}
		set
		{
			SetSprite(value);
		}
	}

	public bool isValid => GetAtlasSprite() != null;

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

	public override Color color
	{
		get
		{
			return base.color;
		}
		set
		{
			if (_isPrefabLink)
			{
				_linkedObject.color = value;
			}
			base.color = value;
		}
	}

	public Color gradientTop
	{
		get
		{
			return mGradientTop;
		}
		set
		{
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
			return mGradientBottom;
		}
		set
		{
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
			UISpriteData atlasSprite = GetAtlasSprite();
			if (atlasSprite == null)
			{
				return base.border;
			}
			Vector4 result = new Vector4(atlasSprite.borderLeft, atlasSprite.borderBottom, atlasSprite.borderRight, atlasSprite.borderTop);
			result.x = Mathf.Max(0f, (float)atlasSprite.borderLeft - (float)atlasSprite.width * mRect.xMin);
			result.y = Mathf.Max(0f, (float)atlasSprite.borderBottom - (float)atlasSprite.height * mRect.yMin);
			result.z = Mathf.Max(0f, (float)atlasSprite.borderRight - (float)atlasSprite.width * (1f - mRect.xMax));
			result.w = Mathf.Max(0f, (float)atlasSprite.borderTop - (float)atlasSprite.height * (1f - mRect.yMax));
			return result;
		}
	}

	public Rect uvRect
	{
		get
		{
			return mRect;
		}
		set
		{
			if (mRect != value)
			{
				mRect = value;
				MarkAsChanged();
			}
		}
	}

	public override float pixelSize => mPixelSize;

	public override int minWidth
	{
		get
		{
			if (type == Type.Sliced || type == Type.Advanced)
			{
				float num = pixelSize;
				Vector4 vector = border * pixelSize;
				int num2 = Mathf.RoundToInt(vector.x + vector.z);
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
			if (type == Type.Sliced || type == Type.Advanced)
			{
				float num = pixelSize;
				Vector4 vector = border * pixelSize;
				int num2 = Mathf.RoundToInt(vector.y + vector.w);
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
			Vector2 vector = base.pivotOffset;
			float num = (0f - vector.x) * (float)mWidth;
			float num2 = (0f - vector.y) * (float)mHeight;
			float num3 = num + (float)mWidth;
			float num4 = num2 + (float)mHeight;
			UISpriteData atlasSprite = GetAtlasSprite();
			if (GetAtlasSprite() != null && mType != Type.Tiled && !mTile.on)
			{
				int num5 = atlasSprite.paddingLeft;
				int num6 = atlasSprite.paddingBottom;
				int num7 = atlasSprite.paddingRight;
				int num8 = atlasSprite.paddingTop;
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
				int num10 = atlasSprite.width + num5 + num7;
				int num11 = atlasSprite.height + num6 + num8;
				float num12 = 1f;
				float num13 = 1f;
				if (num10 > 0 && num11 > 0 && (mType == Type.Simple || mType == Type.Filled || (mType == Type.Sliced && !atlasSprite.hasBorder)))
				{
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
			Vector4 vector2 = ((!(material != null)) ? Vector4.zero : (border * pixelSize));
			float num14 = vector2.x + vector2.z;
			float num15 = vector2.y + vector2.w;
			float x = Mathf.Lerp(num, num3 - num14, mDrawRegion.x);
			float y = Mathf.Lerp(num2, num4 - num15, mDrawRegion.y);
			float z = Mathf.Lerp(num + num14, num3, mDrawRegion.z);
			float w = Mathf.Lerp(num2 + num15, num4, mDrawRegion.w);
			return new Vector4(x, y, z, w);
		}
	}

	public override bool premultipliedAlpha => mPremultipliedAlpha;

	public UISpriteData GetAtlasSprite()
	{
		if (!mIsValidSpriteData)
		{
			RefreshAtlasSprite();
		}
		return mSprite;
	}

	protected virtual void RefreshAtlasSprite()
	{
		mIsValidSpriteData = true;
		_isPrefabLink = false;
		if (string.IsNullOrEmpty(mSpriteName))
		{
			mSprite = null;
			UnlinkPrefab();
			return;
		}
		UISpriteManager uISpriteManager = ResourceSingleton<UISpriteManager>.Instance();
		if (!(uISpriteManager == null))
		{
			if (!uISpriteManager.TryGet(mSpriteName, out var atlas, out mSprite) && uISpriteManager.TryGetPreset(mSpriteName, out var result))
			{
				_isPrefabLink = true;
				LinkPrefab(result);
			}
			if (!_isPrefabLink)
			{
				UnlinkPrefab();
			}
			Material material = mMaterial;
			if (atlas == null)
			{
				mPixelSize = 1f;
				mPremultipliedAlpha = false;
				mMaterial = null;
				mSprite = null;
			}
			else
			{
				mPixelSize = atlas.pixelSize;
				mPremultipliedAlpha = atlas.premultipliedAlpha;
				mMaterial = atlas.spriteMaterial;
			}
			if (material != mMaterial)
			{
				RemoveFromPanel();
			}
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
		Texture texture = mainTexture;
		if (!(texture == null) && (mType == Type.Simple || mType == Type.Filled || !atlasSprite.hasBorder) && texture != null)
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
		RefreshAtlasSprite();
	}

	public override void OnFill(UIGeometry.Arguments arguments)
	{
		if (mSprite == null)
		{
			return;
		}
		Texture texture = mainTexture;
		if (!(texture == null))
		{
			BetterList<Vector3> verts = arguments.verts;
			BetterList<Vector2> uvs = arguments.uvs;
			BetterList<Color> cols = arguments.cols;
			Rect rect = new Rect(mSprite.x, mSprite.y, mSprite.width, mSprite.height);
			rect.Set(Mathf.Lerp(rect.xMin, rect.xMax, mRect.xMin), Mathf.Lerp(rect.yMin, rect.yMax, 1f - mRect.yMax), rect.width * mRect.width, rect.height * mRect.height);
			Vector4 vector = border;
			Rect rect2 = new Rect(rect.x + vector.x, rect.y + vector.w, rect.width - vector.x - vector.z, rect.height - vector.y - vector.w);
			rect = NGUIMath.ConvertToTexCoords(rect, texture.width, texture.height);
			rect2 = NGUIMath.ConvertToTexCoords(rect2, texture.width, texture.height);
			int size = verts.size;
			Fill(verts, uvs, cols, rect, rect2);
			onFillOffset = size;
			if (onPostFill != null)
			{
				onPostFill(this, size, arguments);
			}
		}
	}

	public void SetSprite(string sprite, string defaultSprite = null)
	{
		if (!(sprite == mSpriteName))
		{
			mIsValidSpriteData = false;
			mSpriteName = sprite;
			if (!string.IsNullOrEmpty(defaultSprite) && !string.IsNullOrEmpty(mSpriteName) && !ResourceSingleton<UISpriteManager>.Instance().TryGet(mSpriteName, out var _, out var _))
			{
				mSpriteName = defaultSprite;
			}
			RefreshAtlasSprite();
			MarkAsChanged();
		}
	}

	private void LinkPrefab(UIWidget prefab)
	{
		UnlinkPrefab();
		_linkedObject = base.gameObject.AddChild(prefab.gameObject).GetComponent<UIWidget>();
		_linkedObject.SetAnchor(base.gameObject, 0, 0, 0, 0);
		int num = 0;
		int num2 = 0;
		UIPanel uIPanel = UIUtility.FindComponentInParent<UIPanel>(base.gameObject);
		num = ((!(uIPanel == null)) ? uIPanel.depth : 0);
		num2 = _linkedObject.depth;
		using Reusable<Stack<Transform>> reusable = ReusableStack<Transform>.Pop();
		Stack<Transform> value = reusable.Value;
		value.Push(_linkedObject.transform);
		while (value.Count > 0)
		{
			Transform transform = value.Pop();
			UIRect component = transform.GetComponent<UIRect>();
			UIWidget uIWidget = component as UIWidget;
			if (uIWidget != null)
			{
				if (uIWidget.DrawPanel == null)
				{
					uIWidget.depth += num2;
				}
			}
			else
			{
				UIPanel uIPanel2 = component as UIPanel;
				if (uIPanel2 != null)
				{
					uIPanel2.depth += num;
					continue;
				}
			}
			int i = 0;
			for (int childCount = transform.childCount; i < childCount; i++)
			{
				value.Push(transform.GetChild(i));
			}
		}
		if (Application.isPlaying)
		{
			return;
		}
		value.Push(_linkedObject.transform);
		while (value.Count > 0)
		{
			Transform transform2 = value.Pop();
			transform2.gameObject.hideFlags = HideFlags.DontSave | HideFlags.NotEditable;
			int j = 0;
			for (int childCount2 = transform2.childCount; j < childCount2; j++)
			{
				value.Push(transform2.GetChild(j));
			}
		}
	}

	private void UnlinkPrefab()
	{
		if (!(_linkedObject == null))
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(_linkedObject.gameObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(_linkedObject.gameObject);
			}
			_linkedObject = null;
		}
	}

	LinkLayoutOption ITextLink.UpdateLayout(TextBuilder builder, int size)
	{
		UISpriteData atlasSprite = GetAtlasSprite();
		if (atlasSprite == null)
		{
			SetDimensions(size, size);
		}
		else
		{
			int num = atlasSprite.paddingLeft + atlasSprite.paddingRight + atlasSprite.width;
			int num2 = atlasSprite.paddingBottom + atlasSprite.paddingTop + atlasSprite.height;
			SetDimensions(num * size / num2, size);
		}
		return default(LinkLayoutOption);
	}
}
