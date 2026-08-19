using System;
using System.Collections;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class BinaryToggleSlider : MonoBehaviour
{
	public Action<bool> ValueChanged;

	public Action<float> ValueRatioChanged;

	public Action Clicked;

	private float _ratio;

	[SerializeField]
	private Transform _visualIcon;

	[SerializeField]
	private float _sliderPadding;

	[SerializeField]
	private float _animationDuration = 0.5f;

	[SerializeField]
	private bool _isHorizontal;

	[SerializeField]
	private UITweener[] _tweeners;

	private UIWidget _widget;

	private ICoroutineBinder _snapSequence;

	private bool _isDraged;

	public bool Disabled { get; private set; }

	public float Ratio
	{
		get
		{
			return _ratio;
		}
		private set
		{
			float ratio = _ratio;
			_ratio = Mathf.Clamp(value, 0f, 1f);
			Value = _ratio > 0.5f;
			SetTweener(_ratio);
			if (!_isHorizontal)
			{
				TranslateIconHorizontally(_ratio);
			}
			else
			{
				TranslateIconVertically(_ratio);
			}
			if (ratio != _ratio && ValueRatioChanged != null)
			{
				ValueRatioChanged(_ratio);
			}
		}
	}

	public bool Value { get; private set; }

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

	private void OnEnable()
	{
		SetTweener(Ratio);
	}

	[UsedImplicitly]
	private void OnPress(bool press)
	{
		if (!Disabled)
		{
			if (press)
			{
				_isDraged = false;
			}
			else if (_isDraged)
			{
				float ratioByLastTouch = GetRatioByLastTouch();
				float targetRatio = ((ratioByLastTouch > 0.5f) ? 1 : 0);
				Set(targetRatio, sendEvent: true, playAnimation: true);
			}
		}
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (Clicked != null)
		{
			Clicked();
		}
		if (!Disabled)
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			Set((!(Ratio > 0.5f)) ? 1f : 0f, sendEvent: true, playAnimation: true);
		}
	}

	[UsedImplicitly]
	private void OnDrag(Vector2 delta)
	{
		if (!Disabled)
		{
			_isDraged = true;
			this.StopCoroutine(_snapSequence);
			Ratio = GetRatioByLastTouch();
		}
	}

	public void Set(float targetRatio, bool sendEvent = false, bool playAnimation = false)
	{
		playAnimation = playAnimation && base.gameObject.activeInHierarchy;
		if (playAnimation)
		{
			this.StartCoroutine(ref _snapSequence, SnapSequence(targetRatio));
		}
		else
		{
			Ratio = targetRatio;
		}
		RaiseEvent(targetRatio, sendEvent);
	}

	public void SetDisabled(bool disabled)
	{
		Disabled = disabled;
		Widget.alpha = ((!disabled) ? 1f : 0.5f);
	}

	private void SetTweener(float ratio)
	{
		for (int i = 0; i < _tweeners.Length; i++)
		{
			if (!(_tweeners[i] == null))
			{
				_tweeners[i].Sample(ratio, isFinished: true);
			}
		}
	}

	private void RaiseEvent(float ratio, bool sendEvent)
	{
		if (sendEvent && ValueChanged != null)
		{
			bool obj = ratio > 0.5f;
			ValueChanged(obj);
		}
	}

	private IEnumerator SnapSequence(float targetRatio)
	{
		float elapsedTime = 0f;
		float aniTime = _animationDuration;
		while (elapsedTime < aniTime)
		{
			Ratio = Mathf.Lerp(Ratio, targetRatio, elapsedTime / aniTime);
			elapsedTime += Time.deltaTime;
			yield return null;
		}
		Ratio = targetRatio;
	}

	private float GetRatioByLastTouch()
	{
		Vector3 vector = base.transform.InverseTransformPoint(UICamera.lastWorldPosition);
		Vector2 pivotOffset = Widget.pivotOffset;
		if (!_isHorizontal)
		{
			float num = Widget.height;
			return vector.y / num + pivotOffset.y;
		}
		float num2 = Widget.width;
		return vector.x / num2 + pivotOffset.x;
	}

	private void TranslateIconHorizontally(float ratio)
	{
		Vector3 localPosition = _visualIcon.localPosition;
		Vector3[] localCorners = Widget.localCorners;
		float y = Mathf.Lerp(localCorners[0].y + _sliderPadding, localCorners[2].y - _sliderPadding, ratio);
		localPosition.y = y;
		_visualIcon.localPosition = localPosition;
	}

	private void TranslateIconVertically(float ratio)
	{
		Vector3 localPosition = _visualIcon.localPosition;
		Vector3[] localCorners = Widget.localCorners;
		float x = Mathf.Lerp(localCorners[0].x + _sliderPadding, localCorners[2].x - _sliderPadding, ratio);
		localPosition.x = x;
		_visualIcon.localPosition = localPosition;
	}
}
