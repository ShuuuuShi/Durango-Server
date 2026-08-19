using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("NGUI/UI/Atlas")]
public class UIAtlas : MonoBehaviour
{
	[Serializable]
	private class Sprite
	{
		public string name = "Unity Bug";

		public Rect outer = new Rect(0f, 0f, 1f, 1f);

		public Rect inner = new Rect(0f, 0f, 1f, 1f);

		public bool rotated;

		public float paddingLeft;

		public float paddingRight;

		public float paddingTop;

		public float paddingBottom;

		public bool hasPadding => paddingLeft != 0f || paddingRight != 0f || paddingTop != 0f || paddingBottom != 0f;
	}

	private enum Coordinates
	{
		Pixels,
		TexCoords
	}

	[SerializeField]
	[HideInInspector]
	private Material material;

	[SerializeField]
	[HideInInspector]
	private List<UISpriteData> mSprites = new List<UISpriteData>();

	[HideInInspector]
	[SerializeField]
	private float mPixelSize = 1f;

	[SerializeField]
	[HideInInspector]
	private UIAtlas mReplacement;

	[SerializeField]
	[HideInInspector]
	private Coordinates mCoordinates;

	[SerializeField]
	[HideInInspector]
	private List<Sprite> sprites = new List<Sprite>();

	private int mPMA = -1;

	private Dictionary<string, int> mSpriteIndices = new Dictionary<string, int>();

