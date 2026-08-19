using System;
using Durango.Logic.Social;
using Durango.Utils;
using UnityEngine;

[ResourcePath("portrait_builder")]
public class PortraitBuilder : ResourceSingleton<PortraitBuilder>
{
	[Serializable]
	public class PortraitTexturesGroup
	{
		public PortraitTextures[] Textures;
	}

	[Serializable]
	public class PortraitTextures
	{
		public Texture Mask;

		public Texture Ramped;

		public float GMaskRatio = 0.5f;
	}

	public struct Argument
	{
		public int Type;

		public int Background;

		public bool Male;

		public PortraitEmotion Emotion;

		public Color Skin;

		public Color Hair;

		public Color Eye;

		public Color Lip;

		public Color BgColor;

		public Texture Mask;

		public Vector2 MaskScale;

		public Vector2 MaskOffset;

		public string Preset;
	}

	private static int _mainTex;

	private static int _maskTex;

	private static int _bgTex;

	private static int _filterTex;

	private static int _skinColor;

	private static int _hairColor;

	private static int _eyeColor;

	private static int _lipColor;

	private static int _bgColor;

	private static int _gMaskRatio;

	[SerializeField]
	public PortraitTexturesGroup[] _maleTextureGroup;

	[SerializeField]
	public PortraitTexturesGroup[] _femaleTextureGroup;

	[SerializeField]
	public Texture[] _bgTextures;

	[SerializeField]
	private Material _portraitMaterial;

	[SerializeField]
	private Texture _defaultMask;

	private Color _defaultSkinColor;

	private Color _defaultHairColor;

	public Color DefaultEyeColor { get; private set; }

	public Color DefaultLipColor { get; private set; }

	protected void OnEnable()
	{
		_mainTex = Shader.PropertyToID("_MainTex");
		_maskTex = Shader.PropertyToID("_MaskTex");
		_bgTex = Shader.PropertyToID("_BgTex");
		_filterTex = Shader.PropertyToID("_FilterTex");
		_skinColor = Shader.PropertyToID("_SkinColor");
		_hairColor = Shader.PropertyToID("_HairColor");
		_eyeColor = Shader.PropertyToID("_EyeColor");
		_lipColor = Shader.PropertyToID("_LipColor");
		_bgColor = Shader.PropertyToID("_BgColor");
		_gMaskRatio = Shader.PropertyToID("_GMaskRatio");
		Material portraitMaterial = _portraitMaterial;
		_defaultSkinColor = portraitMaterial.GetColor(_skinColor);
		_defaultHairColor = portraitMaterial.GetColor(_hairColor);
		DefaultEyeColor = portraitMaterial.GetColor(_eyeColor);
		DefaultLipColor = portraitMaterial.GetColor(_lipColor);
	}

	public static Argument MakeArgument(int type, int bg, Color bgColor, bool male, PortraitEmotion emotion, Color skin, Color hair, Color eye, Color lip)
	{
		Argument result = default(Argument);
		result.Type = type;
		result.Background = bg;
		result.Male = male;
		result.Emotion = emotion;
		result.Skin = skin;
		result.Hair = hair;
		result.Eye = eye;
		result.Lip = lip;
		result.BgColor = bgColor;
		result.Mask = ResourceSingleton<PortraitBuilder>.Instance()._defaultMask;
		result.MaskScale = Vector2.one;
		result.MaskOffset = Vector2.zero;
		result.Preset = null;
		return result;
	}

	public static Argument MakeRandomArgument(bool isMale, int key)
	{
		return MakeArgument(ResourceSingleton<PortraitBuilder>.Instance().GetRandomPortraitType(isMale, key), 0, Color.white, isMale, PortraitEmotion.Normal, ColorTableLoader.GetRandom("color_skin.raw", key), ColorTableLoader.GetRandom("color_hair.raw", key), ColorTableLoader.GetRandom("color_eyes.raw", key), ColorTableLoader.GetRandom((!isMale) ? "color_lips_female.raw" : "color_lips_male.raw", key));
	}

