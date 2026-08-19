using System.Collections;
using Holoville.HOTween;
using UnityEngine;

public class STTWaveWidget : MonoBehaviour
{
	internal class WaveTween
	{
		public float Delay;

		public int WaveBarHeight;

		public float Duration;

		public float StartedTime;
	}

	private const float mindB = -2f;

	private const float maxdB = 10f;

	[SerializeField]
	private UISprite[] _waveSprites;

	[SerializeField]
	private float _speed = 0.4f;

	[SerializeField]
	private float _speedRandomRange = 0.2f;

	[SerializeField]
	private EaseType _ease = (EaseType)2;

	[SerializeField]
	private float _randomDelayRange = 0.1f;

	private float _latestVolume;

	private WaveTween[] _tweens;

	private void VolumeChanged(float rmsdB)
	{
		_latestVolume = (rmsdB - -2f) / 12f;
	}

	private void OnEnable()
	{
		SpeechToText.OnRmsChanged += VolumeChanged;
		_latestVolume = 0f;
		if (_tweens == null)
		{
			_tweens = new WaveTween[_waveSprites.Length];
			for (int i = 0; i < _waveSprites.Length; i++)
			{
				_tweens[i] = new WaveTween();
			}
		}
		for (int j = 0; j < _waveSprites.Length; j++)
		{
			float delay = Random.Range(0f, 0.4f);
			SetTween(j, delay);
		}
		((MonoBehaviour)this).StartCoroutine(CoTweenUpdate());
	}

	private void TweenUpdate()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		for (int i = 0; i < _tweens.Length; i++)
		{
			float num = _tweens[i].Duration + _tweens[i].StartedTime;
			float num2 = _tweens[i].Duration * 2f + _tweens[i].StartedTime;
			if (realtimeSinceStartup >= num2)
			{
				float delay = Random.Range(0f, _randomDelayRange);
				SetTween(i, delay);
			}
			else if (realtimeSinceStartup >= num)
			{
				float num3 = 1f - (num2 - realtimeSinceStartup) / _tweens[i].Duration;
				int height = (int)((float)(_tweens[i].WaveBarHeight - 4) * num3);
				_waveSprites[i].height = height;
			}
			else
			{
				float num4 = (num - realtimeSinceStartup) / _tweens[i].Duration;
				int height2 = (int)((float)(_tweens[i].WaveBarHeight - 4) * num4);
				_waveSprites[i].height = height2;
			}
		}
	}

	private IEnumerator CoTweenUpdate()
	{
		while (true)
		{
			TweenUpdate();
			yield return null;
		}
	}

	private void SetTween(int index, float delay)
	{
		_tweens[index].WaveBarHeight = (int)(32f * _latestVolume + 4f);
		_tweens[index].Duration = _latestVolume * _speed + Random.Range(0f, _speedRandomRange);
		_tweens[index].StartedTime = Time.realtimeSinceStartup;
	}

	private void OnDisable()
	{
		SpeechToText.OnRmsChanged -= VolumeChanged;
		((MonoBehaviour)this).StopAllCoroutines();
		for (int i = 0; i < _waveSprites.Length; i++)
		{
			_waveSprites[i].height = 4;
		}
	}
}
