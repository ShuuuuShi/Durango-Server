using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.UI;

public class STTWaveWidget : MonoBehaviour
{
	internal class WaveTween
	{
		public float Delay;

		public int WaveBarHeight;

		public float Duration;

		public float StartedTime;
	}

	[CompilerGenerated]
	private sealed class _003CCoTweenUpdate_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public STTWaveWidget _003C_003E4__this;

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
		public _003CCoTweenUpdate_003Ed__12(int _003C_003E1__state)
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
			STTWaveWidget sTTWaveWidget = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
			}
			sTTWaveWidget.TweenUpdate();
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
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
	private UISprite[] _waveSprites;

	[SerializeField]
	private float _speed = 0.4f;

	[SerializeField]
	private float _speedRandomRange = 0.2f;

	[SerializeField]
	private float _randomDelayRange = 0.1f;

	private const float mindB = -2f;

	private const float maxdB = 10f;

	private float _latestVolume;

	private WaveTween[] _tweens;

	private void VolumeChanged(float rmsdB)
	{
		_latestVolume = (rmsdB - -2f) / 12f;
	}

	private void OnEnable()
	{
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
			float delay = UnityEngine.Random.Range(0f, 0.4f);
			SetTween(j, delay);
		}
		StartCoroutine(CoTweenUpdate());
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
				float delay = UnityEngine.Random.Range(0f, _randomDelayRange);
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoTweenUpdate_003Ed__12(0)
		{
			_003C_003E4__this = this
		};
	}

	private void SetTween(int index, float delay)
	{
		_tweens[index].WaveBarHeight = (int)(32f * _latestVolume + 4f);
		_tweens[index].Duration = _latestVolume * _speed + UnityEngine.Random.Range(0f, _speedRandomRange);
		_tweens[index].StartedTime = Time.realtimeSinceStartup;
	}

	private void OnDisable()
	{
		StopAllCoroutines();
		for (int i = 0; i < _waveSprites.Length; i++)
		{
			_waveSprites[i].height = 4;
		}
	}
}
