using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Effect;

public class FogUpdater : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CCoTwinTintColor_003Ed__15 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public FogUpdater _003C_003E4__this;

		public Color targetColor;

		private Color _003Cinitial_003E5__2;

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
		public _003CCoTwinTintColor_003Ed__15(int _003C_003E1__state)
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
			FogUpdater fogUpdater = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				fogUpdater._transitionRatio = 0f;
				_003Cinitial_003E5__2 = fogUpdater._material.GetColor(fogUpdater._tintColorId);
				break;
			case 1:
				_003C_003E1__state = -1;
				break;
			}
			if (fogUpdater._transitionRatio < 1f)
			{
				fogUpdater._transitionRatio = Mathf.MoveTowards(fogUpdater._transitionRatio, 1f, Time.deltaTime / fogUpdater._transitionSecTime);
				Color value = Color.Lerp(_003Cinitial_003E5__2, targetColor, fogUpdater._transitionRatio);
				fogUpdater._material.SetColor(fogUpdater._tintColorId, value);
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

	[SerializeField]
	private int _dayRateOverTime = 20;

	[SerializeField]
	private int _nightRateOverTime = 40;

	[SerializeField]
	private Vector3[] _dayForcedOverLifeTime;

	[SerializeField]
	private Vector3[] _nightForcedOverLifeTime;

	[SerializeField]
	private Color _dayTintColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	[SerializeField]
	private Color _nightTintColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	[SerializeField]
	private float _transitionSecTime = 3f;

	private float _transitionRatio;

	private ParticleSystem _particleSystem;

	private Material _material;

	private int _tintColorId;

	private ICoroutineBinder _binder;

	private void Awake()
	{
		_tintColorId = Shader.PropertyToID("_TintColor");
		_particleSystem = GetComponent<ParticleSystem>();
		_material = GetComponent<Renderer>().material;
		TimeGauge.IsSunUpChanged += TimeGuage_IsSunUpChanged;
		TimeGuage_IsSunUpChanged();
	}

	private void OnDestroy()
	{
		TimeGauge.IsSunUpChanged -= TimeGuage_IsSunUpChanged;
	}

	private void TimeGuage_IsSunUpChanged()
	{
		ParticleSystem.EmissionModule emission = _particleSystem.emission;
		emission.rateOverTime = ((!TimeGauge.IsSunUp) ? _nightRateOverTime : _dayRateOverTime);
		ParticleSystem.MinMaxCurve x = ((!TimeGauge.IsSunUp) ? new ParticleSystem.MinMaxCurve(_nightForcedOverLifeTime[0].x, _nightForcedOverLifeTime[1].x) : new ParticleSystem.MinMaxCurve(_dayForcedOverLifeTime[0].x, _dayForcedOverLifeTime[1].x));
		ParticleSystem.MinMaxCurve y = ((!TimeGauge.IsSunUp) ? new ParticleSystem.MinMaxCurve(_nightForcedOverLifeTime[0].y, _nightForcedOverLifeTime[1].y) : new ParticleSystem.MinMaxCurve(_dayForcedOverLifeTime[0].y, _dayForcedOverLifeTime[1].y));
		ParticleSystem.MinMaxCurve z = ((!TimeGauge.IsSunUp) ? new ParticleSystem.MinMaxCurve(_nightForcedOverLifeTime[0].z, _nightForcedOverLifeTime[1].z) : new ParticleSystem.MinMaxCurve(_dayForcedOverLifeTime[0].z, _dayForcedOverLifeTime[1].z));
		ParticleSystem.ForceOverLifetimeModule forceOverLifetime = _particleSystem.forceOverLifetime;
		forceOverLifetime.x = x;
		forceOverLifetime.y = y;
		forceOverLifetime.z = z;
		this.StartCoroutine(ref _binder, CoTwinTintColor((!TimeGauge.IsSunUp) ? _nightTintColor : _dayTintColor));
	}

	private IEnumerator CoTwinTintColor(Color targetColor)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoTwinTintColor_003Ed__15(0)
		{
			_003C_003E4__this = this,
			targetColor = targetColor
		};
	}
}
