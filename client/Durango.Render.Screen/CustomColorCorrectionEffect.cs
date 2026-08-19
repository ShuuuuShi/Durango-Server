using System;
using System.Collections.Generic;
using Durango.Render.Camera;
using Durango.Render.Particle;
using Durango.Terrain;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Screen;

public class CustomColorCorrectionEffect : Singleton<CustomColorCorrectionEffect>
{
	[Serializable]
	public class CorrectionSet
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public Texture2D RampTex;

		[SerializeField]
		public Texture2D OverlayTex;

		[SerializeField]
		public Texture2D DirtTex;

		[SerializeField]
		public Texture2D WorldLightTex;

		[SerializeField]
		public AnimationCurve NightEffectCurve = new AnimationCurve();

		[SerializeField]
		public float Contrast = 1f;

		[SerializeField]
		public Vector3 Hilight;

		[SerializeField]
		public Vector3 MidTone;

		[SerializeField]
		public Vector3 Shadow;

		[SerializeField]
		public float ContrastAtNight = 1f;

		[SerializeField]
		public Vector3 HilightAtNight;

		[SerializeField]
		public Vector3 MidToneAtNight;

		[SerializeField]
		public Vector3 ShadowAtNight;

		[NonSerialized]
		public float NightScale;

		[NonSerialized]
		public float NightMin;

