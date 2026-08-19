using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Screen;
using Durango.UI.Prologue;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class PrologueTunnelController : Singleton<PrologueTunnelController>
{
	[CompilerGenerated]
	private sealed class _003CcoLightning_003Ed__60 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueTunnelController _003C_003E4__this;

		private int _003Ccount_003E5__2;

		private ScrollBackgroundController _003CscrollBackgroundController_003E5__3;

		private int _003Ci_003E5__4;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CcoLightning_003Ed__60(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003CscrollBackgroundController_003E5__3 = null;
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PrologueTunnelController prologueTunnelController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				SoundManager.PlayEvent(prologueTunnelController._lightningSound);
				_003Ccount_003E5__2 = UnityEngine.Random.Range(prologueTunnelController._lightningNumFlickerMin, prologueTunnelController._lightningNumFlickerMax);
				_003CscrollBackgroundController_003E5__3 = Singleton<ScrollBackgroundController>.Instance();
				_003Ci_003E5__4 = 0;
				break;
			case 1:
			{
				_003C_003E1__state = -1;
				prologueTunnelController.SetColorCorrectionFromCC_ID(prologueTunnelController._lightningNormalCC_ID);
				prologueTunnelController._colorCorrection.NightTimeOverride = prologueTunnelController._nightCurveAfterTunnel;
				float duration = UnityEngine.Random.Range(prologueTunnelController._lightningDarkDuraionMin, prologueTunnelController._lightningDarkDuraionMax);
				_003CscrollBackgroundController_003E5__3.SetTreeVisible(bNormal: true, bThunder: false);
				prologueTunnelController.SetLightningLitSphere(bLightning: false);
				_003C_003E2__current = prologueTunnelController.StartCoroutine(prologueTunnelController.coLightningMeshFading(prologueTunnelController._lightningIntensity, 0f, duration));
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				_003C_003E1__state = -1;
				_003Ci_003E5__4++;
				break;
			}
			if (_003Ci_003E5__4 < _003Ccount_003E5__2)
			{
				prologueTunnelController.SetColorCorrectionFromCC_ID(prologueTunnelController._lightningCC_ID);
				prologueTunnelController._colorCorrection.NightTimeOverride = prologueTunnelController._nightCurveAtLightning;
				float duration2 = UnityEngine.Random.Range(prologueTunnelController._lightningBrightDurationMin, prologueTunnelController._lightningBrightDurationMax);
				_003CscrollBackgroundController_003E5__3.SetTreeVisible(bNormal: false, bThunder: true);
				prologueTunnelController.SetLightningLitSphere(bLightning: true);
				_003C_003E2__current = prologueTunnelController.StartCoroutine(prologueTunnelController.coLightningMeshFading(0f, prologueTunnelController._lightningIntensity, duration2));
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CcoLightningMeshFading_003Ed__61 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public float duration;

		public float intensityFrom;

		public float intensityTo;

		public PrologueTunnelController _003C_003E4__this;

		private float _003CendTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CcoLightningMeshFading_003Ed__61(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PrologueTunnelController prologueTunnelController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CendTime_003E5__2 = Time.time + duration;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time < _003CendTime_003E5__2)
			{
				float num2 = (_003CendTime_003E5__2 - Time.time) / duration;
				float time = 1f - num2;
				float thunderMeshIntensity = Mathf.Lerp(intensityFrom, intensityTo, prologueTunnelController._lightningIntensityCurve.Evaluate(time));
				Singleton<PrologueTrainManager>.Instance().SetThunderMeshIntensity(thunderMeshIntensity);
				Singleton<TrainTrexController>.Instance().SetThunderMeshIntensity(thunderMeshIntensity);
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			Singleton<PrologueTrainManager>.Instance().SetThunderMeshIntensity(intensityTo);
			Singleton<TrainTrexController>.Instance().SetThunderMeshIntensity(intensityTo);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CcoStartLightningEffect_003Ed__59 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueTunnelController _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CcoStartLightningEffect_003Ed__59(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PrologueTunnelController prologueTunnelController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				prologueTunnelController._colorCorrection.NightTimeOverride = 0f;
				goto IL_0039;
			case 1:
				_003C_003E1__state = -1;
				_003C_003E2__current = new WaitForSeconds(UnityEngine.Random.Range(prologueTunnelController._lightningPeriodMin, prologueTunnelController._lightningPeriodMax));
				_003C_003E1__state = 2;
				return true;
			case 2:
				{
					_003C_003E1__state = -1;
					goto IL_0039;
				}
				IL_0039:
				_003C_003E2__current = prologueTunnelController.StartCoroutine(prologueTunnelController.coLightning());
				_003C_003E1__state = 1;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CcoStartTunnelEffect_003Ed__55 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public PrologueTunnelController _003C_003E4__this;

		private float _003CbeginTime_003E5__2;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CcoStartTunnelEffect_003Ed__55(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			PrologueTunnelController prologueTunnelController = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003CbeginTime_003E5__2 = Time.time;
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (Time.time - _003CbeginTime_003E5__2 < prologueTunnelController.TunnelCC_OverallDuration)
			{
				float time = prologueTunnelController._timeLine.Evaluate(Time.time - _003CbeginTime_003E5__2) / (float)prologueTunnelController._numColorCorrectionLevel;
				prologueTunnelController._colorCorrection.Time = time;
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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
		_scrollBackgroundController = UnityEngine.Object.FindObjectOfType<ScrollBackgroundController>();
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoStartTunnelEffect_003Ed__55(0)
		{
			_003C_003E4__this = this
		};
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoStartLightningEffect_003Ed__59(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator coLightning()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoLightning_003Ed__60(0)
		{
			_003C_003E4__this = this
		};
	}

	private IEnumerator coLightningMeshFading(float intensityFrom, float intensityTo, float duration)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CcoLightningMeshFading_003Ed__61(0)
		{
			_003C_003E4__this = this,
			intensityFrom = intensityFrom,
			intensityTo = intensityTo,
			duration = duration
		};
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
