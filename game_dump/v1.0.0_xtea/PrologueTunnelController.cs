using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrologueTunnelController : KSingleton<PrologueTunnelController>
{
	public AnimationCurve _timeLine = AnimationCurve.EaseInOut(0f, 0f, 10f, 10f);

	public int _numColorCorrectionLevel = 6;

	public int _normalCC_ID = 5;

	private CustomColorCorrectionEffect _colorCorrection;

	private GameObject _bulbSpark;

	private ScrollBackgroundController _scrollBackgroundController;

	public float TunnelCC_OverallDuration = 10f;

	public float _preDelay = 2.5f;

	public float _tunnelEnteringDuration = 1f;

	public float _tunnelEnteringFadeOut = 1f;

	public float _tunnelLeavingDelay = 5f;

	public float _tunnelLeavingDuration = 0.5f;

	public float _tunnelLeavingFadeOut = 3f;

	public float _maxAlphaBlack = 0.8f;

	public float _maxAlphaWhite = 0.6f;

	public AudioClip _tunnelSound;

	private AudioSource _tunnelBGMAudioSource;

	public float _tunnelBGMPlayDelay = 7f;

	public float _bulbSparkPlayDelay = 7f;

	public string _bulbSparkObjectName = "FX_BulbSparkArea_01";

	public float _ScrollBG_TunnelDelay = 3f;

	public float _ScrollBG_TunnelFadeTime = 1f;

	public float _ScrollBG_TunnelDuration = 5f;

	public float _beginRainingDelay = 10f;

	public float _preLightningDelay = 13f;

	public float _lightningPeriodMin = 5f;

	public float _lightningPeriodMax = 7f;

	public int _lightningNumFlickerMin = 1;

	public int _lightningNumFlickerMax = 3;

	public float _lightningBrightDurationMin = 0.1f;

	public float _lightningBrightDurationMax = 0.3f;

	public float _lightningDarkDuraionMin = 0.01f;

	public float _lightningDarkDuraionMax = 0.05f;

	public int _lightningCC_ID;

	public int _lightningNormalCC_ID = 1;

	public List<AudioClip> _lightningSounds = new List<AudioClip>();

	public List<SkinnedMeshRenderer> _lightningLitSphereMeshes = new List<SkinnedMeshRenderer>();

	public Texture _lightningLitSphereTexture;

	public Texture _normalLitSphereTexture;

	public float _nightCurveNormal;

	public float _nightCurveAfterTunnel = 0.4f;

	public float _nightCurveAtLightning = 1f;

	public float _lightningIntensity = 2f;

	public AnimationCurve _lightningIntensityCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

	protected override void OnAwake()
	{
		_colorCorrection = KSingleton<CustomColorCorrectionEffect>.Instance();
		SetColorCorrectionFromCC_ID(_normalCC_ID);
		_scrollBackgroundController = Object.FindObjectOfType<ScrollBackgroundController>();
		_tunnelBGMAudioSource = GameObject.Find("BGMSound").GetComponent<AudioSource>();
		_bulbSpark = KUtility.FindObjectByName(((Component)KSingleton<PrologueTrainManager>.Instance()).gameObject, _bulbSparkObjectName, includeInactive: true);
		_colorCorrection.NightTimeOverride = _nightCurveNormal;
	}

	public void TestBeginTunnelEffect()
	{
		TunnelEffect(skipTunnelEffect: false);
	}

	public void TunnelEffect(bool skipTunnelEffect)
	{
		_colorCorrection.NightTimeOverride = 0f;
		((Component)KSingleton<PrologueManager>.Instance()).GetComponent<AudioSource>().PlayOneShot(_tunnelSound);
		((MonoBehaviour)this).Invoke("PlayBGM", _tunnelBGMPlayDelay);
		if (skipTunnelEffect)
		{
			KSingleton<PrologueManager>.Instance().BeginRaining();
			BeginLightning();
		}
		else
		{
			_scrollBackgroundController.PlayTunnelEffect(_ScrollBG_TunnelDelay, _ScrollBG_TunnelFadeTime, _ScrollBG_TunnelDuration);
			KSingleton<PrologueOverlayGroup>.Instance().PlayTunnelEffect();
			BeginTunnelEffect();
		}
		if (!skipTunnelEffect)
		{
			((MonoBehaviour)this).Invoke("PlayFrightenMotion", _tunnelBGMPlayDelay);
		}
		((MonoBehaviour)this).Invoke("PlayBulbSpark", _bulbSparkPlayDelay);
	}

	private void PlayBGM()
	{
		_tunnelBGMAudioSource.Play();
	}

	private void PlayFrightenMotion()
	{
		KSingleton<PrologueManager>.Instance().PlayFrightenMotion();
	}

	private void PlayBulbSpark()
	{
		if (Object.op_Implicit((Object)(object)_bulbSpark))
		{
			_bulbSpark.SetActive(true);
		}
		_colorCorrection.NightTimeOverride = _nightCurveAfterTunnel;
	}

	public void BeginTunnelEffect()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(coStartTunnelEffect());
		((MonoBehaviour)KSingleton<PrologueManager>.Instance()).Invoke("BeginRaining", _beginRainingDelay);
		((MonoBehaviour)this).Invoke("BeginLightning", _preLightningDelay);
	}

	private IEnumerator coStartTunnelEffect()
	{
		float beginTime = Time.time;
		while (Time.time - beginTime < TunnelCC_OverallDuration)
		{
			float timeRamp = _timeLine.Evaluate(Time.time - beginTime) / (float)_numColorCorrectionLevel;
			_colorCorrection.Time = timeRamp;
			yield return null;
		}
	}

	public void BeginLightning()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(coStartLightningEffect());
	}

	private void SetColorCorrectionFromCC_ID(int ccid)
	{
		float num = 0.5f / (float)_numColorCorrectionLevel;
		float num2 = (float)ccid / (float)_numColorCorrectionLevel + num;
		_colorCorrection.Time = Mathf.Clamp01(num2);
	}

	private void SetLightningLitSphere(bool bLightning)
	{
		int count = _lightningLitSphereMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			if (Object.op_Implicit((Object)(object)_lightningLitSphereMeshes[i]))
			{
				((Renderer)_lightningLitSphereMeshes[i]).material.SetTexture("_LitSphereTex", (!bLightning) ? _normalLitSphereTexture : _lightningLitSphereTexture);
			}
		}
	}

	private IEnumerator coStartLightningEffect()
	{
		_colorCorrection.NightTimeOverride = 0f;
		while (true)
		{
			yield return ((MonoBehaviour)this).StartCoroutine(coLightning());
			yield return (object)new WaitForSeconds(Random.Range(_lightningPeriodMin, _lightningPeriodMax));
		}
	}

	private IEnumerator coLightning()
	{
		if (_lightningSounds.Count > 0)
		{
			int soundID = Random.Range(0, _lightningSounds.Count);
			if (Object.op_Implicit((Object)(object)_lightningSounds[soundID]))
			{
				((Component)KSingleton<PrologueManager>.Instance()).GetComponent<AudioSource>().PlayOneShot(_lightningSounds[soundID]);
			}
		}
		int count = Random.Range(_lightningNumFlickerMin, _lightningNumFlickerMax);
		ScrollBackgroundController scrollBackgroundController = KSingleton<ScrollBackgroundController>.Instance();
		for (int i = 0; i < count; i++)
		{
			SetColorCorrectionFromCC_ID(_lightningCC_ID);
			_colorCorrection.NightTimeOverride = _nightCurveAtLightning;
			float lightningBightDuration = Random.Range(_lightningBrightDurationMin, _lightningBrightDurationMax);
			scrollBackgroundController.SetTreeVisible(bNormal: false, bThunder: true);
			SetLightningLitSphere(bLightning: true);
			yield return ((MonoBehaviour)this).StartCoroutine(coLightningMeshFading(0f, _lightningIntensity, lightningBightDuration));
			SetColorCorrectionFromCC_ID(_lightningNormalCC_ID);
			_colorCorrection.NightTimeOverride = _nightCurveAfterTunnel;
			float lightningDarkDuraion = Random.Range(_lightningDarkDuraionMin, _lightningDarkDuraionMax);
			scrollBackgroundController.SetTreeVisible(bNormal: true, bThunder: false);
			SetLightningLitSphere(bLightning: false);
			yield return ((MonoBehaviour)this).StartCoroutine(coLightningMeshFading(_lightningIntensity, 0f, lightningDarkDuraion));
		}
	}

	private IEnumerator coLightningMeshFading(float intensityFrom, float intensityTo, float duration)
	{
		float endTime = Time.time + duration;
		while (Time.time < endTime)
		{
			float t = (endTime - Time.time) / duration;
			float ratio = 1f - t;
			float intensity = Mathf.Lerp(intensityFrom, intensityTo, _lightningIntensityCurve.Evaluate(ratio));
			KSingleton<PrologueTrainManager>.Instance().SetThunderMeshIntensity(intensity);
			KSingleton<TrainTrexController>.Instance().SetThunderMeshIntensity(intensity);
			yield return null;
		}
		KSingleton<PrologueTrainManager>.Instance().SetThunderMeshIntensity(intensityTo);
		KSingleton<TrainTrexController>.Instance().SetThunderMeshIntensity(intensityTo);
	}

	public void StopLightning()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(coLightning());
	}

	public void ForceLightningOnce()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(coLightning());
	}

	public void EndLightning()
	{
		((MonoBehaviour)this).StopAllCoroutines();
		SetColorCorrectionFromCC_ID(_normalCC_ID);
		_colorCorrection.NightTimeOverride = _nightCurveNormal;
	}
}
