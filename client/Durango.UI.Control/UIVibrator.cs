using UnityEngine;

namespace Durango.UI.Control;

public class UIVibrator : MonoBehaviour
{
	[SerializeField]
	private Vector3 _amplitude;

	[SerializeField]
	private float _duration = 1f;

	[SerializeField]
	private float _period = 0.05f;

	private Vector3 _origin;

	private float _enabledAt;

	private void Awake()
	{
		base.enabled = false;
	}

	private void OnEnable()
	{
		_origin = base.transform.localPosition;
		_duration = Mathf.Max(_duration, 0.1f);
		_period = Mathf.Max(_period, 0.01f);
		_enabledAt = Time.time;
	}

	private void OnDisable()
	{
		base.enabled = false;
	}

	private void Update()
	{
		float time = Time.time;
		if (_enabledAt + _duration < time)
		{
			base.enabled = false;
			base.transform.localPosition = _origin;
			return;
		}
		float num = time - _enabledAt;
		float num2 = num / _duration;
		float num3 = num / _period;
		int num4 = (int)num3;
		float t = num3 - (float)num4;
		Vector3 vector = ((num4 % 2 != 0) ? (-_amplitude) : _amplitude) * (1f - num2);
		base.transform.localPosition = Vector3.Lerp(_origin, _origin + vector, t);
	}
}
