using System.Collections;
using UnityEngine;

public class JoystickGroup : UIBase
{
	[SerializeField]
	private UIWidget _fixedModeContainer;

	private Rect? _fixedModeContainerRect;

	public Transform _target;

	public Vector3 _scale = Vector3.one;

	public float _radius = 40f;

	public float _followBeginRadius = 80f;

	public float _followEndRadius = 80f;

	public float _followSpeed = 0.5f;

	public float _visibleMagnitude = 1f;

	private bool _followBegin;

	private Vector3 _userInitTouchPos;

	public bool _normalize;

	public Vector2 _position;

	public float _deadZone = 2f;

	public float _fadeOutAlpha = 0.2f;

	public float _fadeOutDelay = 1f;

	public UIWidget[] _widgetsToFade;

	public Transform[] _widgetsToCenter;

	private bool _fixedMode;

	public bool IsVisible { get; private set; }

	public bool Pressed { get; private set; }

	public Vector3 InitTouchPos => UICamera.currentCamera.WorldToScreenPoint(_userInitTouchPos);

	public Rect GetFixedModeContainerRect()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!_fixedModeContainerRect.HasValue)
		{
			Vector3 val = UICamera.currentCamera.WorldToScreenPoint(((Component)_fixedModeContainer).transform.position);
			float num = _fixedModeContainer.width;
			float num2 = _fixedModeContainer.height;
			float num3 = val.x - num / 2f;
			float num4 = val.y - num2 / 2f;
			_fixedModeContainerRect = new Rect(num3, num4, num, num2);
		}
		return _fixedModeContainerRect.Value;
	}

	private void Start()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < _widgetsToFade.Length; i++)
		{
			UIWidget uIWidget = _widgetsToFade[i];
			Color color = uIWidget.color;
			color.a = 0f;
			uIWidget.color = color;
		}
		KSingleton<PlayerController>.Instance().DrawModeChanged += SetFixedMode;
	}

	private IEnumerator fadeOutJoystick()
	{
		IsVisible = false;
		yield return (object)new WaitForSeconds(_fadeOutDelay);
		for (int i = 0; i < _widgetsToFade.Length; i++)
		{
			UIWidget widget = _widgetsToFade[i];
			Color lastColor = widget.color;
			Color newColor = lastColor;
			newColor.a = _fadeOutAlpha;
			TweenColor.Begin(((Component)widget).gameObject, 0.5f, newColor).method = UITweener.Method.EaseOut;
		}
		yield return (object)new WaitForSeconds(0.5f);
	}

	private void SetFixedMode(bool enable)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		_fixedMode = enable;
		((Component)_fixedModeContainer).gameObject.SetActive(enable);
		if (_fixedMode)
		{
			((MonoBehaviour)this).StopAllCoroutines();
			Vector3 position = ((Component)_fixedModeContainer).transform.position;
			Transform[] widgetsToCenter = _widgetsToCenter;
			foreach (Transform val in widgetsToCenter)
			{
				val.position = position;
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
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_target != (Object)null)
		{
			Pressed = true;
			Ray val = UICamera.currentCamera.ScreenPointToRay(currentPos);
			float num = 0f;
			currentPos = ((Ray)(ref val)).GetPoint(num);
			currentPos.z = 0f;
			_userInitTouchPos = currentPos;
			if (_fixedMode)
			{
				_userInitTouchPos = ((Component)_fixedModeContainer).transform.position;
			}
			Transform[] widgetsToCenter = _widgetsToCenter;
			foreach (Transform val2 in widgetsToCenter)
			{
				val2.position = _userInitTouchPos;
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

	public void Drag(Vector3 currentPos)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_029b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		Ray val = UICamera.currentCamera.ScreenPointToRay(currentPos);
		float num = 0f;
		currentPos = ((Ray)(ref val)).GetPoint(num);
		currentPos.z = 0f;
		Vector3 val2 = currentPos - _userInitTouchPos;
		if (val2.x != 0f || val2.y != 0f)
		{
			val2 = _target.InverseTransformDirection(val2);
			((Vector3)(ref val2)).Scale(_scale);
			val2 = _target.TransformDirection(val2);
			val2.z = 0f;
		}
		_target.position = _userInitTouchPos + val2;
		Vector3 localPosition = _target.localPosition;
		float magnitude = ((Vector3)(ref localPosition)).magnitude;
		if (magnitude < _deadZone)
		{
			_position = Vector2.zero;
			_target.localPosition = Vector2.op_Implicit(_position);
		}
		else
		{
			if (((_followBeginRadius > 0f && magnitude > _followBeginRadius) || _followBegin) && !_fixedMode)
			{
				float num2 = Mathf.Max(0f, magnitude - _followBeginRadius);
				float num3 = Mathf.Clamp(num2 / 10f, 1f, 3f);
				Vector3 val3 = val2;
				((Vector3)(ref val3)).Normalize();
				_userInitTouchPos += Time.deltaTime * val3 * _followSpeed * num3;
				for (int i = 0; i < _widgetsToCenter.Length; i++)
				{
					Transform val4 = _widgetsToCenter[i];
					val4.position = _userInitTouchPos;
				}
				_followBegin = true;
			}
			_target.localPosition = Vector3.ClampMagnitude(_target.localPosition, _radius);
			_position = Vector2.op_Implicit(_target.localPosition);
			if (!IsVisible && ((Vector2)(ref _position)).magnitude > _visibleMagnitude)
			{
				IsVisible = true;
				((MonoBehaviour)this).StopAllCoroutines();
				for (int j = 0; j < _widgetsToFade.Length; j++)
				{
					UIWidget uIWidget = _widgetsToFade[j];
					TweenColor.Begin(((Component)uIWidget).gameObject, 0.1f, Color.white).method = UITweener.Method.EaseIn;
				}
			}
		}
		if (magnitude <= _followEndRadius && _followBegin)
		{
			_followBegin = false;
		}
		if (_normalize)
		{
			_position = _position / _radius * Mathf.InverseLerp(_radius, _deadZone, 1f);
		}
	}

	private void ResetJoystick(bool fadeOut = true)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_position = Vector2.zero;
		_target.position = _userInitTouchPos;
		if (fadeOut)
		{
			((MonoBehaviour)this).StartCoroutine(fadeOutJoystick());
		}
	}
}
