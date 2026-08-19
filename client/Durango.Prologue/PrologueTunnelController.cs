using System.Collections;
using System.Collections.Generic;
using Durango.Render.Screen;
using Durango.UI.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class PrologueTunnelController : Singleton<PrologueTunnelController>
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

	public SoundEventType _tunnelSound;

	public SoundEventType _tunnelBgm;

	public SoundEventType _tunnelBgmTransition;

	public float _tunnelBGMPlayDelay = 7f;

	public float _bulbSparkPlayDelay = 7f;

	public string _bulbSparkObjectName = "FX_BulbSparkArea_01";

	public float _ScrollBG_TunnelDelay = 3f;

	public float _ScrollBG_TunnelFadeTime = 1f;

	public float _ScrollBG_TunnelDuration = 5f;

	private uint _soundInstanceId;

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

	public SoundEventType _lightningSound;

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
		_colorCorrection = Singleton<CustomColorCorrectionEffect>.Instance();
		SetColorCorrectionFromCC_ID(_normalCC_ID);
		_scrollBackgroundController = Object.FindObjectOfType<ScrollBackgroundController>();
		_bulbSpark = KUtility.FindObjectByName(Singleton<PrologueTrainManager>.Instance().gameObject, _bulbSparkObjectName, includeInactive: true);
		_colorCorrection.NightTimeOverride = _nightCurveNormal;
	}

	public void TestBeginTunnelEffect()
	{
		TunnelEffect(skipTunnelEffect: false);
	}

	public void TunnelEffect(bool skipTunnelEffect)
	{
		_colorCorrection.NightTimeOverride = 0f;
		SoundManager.PlayEvent(_tunnelSound);
		Invoke("PlayBGM", _tunnelBGMPlayDelay);
		if (skipTunnelEffect)
		{
			Singleton<PrologueManager>.Instance().BeginRaining();
			BeginLightning();
		}
		else
		{
			_scrollBackgroundController.PlayTunnelEffect(_ScrollBG_TunnelDelay, _ScrollBG_TunnelFadeTime, _ScrollBG_TunnelDuration);
			Singleton<PrologueOverlayGroup>.Instance().PlayTunnelEffect();
			BeginTunnelEffect();
		}
		if (!skipTunnelEffect)
		{
			Invoke("PlayFrightenMotion", _tunnelBGMPlayDelay);
		}
		Invoke("PlayBulbSpark", _bulbSparkPlayDelay);
	}

	private void PlayBGM()
	{
		_soundInstanceId = SoundManager.PlayEvent(_tunnelBgm, SoundPosition.Empty, exclusive: true);
	}

	public void TransitionBgm()
	{
		SoundManager.PlayEvent(_soundInstanceId, _tunnelBgmTransition);
	}

	public void StopBgm(float fadeOutDuration)
	{
		SoundManager.StopEvent(_soundInstanceId, fadeOutDuration);
		_soundInstanceId = 0u;
	}

	private void PlayFrightenMotion()
	{
		Singleton<PrologueManager>.Instance().PlayFrightenMotion();
	}

	private void PlayBulbSpark()
	{
		if ((bool)_bulbSpark)
		{
			_bulbSpark.SetActive(value: true);
		}
		_colorCorrection.NightTimeOverride = _nightCurveAfterTunnel;
	}

	public void BeginTunnelEffect()
	{
		StopAllCoroutines();
		StartCoroutine(coStartTunnelEffect());
		Singleton<PrologueManager>.Instance().Invoke("BeginRaining", _beginRainingDelay);
		Invoke("BeginLightning", _preLightningDelay);
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
		StopAllCoroutines();
		StartCoroutine(coStartLightningEffect());
	}

	private void SetColorCorrectionFromCC_ID(int ccid)
	{
		float num = 0.5f / (float)_numColorCorrectionLevel;
		float value = (float)ccid / (float)_numColorCorrectionLevel + num;
		_colorCorrection.Time = Mathf.Clamp01(value);
	}

	private void SetLightningLitSphere(bool bLightning)
	{
		int count = _lightningLitSphereMeshes.Count;
		for (int i = 0; i < count; i++)
		{
			if ((bool)_lightningLitSphereMeshes[i])
			{
				_lightningLitSphereMeshes[i].material.SetTexture("_LitSphereTex", (!bLightning) ? _normalLitSphereTexture : _lightningLitSphereTexture);
			}
		}
	}

	private IEnumerator coStartLightningEffect()
	{
		_colorCorrection.NightTimeOverride = 0f;
		while (true)
		{
			yield return StartCoroutine(coLightning());
			yield return new WaitForSeconds(Random.Range(_lightningPeriodMin, _lightningPeriodMax));
		}
	}

	private IEnumerator coLightning()
	{
		SoundManager.PlayEvent(_lightningSound);
		int count = Random.Range(_lightningNumFlickerMin, _lightningNumFlickerMax);
		ScrollBackgroundController scrollBackgroundController = Singleton<ScrollBackgroundController>.Instance();
		for (int i = 0; i < count; i++)
		{
			SetColorCorrectionFromCC_ID(_lightningCC_ID);
			_colorCorrection.NightTimeOverride = _nightCurveAtLightning;
			float lightningBightDuration = Random.Range(_lightningBrightDurationMin, _lightningBrightDurationMax);
			scrollBackgroundController.SetTreeVisible(bNormal: false, bThunder: true);
			SetLightningLitSphere(bLightning: true);
			yield return StartCoroutine(coLightningMeshFading(0f, _lightningIntensity, lightningBightDuration));
			SetColorCorrectionFromCC_ID(_lightningNormalCC_ID);
			_colorCorrection.NightTimeOverride = _nightCurveAfterTunnel;
			float lightningDarkDuraion = Random.Range(_lightningDarkDuraionMin, _lightningDarkDuraionMax);
			scrollBackgroundController.SetTreeVisible(bNormal: true, bThunder: false);
			SetLightningLitSphere(bLightning: false);
			yield return StartCoroutine(coLightningMeshFading(_lightningIntensity, 0f, lightningDarkDuraion));
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
			Singleton<PrologueTrainManager>.Instance().SetThunderMeshIntensity(intensity);
			Singleton<TrainTrexController>.Instance().SetThunderMeshIntensity(intensity);
			yield return null;
		}
		Singleton<PrologueTrainManager>.Instance().SetThunderMeshIntensity(intensityTo);
		Singleton<TrainTrexController>.Instance().SetThunderMeshIntensity(intensityTo);
	}

	public void StopLightning()
	{
		StopAllCoroutines();
		StartCoroutine(coLightning());
	}

	public void ForceLightningOnce()
	{
		StopAllCoroutines();
		StartCoroutine(coLightning());
	}

	public void EndLightning()
	{
		StopAllCoroutines();
		SetColorCorrectionFromCC_ID(_normalCC_ID);
		_colorCorrection.NightTimeOverride = _nightCurveNormal;
	}
}
