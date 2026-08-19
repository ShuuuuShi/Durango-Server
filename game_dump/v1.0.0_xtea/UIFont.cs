using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/NGUI Font")]
public class UIFont : MonoBehaviour
{
	[HideInInspector]
	[SerializeField]
	private Material mMat;

	[HideInInspector]
	[SerializeField]
	private Rect mUVRect = new Rect(0f, 0f, 1f, 1f);

	[HideInInspector]
	[SerializeField]
	private BMFont mFont = new BMFont();

	[HideInInspector]
	[SerializeField]
	private UIAtlas mAtlas;

	[HideInInspector]
	[SerializeField]
	private UIFont mReplacement;

	[HideInInspector]
	[SerializeField]
	private List<BMSymbol> mSymbols = new List<BMSymbol>();

	[SerializeField]
	[HideInInspector]
	private Font mDynamicFont;

	[SerializeField]
	[HideInInspector]
	private int mDynamicFontSize = 16;

	[SerializeField]
	[HideInInspector]
	private FontStyle mDynamicFontStyle;

	[NonSerialized]
	private UISpriteData mSprite;

	private int mPMA = -1;

	private int mPacked = -1;

	public BMFont bmFont
	{
		get
		{
			return (!((Object)(object)mReplacement != (Object)null)) ? mFont : mReplacement.bmFont;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.bmFont = value;
			}
			else
			{
				mFont = value;
			}
		}
	}

	public int texWidth
	{
		get
		{
			return ((Object)(object)mReplacement != (Object)null) ? mReplacement.texWidth : ((mFont == null) ? 1 : mFont.texWidth);
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.texWidth = value;
			}
			else if (mFont != null)
			{
				mFont.texWidth = value;
			}
		}
	}

	public int texHeight
	{
		get
		{
			return ((Object)(object)mReplacement != (Object)null) ? mReplacement.texHeight : ((mFont == null) ? 1 : mFont.texHeight);
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.texHeight = value;
			}
			else if (mFont != null)
			{
				mFont.texHeight = value;
			}
		}
	}

	public bool hasSymbols => ((Object)(object)mReplacement != (Object)null) ? mReplacement.hasSymbols : (mSymbols != null && mSymbols.Count != 0);

	public List<BMSymbol> symbols => (!((Object)(object)mReplacement != (Object)null)) ? mSymbols : mReplacement.symbols;

	public UIAtlas atlas
	{
		get
		{
			return (!((Object)(object)mReplacement != (Object)null)) ? mAtlas : mReplacement.atlas;
		}
		set
		{
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.atlas = value;
			}
			else
			{
				if (!((Object)(object)mAtlas != (Object)(object)value))
				{
					return;
				}
				mPMA = -1;
				mAtlas = value;
				if ((Object)(object)mAtlas != (Object)null)
				{
					mMat = mAtlas.spriteMaterial;
					if (sprite != null)
					{
						mUVRect = uvRect;
					}
				}
				MarkAsChanged();
			}
		}
	}

	public Material material
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.material;
			}
			if ((Object)(object)mAtlas != (Object)null)
			{
				return mAtlas.spriteMaterial;
			}
			if ((Object)(object)mMat != (Object)null)
			{
				if ((Object)(object)mDynamicFont != (Object)null && (Object)(object)mMat != (Object)(object)mDynamicFont.material)
				{
					mMat.mainTexture = mDynamicFont.material.mainTexture;
				}
				return mMat;
			}
			if ((Object)(object)mDynamicFont != (Object)null)
			{
				return mDynamicFont.material;
			}
			return null;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.material = value;
			}
			else if ((Object)(object)mMat != (Object)(object)value)
			{
				mPMA = -1;
				mMat = value;
				MarkAsChanged();
			}
		}
	}

	[Obsolete("Use UIFont.premultipliedAlphaShader instead")]
	public bool premultipliedAlpha => premultipliedAlphaShader;

	public bool premultipliedAlphaShader
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.premultipliedAlphaShader;
			}
			if ((Object)(object)mAtlas != (Object)null)
			{
				return mAtlas.premultipliedAlpha;
			}
			if (mPMA == -1)
			{
				Material val = material;
				mPMA = (((Object)(object)val != (Object)null && (Object)(object)val.shader != (Object)null && ((Object)val.shader).name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public bool packedFontShader
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.packedFontShader;
			}
			if ((Object)(object)mAtlas != (Object)null)
			{
				return false;
			}
			if (mPacked == -1)
			{
				Material val = material;
				mPacked = (((Object)(object)val != (Object)null && (Object)(object)val.shader != (Object)null && ((Object)val.shader).name.Contains("Packed")) ? 1 : 0);
			}
			return mPacked == 1;
		}
	}

	public Texture2D texture
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.texture;
			}
			Material val = material;
			return (Texture2D)((!((Object)(object)val != (Object)null)) ? null : /*isinst with value type is only supported in some contexts*/);
		}
	}

	public Rect uvRect
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.uvRect;
			}
			return (Rect)((!((Object)(object)mAtlas != (Object)null) || sprite == null) ? new Rect(0f, 0f, 1f, 1f) : mUVRect);
		}
		set
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.uvRect = value;
			}
			else if (sprite == null && mUVRect != value)
			{
				mUVRect = value;
				MarkAsChanged();
			}
		}
	}

	public string spriteName
	{
		get
		{
			return (!((Object)(object)mReplacement != (Object)null)) ? mFont.spriteName : mReplacement.spriteName;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.spriteName = value;
			}
			else if (mFont.spriteName != value)
			{
				mFont.spriteName = value;
				MarkAsChanged();
			}
		}
	}

	public bool isValid => (Object)(object)mDynamicFont != (Object)null || mFont.isValid;

	[Obsolete("Use UIFont.defaultSize instead")]
	public int size
	{
		get
		{
			return defaultSize;
		}
		set
		{
			defaultSize = value;
		}
	}

	public int defaultSize
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.defaultSize;
			}
			if (isDynamic || mFont == null)
			{
				return mDynamicFontSize;
			}
			return mFont.charSize;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.defaultSize = value;
			}
			else
			{
				mDynamicFontSize = value;
			}
		}
	}

	public UISpriteData sprite
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.sprite;
			}
			if (mSprite == null && (Object)(object)mAtlas != (Object)null && !string.IsNullOrEmpty(mFont.spriteName))
			{
				mSprite = mAtlas.GetSprite(mFont.spriteName);
				if (mSprite == null)
				{
					mSprite = mAtlas.GetSprite(((Object)this).name);
				}
				if (mSprite == null)
				{
					mFont.spriteName = null;
				}
				else
				{
					UpdateUVRect();
				}
				int i = 0;
				for (int count = mSymbols.Count; i < count; i++)
				{
					symbols[i].MarkAsChanged();
				}
			}
			return mSprite;
		}
	}

	public UIFont replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			UIFont uIFont = value;
			if ((Object)(object)uIFont == (Object)(object)this)
			{
				uIFont = null;
			}
			if ((Object)(object)mReplacement != (Object)(object)uIFont)
			{
				if ((Object)(object)uIFont != (Object)null && (Object)(object)uIFont.replacement == (Object)(object)this)
				{
					uIFont.replacement = null;
				}
				if ((Object)(object)mReplacement != (Object)null)
				{
					MarkAsChanged();
				}
				mReplacement = uIFont;
				if ((Object)(object)uIFont != (Object)null)
				{
					mPMA = -1;
					mMat = null;
					mFont = null;
					mDynamicFont = null;
				}
				MarkAsChanged();
			}
		}
	}

	public bool isDynamic => (!((Object)(object)mReplacement != (Object)null)) ? ((Object)(object)mDynamicFont != (Object)null) : mReplacement.isDynamic;

	public Font dynamicFont
	{
		get
		{
			return (!((Object)(object)mReplacement != (Object)null)) ? mDynamicFont : mReplacement.dynamicFont;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.dynamicFont = value;
			}
			else if ((Object)(object)mDynamicFont != (Object)(object)value)
			{
				if ((Object)(object)mDynamicFont != (Object)null)
				{
					material = null;
				}
				mDynamicFont = value;
				MarkAsChanged();
			}
		}
	}

	public FontStyle dynamicFontStyle
	{
		get
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return (!((Object)(object)mReplacement != (Object)null)) ? mDynamicFontStyle : mReplacement.dynamicFontStyle;
		}
		set
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.dynamicFontStyle = value;
			}
			else if (mDynamicFontStyle != value)
			{
				mDynamicFontStyle = value;
				MarkAsChanged();
			}
		}
	}

	private Texture dynamicTexture
	{
		get
		{
			if (Object.op_Implicit((Object)(object)mReplacement))
			{
				return mReplacement.dynamicTexture;
			}
			if (isDynamic)
			{
				return mDynamicFont.material.mainTexture;
			}
			return null;
		}
	}

	private void Trim()
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		Texture val = mAtlas.texture;
		if ((Object)(object)val != (Object)null && mSprite != null)
		{
			Rect val2 = NGUIMath.ConvertToPixels(mUVRect, ((Texture)texture).width, ((Texture)texture).height, round: true);
			Rect val3 = default(Rect);
			((Rect)(ref val3))._002Ector((float)mSprite.x, (float)mSprite.y, (float)mSprite.width, (float)mSprite.height);
			int xMin = Mathf.RoundToInt(((Rect)(ref val3)).xMin - ((Rect)(ref val2)).xMin);
			int yMin = Mathf.RoundToInt(((Rect)(ref val3)).yMin - ((Rect)(ref val2)).yMin);
			int xMax = Mathf.RoundToInt(((Rect)(ref val3)).xMax - ((Rect)(ref val2)).xMin);
			int yMax = Mathf.RoundToInt(((Rect)(ref val3)).yMax - ((Rect)(ref val2)).yMin);
			mFont.Trim(xMin, yMin, xMax, yMax);
		}
	}

	private bool References(UIFont font)
	{
		if ((Object)(object)font == (Object)null)
		{
			return false;
		}
		if ((Object)(object)font == (Object)(object)this)
		{
			return true;
		}
		return (Object)(object)mReplacement != (Object)null && mReplacement.References(font);
	}

	public static bool CheckIfRelated(UIFont a, UIFont b)
	{
		if ((Object)(object)a == (Object)null || (Object)(object)b == (Object)null)
		{
			return false;
		}
		if (a.isDynamic && b.isDynamic && a.dynamicFont.fontNames[0] == b.dynamicFont.fontNames[0])
		{
			return true;
		}
		return (Object)(object)a == (Object)(object)b || a.References(b) || b.References(a);
	}

	public void MarkAsChanged()
	{
		if ((Object)(object)mReplacement != (Object)null)
		{
			mReplacement.MarkAsChanged();
		}
		mSprite = null;
		UILabel[] array = NGUITools.FindActive<UILabel>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UILabel uILabel = array[i];
			if (((Behaviour)uILabel).enabled && NGUITools.GetActive(((Component)uILabel).gameObject) && CheckIfRelated(this, uILabel.bitmapFont))
			{
				UIFont bitmapFont = uILabel.bitmapFont;
				uILabel.bitmapFont = null;
				uILabel.bitmapFont = bitmapFont;
			}
		}
		int j = 0;
		for (int count = symbols.Count; j < count; j++)
		{
			symbols[j].MarkAsChanged();
		}
	}

	public void UpdateUVRect()
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mAtlas == (Object)null)
		{
			return;
		}
		Texture val = mAtlas.texture;
		if ((Object)(object)val != (Object)null)
		{
			mUVRect = new Rect((float)(mSprite.x - mSprite.paddingLeft), (float)(mSprite.y - mSprite.paddingTop), (float)(mSprite.width + mSprite.paddingLeft + mSprite.paddingRight), (float)(mSprite.height + mSprite.paddingTop + mSprite.paddingBottom));
			mUVRect = NGUIMath.ConvertToTexCoords(mUVRect, val.width, val.height);
			if (mSprite.hasPadding)
			{
				Trim();
			}
		}
	}

	private BMSymbol GetSymbol(string sequence, bool createIfMissing)
	{
		int i = 0;
		for (int count = mSymbols.Count; i < count; i++)
		{
			BMSymbol bMSymbol = mSymbols[i];
			if (bMSymbol.sequence == sequence)
			{
				return bMSymbol;
			}
		}
		if (createIfMissing)
		{
			BMSymbol bMSymbol2 = new BMSymbol();
			bMSymbol2.sequence = sequence;
			mSymbols.Add(bMSymbol2);
			return bMSymbol2;
		}
		return null;
	}

	public BMSymbol MatchSymbol(string text, int offset, int textLength)
	{
		int count = mSymbols.Count;
		if (count == 0)
		{
			return null;
		}
		textLength -= offset;
		for (int i = 0; i < count; i++)
		{
			BMSymbol bMSymbol = mSymbols[i];
			int length = bMSymbol.length;
			if (length == 0 || textLength < length)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < length; j++)
			{
				if (text[offset + j] != bMSymbol.sequence[j])
				{
					flag = false;
					break;
				}
			}
			if (flag && bMSymbol.Validate(atlas))
			{
				return bMSymbol;
			}
		}
		return null;
	}

	public void AddSymbol(string sequence, string spriteName)
	{
		BMSymbol symbol = GetSymbol(sequence, createIfMissing: true);
		symbol.spriteName = spriteName;
		MarkAsChanged();
	}

	public void RemoveSymbol(string sequence)
	{
		BMSymbol symbol = GetSymbol(sequence, createIfMissing: false);
		if (symbol != null)
		{
			symbols.Remove(symbol);
		}
		MarkAsChanged();
	}

	public void RenameSymbol(string before, string after)
	{
		BMSymbol symbol = GetSymbol(before, createIfMissing: false);
		if (symbol != null)
		{
			symbol.sequence = after;
		}
		MarkAsChanged();
	}

	public bool UsesSprite(string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			if (s.Equals(spriteName))
			{
				return true;
			}
			int i = 0;
			for (int count = symbols.Count; i < count; i++)
			{
				BMSymbol bMSymbol = symbols[i];
				if (s.Equals(bMSymbol.spriteName))
				{
					return true;
				}
			}
		}
		return false;
	}
}
