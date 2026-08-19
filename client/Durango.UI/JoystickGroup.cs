using System;
using System.Collections;
using UnityEngine;

namespace Durango.UI;

public class JoystickGroup : UIBase
{
	[SerializeField]
	private Transform _target;

	[SerializeField]
	private Vector3 _scale = Vector3.one;

	[SerializeField]
	private float _radius = 40f;

	[SerializeField]
	private float _followBeginRadius = 80f;

	[SerializeField]
	private float _followEndRadius = 80f;

	[SerializeField]
	private float _followSpeed = 0.5f;

	[SerializeField]
	private float _visibleMagnitude = 1f;

	[SerializeField]
	private bool _normalize;

	[SerializeField]
	private float _deadZone = 2f;

	[SerializeField]
	private float _fadeOutAlpha = 0.2f;

	[SerializeField]
	private float _fadeOutDelay = 1f;

	[SerializeField]
	private UIWidget[] _widgetsToFade;

	[SerializeField]
	private Transform[] _widgetsToCenter;

	[SerializeField]
	private UIWidget _fixedModeContainer;

	private Rect? _fixedModeContainerRect;

	private bool _followBegin;

	private Vector3 _userInitTouchPos;

	private bool _fixedMode;

	public Vector2 Position { get; private set; }

	public bool IsVisible { get; private set; }

	public bool Pressed { get; private set; }

	public Rect GetFixedModeContainerRect()
	{
		if (_fixedModeContainerRect.HasValue)
		{
			return _fixedModeContainerRect.Value;
		}
		Vector3 vector = UICamera.currentCamera.WorldToScreenPoint(_fixedModeContainer.transform.position);
		float num = _fixedModeContainer.width;
		float num2 = _fixedModeContainer.height;
		float x = vector.x - num / 2f;
		float y = vector.y - num2 / 2f;
		_fixedModeContainerRect = new Rect(x, y, num, num2);
		return _fixedModeContainerRect.Value;
	}

	private void Start()
	{
		for (int i = 0; i < _widgetsToFade.Length; i++)
		{
			UIWidget uIWidget = _widgetsToFade[i];
			Color color = uIWidget.color;
			color.a = 0f;
			uIWidget.color = color;
		}
		GameSystem<InputSystem>.Instance().DrawModeChanged += SetFixedMode;
		UIWidget rootAnchor = UIRootAnchor.GetRootAnchor(AnchorType.Base);
		if (rootAnchor != null)
		{
			_fixedModeContainer.SetAnchor(rootAnchor.transform);
		}
		_fixedModeContainer.gameObject.SetActive(value: false);
		SetFixedMode(drawMode: false);
	}

	private IEnumerator FadeOutJoystick()
	{
		IsVisible = false;
		yield return new WaitForSeconds(_fadeOutDelay);
		for (int i = 0; i < _widgetsToFade.Length; i++)
		{
			UIWidget uIWidget = _widgetsToFade[i];
			Color color = uIWidget.color;
			Color color2 = color;
			color2.a = _fadeOutAlpha;
			TweenColor.Begin(uIWidget.gameObject, 0.5f, color2).method = UITweener.Method.EaseOut;
		}
		yield return new WaitForSeconds(0.5f);
	}

	private void SetFixedMode(bool drawMode)
	{
		_fixedMode = drawMode;
		_fixedModeContainer.gameObject.SetActive(drawMode);
		if (_fixedMode)
		{
			StopAllCoroutines();
			Vector3 position = _fixedModeContainer.transform.position;
			Transform[] widgetsToCenter = _widgetsToCenter;
			foreach (Transform transform in widgetsToCenter)
			{
				transform.position = position;
			}
			for (int j = 0; j < _widgetsToFade.Length; j++)
			{
				UIWidget uIWidget = _widgetsToFade[j];
				Color color = uIWidget.color;
				color.a = 1f;
				uIWidget.color = color;
			}
		}
		else
		{
			ResetJoystick();
		}
	}

