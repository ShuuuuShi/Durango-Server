using UnityEngine;

namespace Durango.Cutscene;

public class CameraWiggle : MonoBehaviour
{
	[SerializeField]
	private float _wiggleXyScale;

	[SerializeField]
	private float _wiggleZScale;

	[SerializeField]
	private float _defaultWiggleTime;

	private Vector3 _offset;

	private Vector3 _targetPosition;

	private Vector3 _lastPostion;

	private float _wiggleTime;

	private bool _isWiggling;

	public void Play(bool value)
	{
		_isWiggling = value;
		if (_isWiggling)
		{
			_wiggleTime = _defaultWiggleTime;
		}
	}

	private static float InOut(float k)
	{
		if ((k *= 2f) < 1f)
		{
			return 0.5f * k * k;
		}
		return -0.5f * ((k -= 1f) * (k - 2f) - 1f);
	}

	private void Update()
	{
		if (_isWiggling)
		{
			if (_wiggleTime >= _defaultWiggleTime)
			{
				float x = Random.Range(0f - _wiggleXyScale, _wiggleXyScale);
				float y = Random.Range(0f - _wiggleXyScale, _wiggleXyScale);
				float z = Random.Range(0f - _wiggleZScale, _wiggleZScale);
				_lastPostion = _targetPosition;
				_targetPosition = new Vector3(x, y, z);
				_wiggleTime = 0f;
			}
			_wiggleTime += Time.deltaTime;
			_offset = Vector3.Lerp(_lastPostion, _targetPosition, InOut(_wiggleTime / _defaultWiggleTime));
		}
		base.transform.localPosition = _offset;
	}
}