	public static Material CreateMaterial(Argument arg)
	{
		Material material = null;
		if (string.IsNullOrEmpty(arg.Preset))
		{
			material = UnityEngine.Object.Instantiate(ResourceSingleton<PortraitBuilder>.Instance()._portraitMaterial);
			RefreshPortraitMaterial(material, arg);
		}
		return material;
	}

	public static void Set(Argument arg, UITexture tex)
	{
		Material material = tex.material;
		bool flag;
		if (string.IsNullOrEmpty(arg.Preset))
		{
			if (material == null)
			{
				material = UnityEngine.Object.Instantiate(ResourceSingleton<PortraitBuilder>.Instance()._portraitMaterial);
				RefreshPortraitMaterial(material, arg);
				flag = true;
			}
			else
			{
				flag = RefreshPortraitMaterial(material, arg);
			}
		}
		else
		{
			if (material != null)
			{
				UnityEngine.Object.Destroy(material);
			}
			SetPresetPortrait(tex, arg.Preset);
			flag = false;
		}
		if (flag)
		{
			tex.mainTexture = null;
			tex.material = material;
			if (tex.panel != null)
			{
				tex.panel.RebuildAllDrawCalls();
			}
		}
	}

	private static void SetPresetPortrait(UITexture tex, string preset)
	{
		ResourceSingleton<UISpriteManager>.Instance().TryGet(preset, out var atlas, out var spriteData);
		if (spriteData == null)
		{
			tex.mainTexture = null;
			tex.material = null;
			return;
		}
		Texture texture = atlas.texture;
		float num = texture.width;
		float num2 = texture.height;
		Vector4 vector = new Vector4((float)spriteData.x / num, (float)spriteData.y / num2, (float)spriteData.width / num, (float)spriteData.height / num2);
		vector.y = 1f - (vector.y + vector.w);
		float num3 = spriteData.width + spriteData.paddingLeft + spriteData.paddingRight;
		float num4 = spriteData.height + spriteData.paddingBottom + spriteData.paddingTop;
		Vector4 drawRegion = default(Vector4);
		drawRegion.x = (float)spriteData.paddingLeft / num3;
		drawRegion.y = (float)spriteData.paddingBottom / num4;
		drawRegion.z = drawRegion.x + (float)spriteData.width / num3;
		drawRegion.w = drawRegion.y + (float)spriteData.height / num4;
		Rect uvRect = new Rect(vector.x, vector.y, vector.z, vector.w);
		tex.material = null;
		tex.mainTexture = texture;
		tex.uvRect = uvRect;
		tex.drawRegion = drawRegion;
	}