	public void Press(Vector3 currentPos)
	{
		if (!(_target == null) && !(UICamera.currentCamera == null))
		{
			Pressed = true;
			currentPos = UICamera.currentCamera.ScreenPointToRay(currentPos).GetPoint(0f);
			currentPos.z = 0f;
			_userInitTouchPos = currentPos;
			if (_fixedMode)
			{
				currentPos = _fixedModeContainer.transform.position;
			}
			Transform[] widgetsToCenter = _widgetsToCenter;
			foreach (Transform transform in widgetsToCenter)
			{
				transform.position = currentPos;
			}
		}
	}

	public void Release()
	{
		if (Pressed)
		{
			Pressed = false;
			ResetJoystick(!_fixedMode);
		}
	}

	public bool Drag(Vector3 currentPos)
	{
		if (UICamera.currentCamera == null)
		{
			return false;
		}
		Ray ray = UICamera.currentCamera.ScreenPointToRay(currentPos);
		float distance = 0f;
		currentPos = ray.GetPoint(distance);
		currentPos.z = 0f;
		Vector3 vector = currentPos - _userInitTouchPos;
		if (_fixedMode)
		{
			if (!(Math.Abs(vector.x) > float.Epsilon) && !(Math.Abs(vector.y) > float.Epsilon))
			{
				return false;
			}
			_userInitTouchPos = _fixedModeContainer.transform.position;
			vector = currentPos - _userInitTouchPos;
		}
		if (Math.Abs(vector.x) > float.Epsilon || Math.Abs(vector.y) > float.Epsilon)
		{
			vector = _target.InverseTransformDirection(vector);
			vector.Scale(_scale);
			vector = _target.TransformDirection(vector);
			vector.z = 0f;
		}
		_target.position = _userInitTouchPos + vector;
		float magnitude = _target.localPosition.magnitude;
		if (magnitude < _deadZone)
		{
			Position = Vector2.zero;
			_target.localPosition = Position;
		}
		else
		{
			if (((_followBeginRadius > 0f && magnitude > _followBeginRadius) || _followBegin) && !_fixedMode)
			{
				float num = Mathf.Max(0f, magnitude - _followBeginRadius);
				float num2 = Mathf.Clamp(num / 10f, 1f, 3f);
				Vector3 vector2 = vector;
				vector2.Normalize();
				_userInitTouchPos += Time.deltaTime * vector2 * _followSpeed * num2;
				for (int i = 0; i < _widgetsToCenter.Length; i++)
				{
					Transform transform = _widgetsToCenter[i];
					transform.position = _userInitTouchPos;
				}
				_followBegin = true;
			}
			_target.localPosition = Vector3.ClampMagnitude(_target.localPosition, _radius);
			Position = _target.localPosition;
			if (!IsVisible && Position.magnitude > _visibleMagnitude)
			{
				IsVisible = true;
				StopAllCoroutines();
				Color white = Color.white;
				for (int j = 0; j < _widgetsToFade.Length; j++)
				{
					UIWidget uIWidget = _widgetsToFade[j];
					TweenColor.Begin(uIWidget.gameObject, 0.1f, white).method = UITweener.Method.EaseIn;
				}
			}
		}
		if (magnitude <= _followEndRadius && _followBegin)
		{
			_followBegin = false;
		}
		if (_normalize)
		{
			Position = Position / _radius * Mathf.InverseLerp(_radius, _deadZone, 1f);
		}
		return true;
	}

	private void ResetJoystick(bool fadeOut = true)
	{
		Position = Vector2.zero;
		_target.position = ((!_fixedMode) ? _userInitTouchPos : _fixedModeContainer.transform.position);
		if (fadeOut)
		{
			StartCoroutine(FadeOutJoystick());
		}
		else
		{
			IsVisible = false;
		}
	}
}
