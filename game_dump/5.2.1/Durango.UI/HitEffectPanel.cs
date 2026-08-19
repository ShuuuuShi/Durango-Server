using UnityEngine;

namespace Durango.UI;

public class HitEffectPanel : MonoBehaviour
{
	[SerializeField]
	private UISprite[] _sprites;

	private float _beginTime;

	private float _duration;

	private Color _color;

	[ExposedInEditor(null)]
	public void Play(float duration, Color color)
	{
		base.gameObject.SetActive(value: true);
		_beginTime = Time.time;
		_duration = duration;
		_color = color;
		for (int i = 0; i < _sprites.Length; i++)
		{
			_sprites[i].color = color;
		}
	}

	private void Update()
	{
		if (_beginTime + _duration <= Time.time)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		float t = (Time.time - _beginTime) / _duration;
		for (int i = 0; i < _sprites.Length; i++)
		{
			_sprites[i].alpha = Mathf.Lerp(_color.a, 0f, t);
		}
	}
}
