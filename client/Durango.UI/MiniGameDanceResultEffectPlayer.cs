using Durango.Render.Particle;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

[RequireComponent(typeof(UIWidget))]
public class MiniGameDanceResultEffectPlayer : MonoBehaviour
{
	[SerializeField]
	private ParticleType _particle;

	[SerializeField]
	[Range(1f, 30f)]
	private float _intervalMin = 5f;

	[SerializeField]
	[Range(1f, 30f)]
	private float _intervalMax = 10f;

	private bool _isPlaying;

	private float _nextEffectTime;

	private float NextIntervalTime => Random.Range(_intervalMin, _intervalMax);

	private Vector3 EffectPosition
	{
		get
		{
			Vector3 vector = Vector3.zero;
			UIWidget component = GetComponent<UIWidget>();
			if (component != null)
			{
				Vector2 vector2 = new Vector2((float)component.width * 0.5f, (float)component.height * 0.5f);
				Vector3 localScale = Singleton<UIManager>.Instance().UIRoot.transform.localScale;
				vector = new Vector3(Random.Range(0f - vector2.x, vector2.x), Random.Range(0f - vector2.y, vector2.y), 0f);
				vector.x *= localScale.x;
				vector.y *= localScale.y;
			}
			return base.transform.position + vector;
		}
	}

	private void OnValidate()
	{
		_intervalMax = Mathf.Max(_intervalMax, _intervalMin);
		_intervalMin = Mathf.Min(_intervalMax, _intervalMin);
	}

	public void Play()
	{
		if (!string.IsNullOrEmpty(_particle.Path))
		{
			_isPlaying = true;
			_nextEffectTime = Time.time;
		}
	}

	public void Stop()
	{
		_isPlaying = false;
		_nextEffectTime = 0f;
	}

	private void Update()
	{
		if (_isPlaying)
		{
			float time = Time.time;
			if (!(time < _nextEffectTime))
			{
				ParticleManager.Emit(_particle, EffectPosition, Quaternion.identity);
				_nextEffectTime = time + NextIntervalTime;
			}
		}
	}
}
