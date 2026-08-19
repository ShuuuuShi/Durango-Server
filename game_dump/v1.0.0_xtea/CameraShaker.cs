using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShaker : KSingleton<CameraShaker>
{
	private const float MinShakeDuration = 0.3f;

	private const float MinShakeInterval = 0.02f;

	[SerializeField]
	private float _minDampRatio = 0.5f;

	[SerializeField]
	private float _damageScaleU = 0.4f;

	[SerializeField]
	private float _damageScaleV = 0.4f;

	[SerializeField]
	private float _shakeScaleMaxU = 40f;

	[SerializeField]
	private float _shakeScaleMaxV = 40f;

	private readonly List<Vector2> _shakeDirRef = new List<Vector2>();

	private float _shakeScaleU;

	private float _shakeScaleV;

	private bool _isShakeActive;

	private Vector2 _damageShakerPos = Vector2.zero;

	private WaitForSeconds _waitForInterval;

	private void Start()
	{
		InitShakerDirRef();
	}

	private void InitShakerDirRef()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		float num = 0.5f;
		float num2 = 1f;
		_shakeDirRef.Add(new Vector2(0f, 0f));
		_shakeDirRef.Add(new Vector2(0f - num, num2));
		_shakeDirRef.Add(new Vector2(0f - num, 0f - num2));
		_shakeDirRef.Add(new Vector2(num, num2));
		_shakeDirRef.Add(new Vector2(num, 0f - num2));
	}

	private IEnumerator CoUpdateShake(float duration, float interval, float dampRatio)
	{
		_isShakeActive = true;
		int count = _shakeDirRef.Count;
		_waitForInterval = new WaitForSeconds(interval);
		int shakeIteration = Mathf.CeilToInt(duration / interval / 5f);
		float elapsedDampRatio = dampRatio;
		for (int i = 0; i < shakeIteration; i++)
		{
			for (int dirRef = 0; dirRef < count; dirRef++)
			{
				Vector2 v1 = _shakeDirRef[dirRef];
				v1.x *= _shakeScaleU * elapsedDampRatio;
				v1.y *= _shakeScaleV * elapsedDampRatio;
				_damageShakerPos = v1;
				yield return _waitForInterval;
			}
			elapsedDampRatio *= dampRatio;
		}
		_damageShakerPos = Vector2.zero;
		_isShakeActive = false;
	}

	private void LateUpdate()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (_isShakeActive)
		{
			Transform transform = ((Component)this).gameObject.transform;
			transform.position += new Vector3(_damageShakerPos.x, _damageShakerPos.y, 0f);
		}
	}

	public void DamageShake(int damage)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		_shakeScaleU = Mathf.Min((float)damage * _damageScaleU, _shakeScaleMaxU);
		_shakeScaleV = Mathf.Min((float)damage * _damageScaleV, _shakeScaleMaxV);
		_damageShakerPos = Vector2.zero;
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoUpdateShake(0.3f, 0.02f, _minDampRatio));
	}

	public void Shake(float shakeScaleU, float shakeScaleV, float updateInterval = -1f, float duration = -1f, float dampRatio = -1f)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		_shakeScaleU = Mathf.Min(shakeScaleU, _shakeScaleMaxU);
		_shakeScaleV = Mathf.Min(shakeScaleV, _shakeScaleMaxV);
		duration = Mathf.Max(duration, 0.3f);
		updateInterval = Mathf.Max(updateInterval, 0.02f);
		dampRatio = Mathf.Max(_minDampRatio, dampRatio);
		_damageShakerPos = Vector2.zero;
		((MonoBehaviour)this).StopAllCoroutines();
		((MonoBehaviour)this).StartCoroutine(CoUpdateShake(duration, updateInterval, dampRatio));
	}
}
