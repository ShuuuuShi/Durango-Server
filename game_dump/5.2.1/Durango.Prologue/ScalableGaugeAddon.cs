using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class ScalableGaugeAddon : MonoBehaviour
{
	[CompilerGenerated]
	private sealed class _003CAnimatedGaugeSequence_003Ed__17 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public ScalableGaugeAddon _003C_003E4__this;

		public bool isHorizontal;

		public float ratio;

		private float _003Ctime_003E5__2;

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
		public _003CAnimatedGaugeSequence_003Ed__17(int _003C_003E1__state)
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
			ScalableGaugeAddon scalableGaugeAddon = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ctime_003E5__2 = 0f;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ctime_003E5__2 += Time.deltaTime;
				break;
			}
			if (_003Ctime_003E5__2 < scalableGaugeAddon._aniTime)
			{
				float t = _003Ctime_003E5__2 / scalableGaugeAddon._aniTime;
				if (!isHorizontal)
				{
					scalableGaugeAddon._gaugeContent.height = (int)Mathf.Lerp(scalableGaugeAddon._gaugeContent.height, (float)scalableGaugeAddon.Widget.height * ratio, t);
				}
				else
				{
					scalableGaugeAddon._gaugeContent.width = (int)Mathf.Lerp(scalableGaugeAddon._gaugeContent.width, (float)scalableGaugeAddon.Widget.width * ratio, t);
				}
				_003C_003E2__current = null;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
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
	private float _aniTime = 0.7f;

	[SerializeField]
	private float _scrollSensitivity = 0.1f;

	[SerializeField]
	private UIWidget _gaugeContent;

	[SerializeField]
	private bool _isHorizontal;

	private UIWidget _widget;

	private float _value;

	private float _min;

	private float _max;

	public Action<float> ValueChanged;

	private ICoroutineBinder _animatedSequence;

	public UIWidget Widget
	{
		get
		{
			if (_widget == null)
			{
				_widget = GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	private void OnPress(bool press)
	{
		UpdateSelectorDirectly();
	}

	private void OnDrag(Vector2 delta)
	{
		UpdateSelectorDirectly();
	}

	private void OnScroll(float delta)
	{
		Set(_value + delta * _scrollSensitivity, raiseEvent: true);
	}

	public void Init(float minRatio, float maxRatio, float ratio)
	{
		_min = minRatio;
		_max = maxRatio;
		Set(ratio);
	}

	public float Set(float value, bool raiseEvent = false, bool playAnimation = false)
	{
		_value = Mathf.Clamp(value, _min, _max);
		float num = Mathf.Abs(_value - _min) / (_max - _min);
		playAnimation = playAnimation && base.gameObject.activeInHierarchy;
		if (!_isHorizontal)
		{
			if (playAnimation)
			{
				this.StartCoroutine(ref _animatedSequence, AnimatedGaugeSequence(num, _isHorizontal));
			}
			else
			{
				_gaugeContent.height = (int)((float)Widget.height * num);
			}
		}
		else if (playAnimation)
		{
			this.StartCoroutine(ref _animatedSequence, AnimatedGaugeSequence(num, _isHorizontal));
		}
		else
		{
			_gaugeContent.width = (int)((float)Widget.width * num);
		}
		if (raiseEvent && ValueChanged != null)
		{
			ValueChanged(_value);
		}
		return _value;
	}

	private IEnumerator AnimatedGaugeSequence(float ratio, bool isHorizontal)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CAnimatedGaugeSequence_003Ed__17(0)
		{
			_003C_003E4__this = this,
			ratio = ratio,
			isHorizontal = isHorizontal
		};
	}

	private void UpdateSelectorDirectly()
	{
		Vector3 vector = base.transform.InverseTransformPoint(UICamera.lastWorldPosition);
		Vector2 pivotOffset = Widget.pivotOffset;
		float num = 1f;
		if (!_isHorizontal)
		{
			float num2 = Widget.height;
			num = vector.y / num2 + pivotOffset.y;
		}
		else
		{
			float num3 = Widget.width;
			num = vector.x / num3 + pivotOffset.x;
		}
		float value = _min + (_max - _min) * num;
		Set(value, raiseEvent: true);
	}
}
