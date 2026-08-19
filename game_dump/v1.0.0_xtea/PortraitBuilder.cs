using System;
using Player;
using UnityEngine;

public class PortraitBuilder : KSingleton<PortraitBuilder>
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
	}

	[SerializeField]
	public PortraitTexturesGroup[] _maleTextureGroup;

	[SerializeField]
	public PortraitTexturesGroup[] _femaleTextureGroup;

	[SerializeField]
	public Texture[] _bgTextures;

	[SerializeField]
	private Material _portraitMaterial;

	private Color _defaultSkinColor;

	private Color _defaultHairColor;

	private Color _defaultEyeColor;

	private Color _defaultLipColor;

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

	private static int _filterTex_ST;

	public Color DefaultEyeColor => _defaultEyeColor;

	public Color DefaultLipColor => _defaultLipColor;

	protected override void OnAwake()
	{
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		base.OnAwake();
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
		_filterTex_ST = Shader.PropertyToID("_FilterTex_ST");
		_portraitMaterial = Object.Instantiate<Material>(_portraitMaterial);
		Material portraitMaterial = _portraitMaterial;
		_defaultSkinColor = portraitMaterial.GetColor(_skinColor);
		_defaultHairColor = portraitMaterial.GetColor(_hairColor);
		_defaultEyeColor = portraitMaterial.GetColor(_eyeColor);
		_defaultLipColor = portraitMaterial.GetColor(_lipColor);
	}

	public static Argument MakeArgument(int type, bool male, PortraitEmotion emotion, Texture2D mask = null)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		return MakeArgument(type, 0, Color.white, male, emotion, KSingleton<PortraitBuilder>.Instance()._defaultSkinColor, KSingleton<PortraitBuilder>.Instance()._defaultHairColor, KSingleton<PortraitBuilder>.Instance().DefaultEyeColor, KSingleton<PortraitBuilder>.Instance().DefaultLipColor, mask);
	}

	public static Argument MakeArgument(int type, int bg, Color bgColor, bool male, PortraitEmotion emotion, Color skin, Color hair, Color eye, Color lip, Texture2D mask = null)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
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
		result.Mask = (Texture)(object)mask;
		result.MaskScale = Vector2.one;
		result.MaskOffset = Vector2.zero;
		return result;
	}

	public static void Set(Argument arg, UITexture tex)
	{
		bool flag = true;
		Material mat = tex.material;
		if ((Object)(object)mat == (Object)null)
		{
			mat = MakePortraitMaterial(arg);
		}
		else
		{
			flag = RefreshPortraitMaterial(ref mat, arg);
		}
		if (flag)
		{
			tex.material = mat;
			if ((Object)(object)tex.panel != (Object)null)
			{
				tex.panel.RebuildAllDrawCalls();
			}
		}
	}

	public static Material MakePortraitMaterial(Argument arg)
	{
		Material mat = Object.Instantiate<Material>(KSingleton<PortraitBuilder>.Instance()._portraitMaterial);
		RefreshPortraitMaterial(ref mat, arg);
		return mat;
	}

	private static bool RefreshPortraitMaterial(ref Material mat, Argument arg)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0202: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0250: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		KSingleton<PortraitBuilder>.Instance().GetTexture(arg.Type, arg.Male, arg.Emotion, out var rampedBase, out var maskTex, out var gMaskRatio);
		Texture bgTexture = KSingleton<PortraitBuilder>.Instance().GetBgTexture(arg.Background);
		if (arg.Skin == Color.clear)
		{
			arg.Skin = KSingleton<PortraitBuilder>.Instance()._defaultSkinColor;
		}
		if (arg.Hair == Color.clear)
		{
			arg.Hair = KSingleton<PortraitBuilder>.Instance()._defaultHairColor;
		}
		if (arg.Eye == Color.clear)
		{
			arg.Eye = KSingleton<PortraitBuilder>.Instance().DefaultEyeColor;
		}
		if (arg.Lip == Color.clear)
		{
			arg.Lip = KSingleton<PortraitBuilder>.Instance().DefaultLipColor;
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
		if ((Object)(object)rampedBase != (Object)(object)texture)
		{
			mat.SetTexture(_mainTex, rampedBase);
			result = true;
		}
		if ((Object)(object)texture2 != (Object)(object)maskTex)
		{
			mat.SetTexture(_maskTex, maskTex);
			result = true;
		}
		if ((Object)(object)texture3 != (Object)(object)bgTexture)
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
		if ((Object)(object)texture4 != (Object)(object)arg.Mask)
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
			Debug.LogError((object)string.Concat(emotion, " portrait type is invalid"));
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
		return (index >= num) ? null : _bgTextures[index];
	}

	public int GetPortraitCount(bool male)
	{
		PortraitTexturesGroup[] array = ((!male) ? _femaleTextureGroup : _maleTextureGroup);
		return (array != null) ? array.Length : 0;
	}

	public int GetPortraitBgCount()
	{
		return (_bgTextures != null) ? _bgTextures.Length : 0;
	}

	public static void FillEmptyBackground(ulong entityId, ref int bg, ref Color bgColor)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		if (bg == -1 || bgColor == Color.clear)
		{
			Random random = new Random(entityId.GetHashCode());
			bg = random.Next(KSingleton<PortraitBuilder>.Instance().GetPortraitBgCount());
			byte[] array = new byte[3];
			random.NextBytes(array);
			bgColor = Color32.op_Implicit(new Color32(array[0], array[1], array[2], byte.MaxValue));
		}
	}
}
