using System;
using UnityEngine;

public class CustomColorCorrectionEffect : KSingleton<CustomColorCorrectionEffect>
{
	[Serializable]
	public class CorrectionSet
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public Texture RampTex;

		[SerializeField]
		public Texture RampCludyTex;

		[SerializeField]
		public Texture OverlayTex;

		[SerializeField]
		public Texture DirtTex;

		public void OverrideDefaultValue(CorrectionSet defaultSet)
		{
			if ((Object)(object)RampTex == (Object)null)
			{
				RampTex = defaultSet.RampTex;
			}
			if ((Object)(object)RampCludyTex == (Object)null)
			{
				RampCludyTex = defaultSet.RampCludyTex;
			}
			if ((Object)(object)OverlayTex == (Object)null)
			{
				OverlayTex = defaultSet.OverlayTex;
			}
			if ((Object)(object)DirtTex == (Object)null)
			{
				DirtTex = defaultSet.DirtTex;
			}
		}
	}

	[SerializeField]
	private Shader _shader;

	[SerializeField]
	private CorrectionSet _defaultSet;

	[SerializeField]
	private CorrectionSet[] _overrideSets;

	[SerializeField]
	private AnimationCurve _nightEffectCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	private Material _material;

	private int _rampTex;

	private int _rampCloudyTex;

	private int _overlayTex;

	private int _dirtTex;

	private int _timeFactor;

	private int _nightEffect;

	private int _cloudiness;

	public CorrectionSet CurrentSet { get; private set; }

	public float Time { get; set; }

	public bool PauseTime { get; set; }

	public float Cloudiness { get; set; }

	public float NightTimeOverride { get; set; }

	public float NightEffectMin { get; set; }

	private Material Material
	{
		get
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Expected O, but got Unknown
			if ((Object)(object)_material == (Object)null)
			{
				_material = new Material(_shader);
				((Object)_material).hideFlags = (HideFlags)61;
			}
			return _material;
		}
	}

	private void Start()
	{
		NightTimeOverride = -1f;
		CurrentSet = _defaultSet;
		_rampTex = Shader.PropertyToID("_RampTex");
		_rampCloudyTex = Shader.PropertyToID("_RampCloudyTex");
		_overlayTex = Shader.PropertyToID("_OverlayTex");
		_dirtTex = Shader.PropertyToID("_DirtTex");
		_timeFactor = Shader.PropertyToID("_TimeFactor");
		_nightEffect = Shader.PropertyToID("_NightEffect");
		_cloudiness = Shader.PropertyToID("_Cloudiness");
		if (KSingleton<TerrainA6>.Exist())
		{
			ApplyTileSet();
			KSingleton<GameManager>.Instance().PostReconnect += ApplyTileSet;
		}
	}

	private void ApplyTileSet()
	{
		for (int i = 0; i < _overrideSets.Length; i++)
		{
			if (_overrideSets[i].Name == TerrainMeta.TileSet)
			{
				CurrentSet = _overrideSets[i];
				break;
			}
		}
		CurrentSet.OverrideDefaultValue(_defaultSet);
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
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		if (Cloudiness > 0f && Cloudiness < 1f)
		{
			Material.EnableKeyword("BLENDING_ON");
			Material.SetTexture(_rampTex, CurrentSet.RampTex);
			Material.SetTexture(_rampCloudyTex, CurrentSet.RampCludyTex);
		}
		else
		{
			Material.DisableKeyword("BLENDING_ON");
			Material.SetTexture(_rampTex, (!(Cloudiness <= 0f)) ? CurrentSet.RampCludyTex : CurrentSet.RampTex);
		}
		Material.SetTexture(_overlayTex, CurrentSet.OverlayTex);
		Material.SetTexture(_dirtTex, CurrentSet.DirtTex);
		Material.SetFloat(_timeFactor, Time);
		float num = ((!(NightTimeOverride > 0f)) ? Mathf.Max(_nightEffectCurve.Evaluate(Time), NightEffectMin) : NightTimeOverride);
		num = Mathf.Clamp(num, 0f, 1f);
		Material.SetFloat(_nightEffect, num);
		Material.SetFloat(_cloudiness, Cloudiness);
		RenderTexture targetTexture = KSingleton<MainCamera>.Instance().TargetTexture;
		if (!((Object)(object)targetTexture == (Object)null))
		{
			RenderTexture temporary = RenderTexture.GetTemporary(targetTexture.width, targetTexture.height, 0, targetTexture.format);
			Graphics.Blit((Texture)(object)targetTexture, temporary, Material);
			targetTexture.DiscardContents(true, false);
			Graphics.Blit((Texture)(object)temporary, targetTexture);
			RenderTexture.ReleaseTemporary(temporary);
		}
	}
}
