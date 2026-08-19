using System;
using System.Collections;
using Durango.Utils;
using UnityEngine;

namespace Durango.Prologue;

public class ScalableGaugeAddon : MonoBehaviour
{
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
		for (float time = 0f; time < _aniTime; time += Time.deltaTime)
		{
			float alpha = time / _aniTime;
			if (!isHorizontal)
			{
				_gaugeContent.height = (int)Mathf.Lerp(_gaugeContent.height, (float)Widget.height * ratio, alpha);
			}
			else
			{
				_gaugeContent.width = (int)Mathf.Lerp(_gaugeContent.width, (float)Widget.width * ratio, alpha);
			}
			yield return null;
		}
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