	public Material spriteMaterial
	{
		get
		{
			return (!((Object)(object)mReplacement != (Object)null)) ? material : mReplacement.spriteMaterial;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.spriteMaterial = value;
				return;
			}
			if ((Object)(object)material == (Object)null)
			{
				mPMA = 0;
				material = value;
				return;
			}
			MarkAsChanged();
			mPMA = -1;
			material = value;
			MarkAsChanged();
		}
	}

	public bool premultipliedAlpha
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.premultipliedAlpha;
			}
			if (mPMA == -1)
			{
				Material val = spriteMaterial;
				mPMA = (((Object)(object)val != (Object)null && (Object)(object)val.shader != (Object)null && ((Object)val.shader).name.Contains("Premultiplied")) ? 1 : 0);
			}
			return mPMA == 1;
		}
	}

	public List<UISpriteData> spriteList
	{
		get
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				return mReplacement.spriteList;
			}
			if (mSprites.Count == 0)
			{
				Upgrade();
			}
			return mSprites;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.spriteList = value;
			}
			else
			{
				mSprites = value;
			}
		}
	}

	public Texture texture => ((Object)(object)mReplacement != (Object)null) ? mReplacement.texture : ((!((Object)(object)material != (Object)null)) ? null : material.mainTexture);

	public float pixelSize
	{
		get
		{
			return (!((Object)(object)mReplacement != (Object)null)) ? mPixelSize : mReplacement.pixelSize;
		}
		set
		{
			if ((Object)(object)mReplacement != (Object)null)
			{
				mReplacement.pixelSize = value;
				return;
			}
			float num = Mathf.Clamp(value, 0.25f, 4f);
			if (mPixelSize != num)
			{
				mPixelSize = num;
				MarkAsChanged();
			}
		}
	}

	public UIAtlas replacement
	{
		get
		{
			return mReplacement;
		}
		set
		{
			UIAtlas uIAtlas = value;
			if ((Object)(object)uIAtlas == (Object)(object)this)
			{
				uIAtlas = null;
			}
			if ((Object)(object)mReplacement != (Object)(object)uIAtlas)
			{
				if ((Object)(object)uIAtlas != (Object)null && (Object)(object)uIAtlas.replacement == (Object)(object)this)
				{
					uIAtlas.replacement = null;
				}
				if ((Object)(object)mReplacement != (Object)null)
				{
					MarkAsChanged();
				}
				mReplacement = uIAtlas;
				if ((Object)(object)uIAtlas != (Object)null)
				{
					material = null;
				}
				MarkAsChanged();
			}
		}
	}

	public event Action<UIAtlas, UISpriteData> OnGetSprite;

	public UISpriteData GetSprite(string name)
	{
		if ((Object)(object)mReplacement != (Object)null)
		{
			return mReplacement.GetSprite(name);
		}
		if (!string.IsNullOrEmpty(name))
		{
			if (mSprites.Count == 0)
			{
				Upgrade();
			}
			if (mSprites.Count == 0)
			{
				return null;
			}
			if (mSpriteIndices.Count != mSprites.Count)
			{
				MarkSpriteListAsChanged();
			}
			if (mSpriteIndices.TryGetValue(name, out var value))
			{
				if (value > -1 && value < mSprites.Count)
				{
					if (this.OnGetSprite != null)
					{
						this.OnGetSprite(this, mSprites[value]);
					}
					return mSprites[value];
				}
				MarkSpriteListAsChanged();
				if (mSpriteIndices.TryGetValue(name, out value))
				{
					if (this.OnGetSprite != null)
					{
						this.OnGetSprite(this, mSprites[value]);
					}
					return mSprites[value];
				}
				return null;
			}
			return null;
		}
		return null;
	}

	public string GetRandomSprite(string startsWith)
	{
		if (GetSprite(startsWith) == null)
		{
			List<UISpriteData> list = spriteList;
			List<string> list2 = new List<string>();
			foreach (UISpriteData item in list)
			{
				if (item.name.StartsWith(startsWith))
				{
					list2.Add(item.name);
				}
			}
			return (list2.Count <= 0) ? null : list2[Random.Range(0, list2.Count)];
		}
		return startsWith;
	}

	public void MarkSpriteListAsChanged()
	{
		mSpriteIndices.Clear();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			mSpriteIndices[mSprites[i].name] = i;
		}
	}

	public void SortAlphabetically()
	{
		mSprites.Sort((UISpriteData s1, UISpriteData s2) => s1.name.CompareTo(s2.name));
	}

	public BetterList<string> GetListOfSprites()
	{
		if ((Object)(object)mReplacement != (Object)null)
		{
			return mReplacement.GetListOfSprites();
		}
		if (mSprites.Count == 0)
		{
			Upgrade();
		}
		BetterList<string> betterList = new BetterList<string>();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			UISpriteData uISpriteData = mSprites[i];
			if (uISpriteData != null && !string.IsNullOrEmpty(uISpriteData.name))
			{
				betterList.Add(uISpriteData.name);
			}
		}
		return betterList;
	}

	public BetterList<string> GetListOfSprites(string match)
	{
		if (Object.op_Implicit((Object)(object)mReplacement))
		{
			return mReplacement.GetListOfSprites(match);
		}
		if (string.IsNullOrEmpty(match))
		{
			return GetListOfSprites();
		}
		if (mSprites.Count == 0)
		{
			Upgrade();
		}
		BetterList<string> betterList = new BetterList<string>();
		int i = 0;
		for (int count = mSprites.Count; i < count; i++)
		{
			UISpriteData uISpriteData = mSprites[i];
			if (uISpriteData != null && !string.IsNullOrEmpty(uISpriteData.name) && string.Equals(match, uISpriteData.name, StringComparison.OrdinalIgnoreCase))
			{
				betterList.Add(uISpriteData.name);
				return betterList;
			}
		}
		string[] array = match.Split(new char[1] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
		for (int j = 0; j < array.Length; j++)
		{
			array[j] = array[j].ToLower();
		}
		int k = 0;
		for (int count2 = mSprites.Count; k < count2; k++)
		{
			UISpriteData uISpriteData2 = mSprites[k];
			if (uISpriteData2 == null || string.IsNullOrEmpty(uISpriteData2.name))
			{
				continue;
			}
			string text = uISpriteData2.name.ToLower();
			int num = 0;
			for (int l = 0; l < array.Length; l++)
			{
				if (text.Contains(array[l]))
				{
					num++;
				}
			}
			if (num == array.Length)
			{
				betterList.Add(uISpriteData2.name);
			}
		}
		return betterList;
	}

	private bool References(UIAtlas atlas)
	{
		if ((Object)(object)atlas == (Object)null)
		{
			return false;
		}
		if ((Object)(object)atlas == (Object)(object)this)
		{
			return true;
		}
		return (Object)(object)mReplacement != (Object)null && mReplacement.References(atlas);
	}

	public static bool CheckIfRelated(UIAtlas a, UIAtlas b)
	{
		if ((Object)(object)a == (Object)null || (Object)(object)b == (Object)null)
		{
			return false;
		}
		return (Object)(object)a == (Object)(object)b || a.References(b) || b.References(a);
	}

	public void MarkAsChanged()
	{
		if ((Object)(object)mReplacement != (Object)null)
		{
			mReplacement.MarkAsChanged();
		}
		UISprite[] array = NGUITools.FindActive<UISprite>();
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			UISprite uISprite = array[i];
			if (CheckIfRelated(this, uISprite.atlas))
			{
				UIAtlas atlas = uISprite.atlas;
				uISprite.atlas = null;
				uISprite.atlas = atlas;
			}
		}
		UIFont[] array2 = Resources.FindObjectsOfTypeAll(typeof(UIFont)) as UIFont[];
		int j = 0;
		for (int num2 = array2.Length; j < num2; j++)
		{
			UIFont uIFont = array2[j];
			if (CheckIfRelated(this, uIFont.atlas))
			{
				UIAtlas atlas2 = uIFont.atlas;
				uIFont.atlas = null;
				uIFont.atlas = atlas2;
			}
		}
		UILabel[] array3 = NGUITools.FindActive<UILabel>();
		int k = 0;
		for (int num3 = array3.Length; k < num3; k++)
		{
			UILabel uILabel = array3[k];
			if ((Object)(object)uILabel.bitmapFont != (Object)null && CheckIfRelated(this, uILabel.bitmapFont.atlas))
			{
				UIFont bitmapFont = uILabel.bitmapFont;
				uILabel.bitmapFont = null;
				uILabel.bitmapFont = bitmapFont;
			}
		}
	}

	private bool Upgrade()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)mReplacement))
		{
			return mReplacement.Upgrade();
		}
		if (mSprites.Count == 0 && sprites.Count > 0 && Object.op_Implicit((Object)(object)material))
		{
			Texture mainTexture = material.mainTexture;
			int width = ((!((Object)(object)mainTexture != (Object)null)) ? 512 : mainTexture.width);
			int height = ((!((Object)(object)mainTexture != (Object)null)) ? 512 : mainTexture.height);
			for (int i = 0; i < sprites.Count; i++)
			{
				Sprite sprite = sprites[i];
				Rect outer = sprite.outer;
				Rect inner = sprite.inner;
				if (mCoordinates == Coordinates.TexCoords)
				{
					NGUIMath.ConvertToPixels(outer, width, height, round: true);
					NGUIMath.ConvertToPixels(inner, width, height, round: true);
				}
				UISpriteData uISpriteData = new UISpriteData();
				uISpriteData.name = sprite.name;
				uISpriteData.x = Mathf.RoundToInt(((Rect)(ref outer)).xMin);
				uISpriteData.y = Mathf.RoundToInt(((Rect)(ref outer)).yMin);
				uISpriteData.width = Mathf.RoundToInt(((Rect)(ref outer)).width);
				uISpriteData.height = Mathf.RoundToInt(((Rect)(ref outer)).height);
				uISpriteData.paddingLeft = Mathf.RoundToInt(sprite.paddingLeft * ((Rect)(ref outer)).width);
				uISpriteData.paddingRight = Mathf.RoundToInt(sprite.paddingRight * ((Rect)(ref outer)).width);
				uISpriteData.paddingBottom = Mathf.RoundToInt(sprite.paddingBottom * ((Rect)(ref outer)).height);
				uISpriteData.paddingTop = Mathf.RoundToInt(sprite.paddingTop * ((Rect)(ref outer)).height);
				uISpriteData.borderLeft = Mathf.RoundToInt(((Rect)(ref inner)).xMin - ((Rect)(ref outer)).xMin);
				uISpriteData.borderRight = Mathf.RoundToInt(((Rect)(ref outer)).xMax - ((Rect)(ref inner)).xMax);
				uISpriteData.borderBottom = Mathf.RoundToInt(((Rect)(ref outer)).yMax - ((Rect)(ref inner)).yMax);
				uISpriteData.borderTop = Mathf.RoundToInt(((Rect)(ref inner)).yMin - ((Rect)(ref outer)).yMin);
				mSprites.Add(uISpriteData);
			}
			sprites.Clear();
			return true;
		}
		return false;
	}
}
