using System;
using System.Collections;
using UnityEngine;

namespace Durango.Environment;

public class CloudUpdater : MonoBehaviour
{
	private const float Radius = 5000f;

	[SerializeField]
	private GameObject _cloud;

	[SerializeField]
	private float _appearPeriod;

	[SerializeField]
	private float _runTime;

	[SerializeField]
	private float _speed;

	[SerializeField]
	private float _fadeSpeed;

	private Material _material;

	private int _alphaId;

	private Vector3 _flowDir;

	private float _initialScale;

	private WaitForSeconds _waitForSeconds;

	private void Start()
	{
		_cloud.SetActive(value: true);
		_material = _cloud.GetComponent<MeshRenderer>().material;
		_alphaId = Shader.PropertyToID("_AlphaRatio");
		_material.SetFloat(_alphaId, 1f);
		float value = UnityEngine.Random.value;
		float z = Mathf.Sin((float)Math.PI * 2f * value);
		float x = Mathf.Cos((float)Math.PI * 2f * value);
		_flowDir = new Vector3(x, 0f, z);
		_initialScale = _cloud.transform.localScale.x;
		_waitForSeconds = new WaitForSeconds(_appearPeriod);
		StartCoroutine(CoProcessCloud());
	}

	private IEnumerator CoProcessCloud()
	{
		while (true)
		{
			Vector3 localPlayerPos = PlayerBehavior.LocalPlayer.CurrentPosition;
			Vector3 startPos = GetRandomPos(localPlayerPos, 5000f);
			_cloud.transform.rotation = Quaternion.Euler(new Vector3(90f, UnityEngine.Random.Range(0, 360), 0f));
			_cloud.transform.position = startPos;
			float randomScale = _initialScale * (1f + UnityEngine.Random.Range(-0.2f, 0.2f));
			_cloud.transform.localScale = new Vector3(randomScale, randomScale, 0f);
			yield return StartCoroutine(CoFlowFading(appear: true));
			float elapsedTime = 0f;
			while (elapsedTime < _runTime)
			{
				MoveCloud();
				elapsedTime += Time.deltaTime;
				yield return null;
			}
			yield return StartCoroutine(CoFlowFading(appear: false));
			yield return _waitForSeconds;
		}
	}

	private IEnumerator CoFlowFading(bool appear)
	{
		float alpha = _material.GetFloat(_alphaId);
		while ((appear && alpha > 0f) || (!appear && alpha < 1f))
		{
			MoveCloud();
			float delta = _fadeSpeed * Time.deltaTime;
			alpha = ((!appear) ? (alpha + delta) : (alpha - delta));
			_material.SetFloat(_alphaId, Mathf.Clamp01(alpha));
			yield return null;
		}
	}

	private void MoveCloud()
	{
		_cloud.transform.position = _cloud.transform.position + _flowDir * Time.deltaTime * _speed;
	}

	private static Vector3 GetRandomPos(Vector3 center, float radius)
	{
		float num = UnityEngine.Random.Range(-1f, 1f);
		float num2 = UnityEngine.Random.Range(-1f, 1f);
		return center + new Vector3(num * radius, 0f, num2 * radius);
	}
}