		public void Override(CorrectionSet defaultSet)
		{
			if (this != defaultSet)
			{
				if (RampTex == null)
				{
					RampTex = defaultSet.RampTex;
				}
				if (OverlayTex == null)
				{
					OverlayTex = defaultSet.OverlayTex;
				}
				if (DirtTex == null)
				{
					DirtTex = defaultSet.DirtTex;
				}
				if (NightEffectCurve.keys.Length <= 1)
				{
					NightEffectCurve = new AnimationCurve(defaultSet.NightEffectCurve.keys);
				}
			}
			float num = NightEffectCurve.Evaluate(0f);
			NightMin = NightEffectCurve.Evaluate(0.5f);
			NightScale = num - NightMin;
		}
	}

	[Serializable]
	private class CorrectionFilter
	{
		[SerializeField]
		public Vector3 HilightAdd;

		[SerializeField]
		public Vector3 MidToneAdd;

		[SerializeField]
		public Vector3 ShadowAdd;
	}

	private static readonly int WorldLightColorId = Shader.PropertyToID("_WorldLightColor");

	private static readonly int TimeFactorId = Shader.PropertyToID("_TimeFactor");

	private static readonly int RampTexId = Shader.PropertyToID("_RampTex");

	private static readonly int OverlayTexId = Shader.PropertyToID("_OverlayTex");

	private static readonly int DirtTexId = Shader.PropertyToID("_DirtTex");

	private static readonly int LookupTexId = Shader.PropertyToID("_LookupTex");

	private static readonly int ContrastId = Shader.PropertyToID("_Contrast");

	private static readonly int NightEffectId = Shader.PropertyToID("_NightEffect");

	private static readonly int DirtFactorId = Shader.PropertyToID("_DirtFactor");

	private static readonly int CloudinessId = Shader.PropertyToID("_Cloudiness");

	private static readonly int AtmosphereId = Shader.PropertyToID("_Atmosphere");

	private static readonly int IndoorEffectId = Shader.PropertyToID("_IndoorEffect");

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private CorrectionSet _defaultSet;

	[SerializeField]
	private List<CorrectionSet> _overrideSets = new List<CorrectionSet>();

	[SerializeField]
	private CorrectionFilter _atmosphereFilter;

	[SerializeField]
	private float _atmosphereFadeTime;

	[SerializeField]
	private ParticleType _cloudFogParticle;

	[SerializeField]
	private Material _indoorEffectMaterial;

	[SerializeField]
	private float _indoorEffectFadeSpeed = 2f;

	private Material _material;

	private Texture2D _lookupTex;

	private Color32[] _lightColors;

	private float _nightFactorNormalized;

	private SimpleTimer _textureGenerationTimer;

	private float _curAtmosphereRatio;

	private readonly CorrectionFilter _currentFilter = new CorrectionFilter();

	private bool _atmosphericEffectOn;

	private int _cloudFogId;

	private float _targetIndoorEffectRatio;

	private float _curIndoorEffectRatio;

	public CorrectionSet CurrentSet { get; private set; }

	public float Time { get; set; }

	public bool PauseTime { get; set; }

	public float Cloudiness { get; set; }

	public float NightTimeOverride { get; set; }

	public float NightEffectMin { get; set; }

	public bool UseOverlayAndDirtTexture { get; set; }

	public bool EnableOverlayAndDirtTexture { private get; set; }

	public Color LightColor { get; private set; }

	public bool DisableIndoorEffect { get; set; }

	private Material Material
	{
		get
		{
			if (_material == null)
			{
				_material = new Material(_shader);
				_material.hideFlags = HideFlags.HideAndDontSave;
			}
			return _material;
		}
	}

	private void Start()
	{
		NightTimeOverride = -1f;
		UseOverlayAndDirtTexture = true;
		EnableOverlayAndDirtTexture = true;
		ParticleManager.Cache(_cloudFogParticle);
		_textureGenerationTimer = new SimpleTimer(1f);
		ApplyTileSet();
		Singleton<GameManager>.Instance().PostReconnect += ApplyTileSet;
		PlayerBehavior.LocalPlayer.IsIndoorChanged += RefreshIndoorEffect;
		UIManager.ClosableUIChanged += RefreshIndoorEffect;
	}

	private void ApplyTileSet()
	{
		for (int i = 0; i < _overrideSets.Count; i++)
		{
			if (_overrideSets[i].Name == TerrainMeta.ColorSet)
			{
				ChangeCurrentSet(i + 1);
				return;
			}
		}
		for (int j = 0; j < _overrideSets.Count; j++)
		{
			if (_overrideSets[j].Name == TerrainMeta.TileSet)
			{
				ChangeCurrentSet(j + 1);
				return;
			}
		}
		ChangeCurrentSet(0);
	}

	public void ChangeCurrentSet(int index)
	{
		CurrentSet = ((index != 0) ? _overrideSets[index - 1] : _defaultSet);
		CurrentSet.Override(_defaultSet);
		OnCorrectionSetChanged();
	}

	public void SetAtmosphereEffect(bool on)
	{
		_atmosphericEffectOn = on;
		if (on)
		{
			if (_cloudFogId == 0)
			{
				_cloudFogId = ParticleManager.EmitFollow(_cloudFogParticle, Vector3.zero, Quaternion.identity, Singleton<PlayerController>.Instance().transform, useLocalPosition: true, comeForwardToCamera: false, groundDecal: true);
			}
		}
		else
		{
			ParticleManager.Stop(_cloudFogId, immediately: false);
			_cloudFogId = 0;
		}
	}

	private void OnCorrectionSetChanged()
	{
		GenerateLookupTexture(force: true);
		_lightColors = null;
	}

	private void Update()
	{
		float normalizedTime = TimeGauge.GetNormalizedTime();
		if (!PauseTime)
		{
			Time = normalizedTime;
		}
	}

	private void OnPostRender()
	{
		Material.SetTexture(RampTexId, CurrentSet.RampTex);
		if (UIManager.IsPortraitScreen)
		{
			Shader.EnableKeyword("PORTRAIT_ON");
		}
		else
		{
			Shader.DisableKeyword("PORTRAIT_ON");
		}
		if (UseOverlayAndDirtTexture && EnableOverlayAndDirtTexture)
		{
			Material.SetTexture(OverlayTexId, CurrentSet.OverlayTex);
			Material.SetTexture(DirtTexId, CurrentSet.DirtTex);
		}
		else
		{
			Material.SetTexture(OverlayTexId, null);
			Material.SetTexture(DirtTexId, null);
		}
		Shader.SetGlobalFloat(TimeFactorId, Time);
		float value = ((!(NightTimeOverride > 0f)) ? Mathf.Max(CurrentSet.NightEffectCurve.Evaluate(Time), NightEffectMin) : NightTimeOverride);
		value = Mathf.Clamp(value, 0f, 1f);
		_nightFactorNormalized = ((!(CurrentSet.NightScale <= 0f)) ? Mathf.Clamp01((value - CurrentSet.NightMin) / CurrentSet.NightScale) : 0f);
		Material.SetFloat(ContrastId, Mathf.Lerp(CurrentSet.Contrast, CurrentSet.ContrastAtNight, _nightFactorNormalized));
		ProcessAtmosphere();
		GenerateLookupTexture();
		SetGlobalLightColor();
		Material.SetTexture(LookupTexId, _lookupTex);
		Material.SetFloat(NightEffectId, value);
		Material.SetFloat(DirtFactorId, value * 0.5f + 0.5f);
		Material.SetFloat(CloudinessId, Cloudiness);
		RenderTexture targetTexture = Singleton<MainCamera>.Instance().TargetTexture;
		if (!(targetTexture == null))
		{
			RenderTexture temporary = RenderTexture.GetTemporary(targetTexture.width, targetTexture.height, 0, targetTexture.format);
			Graphics.Blit(targetTexture, temporary, Material);
			targetTexture.DiscardContents(discardColor: true, discardDepth: false);
			UpdateIndoorEffectRatio();
			if (0f < _curIndoorEffectRatio && _curIndoorEffectRatio <= 1f && _indoorEffectMaterial != null && !DisableIndoorEffect)
			{
				Graphics.Blit(temporary, targetTexture, _indoorEffectMaterial);
			}
			else
			{
				Graphics.Blit(temporary, targetTexture);
			}
			RenderTexture.ReleaseTemporary(temporary);
		}
	}

	private void ProcessAtmosphere()
	{
		float a = ((!_atmosphericEffectOn) ? 0f : 1f);
		if (Mathf.Approximately(a, _curAtmosphereRatio))
		{
			return;
		}
		float num = UnityEngine.Time.deltaTime / _atmosphereFadeTime;
		num = ((!_atmosphericEffectOn) ? (0f - num) : num);
		float num2 = _curAtmosphereRatio + num;
		_curAtmosphereRatio = Mathf.Clamp01(num2);
		bool flag = Material.IsKeywordEnabled("ATMOSPHERIC_ON");
		if (num2 > 0f)
		{
			_currentFilter.HilightAdd = _atmosphereFilter.HilightAdd * _curAtmosphereRatio;
			_currentFilter.MidToneAdd = _atmosphereFilter.MidToneAdd * _curAtmosphereRatio;
			_currentFilter.ShadowAdd = _atmosphereFilter.ShadowAdd * _curAtmosphereRatio;
			if (!flag)
			{
				Material.EnableKeyword("ATMOSPHERIC_ON");
			}
		}
		else if (flag)
		{
			Material.DisableKeyword("ATMOSPHERIC_ON");
		}
		Material.SetFloat(AtmosphereId, _curAtmosphereRatio);
	}

	public void GenerateLookupTexture(bool force = false)
	{
		if (!_textureGenerationTimer.CheckTime() && !force)
		{
			return;
		}
		if (_lookupTex == null)
		{
			_lookupTex = new Texture2D(256, 1, TextureFormat.RGB24, mipmap: false);
		}
		Color32[] pixels = _lookupTex.GetPixels32();
		for (int i = 0; i < 256; i++)
		{
			float original = (float)i / 255f;
			for (int j = 0; j < 3; j++)
			{
				float num = Mathf.Lerp(CurrentSet.Hilight[j], CurrentSet.HilightAtNight[j], _nightFactorNormalized);
				float num2 = Mathf.Lerp(CurrentSet.MidTone[j], CurrentSet.MidToneAtNight[j], _nightFactorNormalized);
				float num3 = Mathf.Lerp(CurrentSet.Shadow[j], CurrentSet.ShadowAtNight[j], _nightFactorNormalized);
				if (_curAtmosphereRatio > 0f)
				{
					num += _currentFilter.HilightAdd[j];
					num2 += _currentFilter.MidToneAdd[j];
					num3 += _currentFilter.ShadowAdd[j];
				}
				float colorCorretionResult = GetColorCorretionResult(original, num, num2, num3);
				byte b = (byte)Mathf.Clamp(colorCorretionResult * 255f, 0f, 255f);
				switch (j)
				{
				case 0:
					pixels[i].r = b;
					break;
				case 1:
					pixels[i].g = b;
					break;
				case 2:
					pixels[i].b = b;
					break;
				}
			}
		}
		_lookupTex.SetPixels32(pixels);
		_lookupTex.Apply(updateMipmaps: false);
		_lookupTex.wrapMode = TextureWrapMode.Clamp;
		_lookupTex.filterMode = FilterMode.Point;
	}

	private void SetGlobalLightColor()
	{
		if (_lightColors == null)
		{
			Texture2D texture = ((!(CurrentSet.WorldLightTex != null)) ? CurrentSet.RampTex : CurrentSet.WorldLightTex);
			Texture2D texture2D = ToReadableTexture(texture);
			_lightColors = new Color32[texture2D.height];
			for (int i = 0; i < texture2D.height; i++)
			{
				ref Color32 reference = ref _lightColors[i];
				reference = texture2D.GetPixel(0, i);
			}
		}
		LightColor = _lightColors[(int)(Time * (float)_lightColors.Length)];
		Shader.SetGlobalColor(WorldLightColorId, LightColor);
	}

	private static Texture2D ToReadableTexture(Texture2D texture)
	{
		try
		{
			texture.GetPixel(0, 0);
			return texture;
		}
		catch
		{
			RenderTexture temporary = RenderTexture.GetTemporary(texture.width, texture.height, 0, RenderTextureFormat.Default, RenderTextureReadWrite.Linear);
			Graphics.Blit(texture, temporary);
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = temporary;
			Texture2D texture2D = new Texture2D(texture.width, texture.height);
			texture2D.ReadPixels(new Rect(0f, 0f, temporary.width, temporary.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = active;
			RenderTexture.ReleaseTemporary(temporary);
			return texture2D;
		}
	}

	public static float GetColorCorretionResult(float original, float hilight, float midTone, float shadow)
	{
		Vector3 zero = Vector3.zero;
		if (hilight > 0f)
		{
			zero.x = Math.Abs(original) * 0.62f + 0.0157f + Math.Max(0f, original) * 0f;
		}
		else
		{
			zero.x = Math.Abs(original - 0.4313f) * -0.282f + 0.1255f + Math.Max(0f, original - 0.4313f) * 0.062f;
		}
		zero.x *= hilight;
		if (midTone > 0f)
		{
			zero.y = Math.Abs(original - 0.255f) * -0.938f + 0.251f + Math.Max(0f, original - 0.255f) * 0.463f;
		}
		else
		{
			zero.y = Math.Abs(original - 0.49f) * -0.35f + 0.251f + Math.Max(0f, original - 0.49f) * 0.0154f;
		}
		zero.y *= midTone;
		if (shadow > 0f)
		{
			zero.z = Math.Abs(original - 0.3137f) * -0.18f + 0.1255f + Math.Max(0f, original - 0.3137f) * 0.08f;
		}
		else
		{
			zero.z = Math.Abs(original - 1f) * 0.9545f + Math.Max(0f, original - 1f) * 0f;
		}
		zero.z *= shadow;
		return Mathf.Clamp01(original + zero.x + zero.y + zero.z);
	}

	private void RefreshIndoorEffect()
	{
		bool flag = PlayerBehavior.LocalPlayer.IsIndoor && (UIBase.CurrentUI == null || (UIBase.CurrentUI.Anchor != UIBase.AnchorType.FullscreenMobileOnly && UIBase.CurrentUI.Anchor != UIBase.AnchorType.CloneFullscreen));
		if (GameManager.Region.IsPvpIsland())
		{
			flag = false;
		}
		_targetIndoorEffectRatio = ((!flag) ? 0f : 1f);
	}

	private void UpdateIndoorEffectRatio()
	{
		if (!Mathf.Approximately(_curIndoorEffectRatio, _targetIndoorEffectRatio))
		{
			_curIndoorEffectRatio = Mathf.MoveTowards(_curIndoorEffectRatio, _targetIndoorEffectRatio, _indoorEffectFadeSpeed * UnityEngine.Time.deltaTime);
			_curIndoorEffectRatio = Mathf.Clamp01(_curIndoorEffectRatio);
			if (_indoorEffectMaterial != null)
			{
				_indoorEffectMaterial.SetFloat(IndoorEffectId, _curIndoorEffectRatio);
			}
		}
	}
}
