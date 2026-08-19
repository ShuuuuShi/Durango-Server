using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Effect;

public class FogUpdater : MonoBehaviour
{
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
		_transitionRatio = 0f;
		Color initial = _material.GetColor(_tintColorId);
		while (_transitionRatio < 1f)
		{
			_transitionRatio = Mathf.MoveTowards(_transitionRatio, 1f, Time.deltaTime / _transitionSecTime);
			Color cur = Color.Lerp(initial, targetColor, _transitionRatio);
			_material.SetColor(_tintColorId, cur);
			yield return null;
		}
	}
}
