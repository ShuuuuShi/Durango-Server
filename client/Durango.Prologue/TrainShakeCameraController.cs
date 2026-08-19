using System.Collections;
using UnityEngine;

namespace Durango.Prologue;

public class TrainShakeCameraController : MonoBehaviour
{
	[SerializeField]
	private int _maxVib = 3;

	[SerializeField]
	private float _u = 10f;

	[SerializeField]
	private float _v = 10f;

	[SerializeField]
	private float _w = 10f;

	[SerializeField]
	private float _period = 3f;

	[SerializeField]
	private float _minVibDuration = 0.1f;

	[SerializeField]
	private float _maxVibDuration = 0.5f;

	[SerializeField]
	private float _minVibInterval = 0.5f;

	[SerializeField]
	private float _maxVibInterval = 2f;

	private Vector3 _shakeDisplace = Vector3.zero;

	private Transform _transformCached;

	private IEnumerator Start()
	{
		_transformCached = base.transform;
		while (true)
		{
			float endTime = Time.time + Random.Range(_minVibDuration, _maxVibDuration);
			do
			{
				_shakeDisplace.x = Mathf.Sin(Time.time * _period) * _u;
				_shakeDisplace.y = Mathf.Sin(Time.time * _period) * _v;
				_shakeDisplace.z = Mathf.Sin(Time.time * _period) * _w;
				yield return null;
			}
			while (!(Time.time > endTime));
			yield return new WaitForSeconds(Random.Range(_minVibInterval, _maxVibInterval));
		}
	}

	private void LateUpdate()
	{
		_transformCached.localPosition += new Vector3(_shakeDisplace.x, _shakeDisplace.y, _shakeDisplace.z);
	}
}