	private static bool RefreshPortraitMaterial(Material mat, Argument arg)
	{
		ResourceSingleton<PortraitBuilder>.Instance().GetTexture(arg.Type, arg.Male, arg.Emotion, out var rampedBase, out var maskTex, out var gMaskRatio);
		Texture bgTexture = ResourceSingleton<PortraitBuilder>.Instance().GetBgTexture(arg.Background);
		if (arg.Skin == Color.clear)
		{
			arg.Skin = ResourceSingleton<PortraitBuilder>.Instance()._defaultSkinColor;
		}
		if (arg.Hair == Color.clear)
		{
			arg.Hair = ResourceSingleton<PortraitBuilder>.Instance()._defaultHairColor;
		}
		if (arg.Eye == Color.clear)
		{
			arg.Eye = ResourceSingleton<PortraitBuilder>.Instance().DefaultEyeColor;
		}
		if (arg.Lip == Color.clear)
		{
			arg.Lip = ResourceSingleton<PortraitBuilder>.Instance().DefaultLipColor;
		}
		Texture texture = mat.GetTexture(_mainTex);
		Texture texture2 = mat.GetTexture(_maskTex);
		Texture texture3 = mat.GetTexture(_bgTex);
		Texture texture4 = mat.GetTexture(_filterTex);
		Color color = mat.GetColor(_skinColor);
		Color color2 = mat.GetColor(_hairColor);
		Color color3 = mat.GetColor(_eyeColor);
		Color color4 = mat.GetColor(_lipColor);
		Color color5 = mat.GetColor(_bgColor);
		float @float = mat.GetFloat(_gMaskRatio);
		Vector2 textureScale = mat.GetTextureScale("_FilterTex");
		Vector2 textureOffset = mat.GetTextureOffset("_FilterTex");
		bool result = false;
		if (rampedBase != texture)
		{
			mat.SetTexture(_mainTex, rampedBase);
			result = true;
		}
		if (texture2 != maskTex)
		{
			mat.SetTexture(_maskTex, maskTex);
			result = true;
		}
		if (texture3 != bgTexture)
		{
			mat.SetTexture(_bgTex, bgTexture);
			result = true;
		}
		if (color != arg.Skin)
		{
			mat.SetColor(_skinColor, arg.Skin);
			result = true;
		}
		if (color2 != arg.Hair)
		{
			mat.SetColor(_hairColor, arg.Hair);
			result = true;
		}
		if (color3 != arg.Eye)
		{
			mat.SetColor(_eyeColor, arg.Eye);
			result = true;
		}
		if (color4 != arg.Lip)
		{
			mat.SetColor(_lipColor, arg.Lip);
			result = true;
		}
		if (color5 != arg.BgColor)
		{
			mat.SetColor(_bgColor, arg.BgColor);
			result = true;
		}
		if (texture4 != arg.Mask)
		{
			mat.SetTexture(_filterTex, arg.Mask);
			result = true;
		}
		if (textureScale != arg.MaskScale)
		{
			mat.SetTextureScale("_FilterTex", arg.MaskScale);
			result = true;
		}
		if (textureOffset != arg.MaskOffset)
		{
			mat.SetTextureOffset("_FilterTex", arg.MaskOffset);
			result = true;
		}
		if (Math.Abs(@float - gMaskRatio) > float.Epsilon)
		{
			mat.SetFloat(_gMaskRatio, gMaskRatio);
			result = true;
		}
		return result;
	}

	private void GetTexture(int type, bool isMale, PortraitEmotion emotion, out Texture rampedBase, out Texture maskTex, out float gMaskRatio)
	{
		PortraitTexturesGroup[] array = ((!isMale) ? _femaleTextureGroup : _maleTextureGroup);
		PortraitTexturesGroup portraitTexturesGroup = array[Mathf.Clamp(type, 0, array.Length - 1)];
		PortraitTextures portraitTextures = null;
		if (emotion > PortraitEmotion.None && emotion < PortraitEmotion.Count)
		{
			portraitTextures = portraitTexturesGroup.Textures[(int)emotion];
		}
		else
		{
			Debug.LogError(string.Concat(emotion, " portrait type is invalid"));
		}
		rampedBase = portraitTextures?.Ramped;
		maskTex = portraitTextures?.Mask;
		gMaskRatio = portraitTextures?.GMaskRatio ?? 0.5f;
	}

	private Texture GetBgTexture(int index)
	{
		if (_bgTextures == null)
		{
			return null;
		}
		int num = _bgTextures.Length;
		index = Mathf.Clamp(index, 0, num);
		if (index < num)
		{
			return _bgTextures[index];
		}
		return null;
	}

	public int GetPortraitCount(bool male)
	{
		PortraitTexturesGroup[] array = ((!male) ? _femaleTextureGroup : _maleTextureGroup);
		if (array == null)
		{
			return 0;
		}
		return array.Length;
	}

	private int GetRandomPortraitType(bool male, int hashKey)
	{
		int portraitCount = GetPortraitCount(male);
		return KUtility.GetRandomHashRange(0, portraitCount, hashKey);
	}

	public int GetPortraitBgCount()
	{
		if (_bgTextures == null)
		{
			return 0;
		}
		return _bgTextures.Length;
	}

	public static void FillEmptyBackground(string entityId, ref int bg, ref Color bgColor)
	{
		if (bg == -1 || !(bgColor != Color.clear))
		{
			System.Random random = new System.Random(entityId.GetHashCode());
			bg = random.Next(ResourceSingleton<PortraitBuilder>.Instance().GetPortraitBgCount());
			byte[] array = new byte[3];
			random.NextBytes(array);
			bgColor = new Color32(array[0], array[1], array[2], byte.MaxValue);
		}
	}
}
