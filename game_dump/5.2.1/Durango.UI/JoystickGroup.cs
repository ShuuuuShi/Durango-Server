using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Durango.UI;

public class JoystickGroup : UIBase
{
	[CompilerGenerated]
	private sealed class _003CFadeOutJoystick_003Ed__32 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public JoystickGroup _003C_003E4__this;

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
		public _003CFadeOutJoystick_003Ed__32(int _003C_003E1__state)
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
			JoystickGroup joystickGroup = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				joystickGroup.IsVisible = false;
				_003C_003E2__current = new WaitForSeconds(joystickGroup._fadeOutDelay);
				_003C_003E1__state = 1;
				return true;
			case 1:
			{
				_003C_003E1__state = -1;
				for (int i = 0; i < joystickGroup._widgetsToFade.Length; i++)
				{
					UIWidget obj = joystickGroup._widgetsToFade[i];
					Color color = obj.color;
					color.a = joystickGroup._fadeOutAlpha;
					TweenColor.Begin(obj.gameObject, 0.5f, color).method = UITweener.Method.EaseOut;
				}
				_003C_003E2__current = new WaitForSeconds(0.5f);
				_003C_003E1__state = 2;
				return true;
			}
			case 2:
				_003C_003E1__state = -1;
				return false;
			}
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
			UIWidget obj = _widgetsToFade[i];
			Color color = obj.color;
			color.a = 0f;
			obj.color = color;
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CFadeOutJoystick_003Ed__32(0)
		{
			_003C_003E4__this = this
		};
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
			for (int i = 0; i < widgetsToCenter.Length; i++)
			{
				widgetsToCenter[i].position = position;
			}
			for (int j = 0; j < _widgetsToFade.Length; j++)
			{
				UIWidget obj = _widgetsToFade[j];
				Color color = obj.color;
				color.a = 1f;
				obj.color = color;
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
			for (int i = 0; i < widgetsToCenter.Length; i++)
			{
				widgetsToCenter[i].position = currentPos;
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
				float num = Mathf.Clamp(Mathf.Max(0f, magnitude - _followBeginRadius) / 10f, 1f, 3f);
				Vector3 vector2 = vector;
				vector2.Normalize();
				_userInitTouchPos += Time.deltaTime * vector2 * _followSpeed * num;
				for (int i = 0; i < _widgetsToCenter.Length; i++)
				{
					_widgetsToCenter[i].position = _userInitTouchPos;
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
					TweenColor.Begin(_widgetsToFade[j].gameObject, 0.1f, white).method = UITweener.Method.EaseIn;
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
