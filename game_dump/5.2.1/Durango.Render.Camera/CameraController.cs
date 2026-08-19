using System;
using Durango.Utils;
using UnityEngine;

namespace Durango.Render.Camera;

public class CameraController : Singleton<CameraController>
{
	public const float DefaultMinZoom = 0.42f;

	public const float DefaultMaxZoom = 2.2f;

	public static float LastZoom;

	[SerializeField]
	private float _scrollSpeed = 15f;

	private bool _lockZoomControl;

	private Sequence<float> _zoomRatio;

	private Sequence<float> _zoom;

	private Sequence<Vector3> _offset;

	private Sequence<Vector3> _angle;

	private Sequence<Target> _target;

	private Sequence<ZoomRange> _zoomRange;

	private Sequence<Vector2> _screenOffset;

	private Sequence[] _sequences;

	private float _overZoomResetAt;

	private float _overZoomOffset;

	private float _smothScroll;

	public event Action<float> TryOverZoom;

	protected override void OnAwake()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.GestureZoom, OnGestureZoomProcess, InputSystem.Priority.Lower);
		_zoomRatio = new Sequence<float>((NgInterpolate.Function ease, float begin, float end, float time, float duration) => ease(begin, end - begin, time, duration));
		_zoom = new Sequence<float>((NgInterpolate.Function ease, float begin, float end, float time, float duration) => ease(begin, end - begin, time, duration));
		_offset = new Sequence<Vector3>(delegate(NgInterpolate.Function ease, Vector3 begin, Vector3 end, float time, float duration)
		{
			Vector3 result4 = default(Vector3);
			for (int l = 0; l < 3; l++)
			{
				result4[l] = ease(begin[l], end[l] - begin[l], time, duration);
			}
			return result4;
		});
		_angle = new Sequence<Vector3>(delegate(NgInterpolate.Function ease, Vector3 begin, Vector3 end, float time, float duration)
		{
			Vector3 result3 = default(Vector3);
			for (int k = 0; k < 3; k++)
			{
				result3[k] = ease(begin[k], end[k] - begin[k], time, duration);
			}
			return result3;
		});
		_target = new Sequence<Target>(delegate(NgInterpolate.Function ease, Target begin, Target end, float time, float duration)
		{
			Vector3 vector = begin.Get();
			Vector3 vector2 = end.Get();
			Vector3 value2 = default(Vector3);
			for (int j = 0; j < 3; j++)
			{
				value2[j] = ease(vector[j], vector2[j] - vector[j], time, duration);
			}
			return new Target(value2);
		}, (Target value) => new Target(value.Get()));
		_zoomRange = new Sequence<ZoomRange>(delegate(NgInterpolate.Function ease, ZoomRange begin, ZoomRange end, float time, float duration)
		{
			ZoomRange result2 = default(ZoomRange);
			result2.Min = ease(begin.Min, end.Min - begin.Min, time, duration);
			result2.Max = ease(begin.Max, end.Max - begin.Max, time, duration);
			return result2;
		});
		_screenOffset = new Sequence<Vector2>(delegate(NgInterpolate.Function ease, Vector2 begin, Vector2 end, float time, float duration)
		{
			Vector2 result = default(Vector2);
			for (int i = 0; i < 2; i++)
			{
				result[i] = ease(begin[i], end[i] - begin[i], time, duration);
			}
			return result;
		});
		_sequences = new Sequence[7] { _zoomRatio, _zoom, _offset, _angle, _target, _zoomRange, _screenOffset };
		_zoomRatio.Value = 1f;
		_zoomRange.Value = new ZoomRange(0.3f, 6f);
		if (Debug.isDebugBuild)
		{
			_zoom.Value = Preferences.GetFloat("CameraZoom", 1f);
		}
		else
		{
			_zoom.Value = ((LastZoom <= 0f) ? 1f : LastZoom);
		}
	}

	protected override void OnDestroyed()
	{
		if (Debug.isDebugBuild)
		{
			Preferences.SetFloat("CameraZoom", _zoom.Value);
		}
		else
		{
			LastZoom = _zoom.Value;
		}
	}

	private void Update()
	{
		if (_smothScroll != 0f)
		{
			float num = _smothScroll * Time.deltaTime * _scrollSpeed;
			if (Mathf.Abs(_smothScroll) < Mathf.Abs(num))
			{
				ScrollZoom(_smothScroll);
				_smothScroll = 0f;
			}
			else
			{
				ScrollZoom(num);
				_smothScroll -= num;
			}
		}
		if (_overZoomResetAt > 0f && _overZoomResetAt < Time.time)
		{
			_overZoomOffset = 0f;
			_overZoomResetAt = 0f;
		}
		UpdateCamera();
	}

	public void ScrollZoom(float offset)
	{
		if (offset == 0f)
		{
			return;
		}
		offset = offset * _zoom.Value * 2f;
		float num = _zoom.Value + offset;
		SetZoom(num);
		float num2 = num - _zoom.Value;
		if (Mathf.Abs(num2) > 0f)
		{
			if (_overZoomOffset == 0f)
			{
				_smothScroll = 0f;
			}
			_overZoomOffset += num2 * (1f - Mathf.Abs(_overZoomOffset)) / _zoom.Value;
			_overZoomOffset = Mathf.Clamp(_overZoomOffset, -1f, 1f);
			_overZoomResetAt = Time.time + 0.1f;
			if (this.TryOverZoom != null)
			{
				this.TryOverZoom(_overZoomOffset);
			}
		}
	}

	private Vector3 ScreenOffsetToWorldOffset(Vector2 screenOffset)
	{
		if (screenOffset == Vector2.zero)
		{
			return Vector3.zero;
		}
		Vector3 vector = new Vector3(UnityEngine.Screen.width, UnityEngine.Screen.height, 0f) * 0.5f;
		Vector3 vector2 = MainCamera.ScreenPosToWorldPos(vector);
		Vector3 vector3 = MainCamera.ScreenPosToWorldPos(new Vector3(screenOffset.x, screenOffset.y, 0f) + vector);
		return vector2 - vector3;
	}

	public void SetZoom(float zoom)
	{
		float min = _zoomRange.Value.Min;
		float max = _zoomRange.Value.Max;
		zoom = Mathf.Clamp(zoom, min, max);
		_zoom.Value = zoom;
		zoom *= _zoomRatio.Value;
		if (_overZoomResetAt > 0f)
		{
			float num = _overZoomResetAt - Time.time;
			float num2 = 1f - Mathf.Abs(_overZoomOffset);
			num2 = 1f - num2 * num2;
			num2 *= Mathf.Sign(_overZoomOffset);
			zoom += num2 * zoom * 0.02f * (num / 0.1f);
		}
		Singleton<MainCamera>.Instance().Zoom = zoom * _zoomRatio.Value;
	}

	public void LockZoomControl(bool isLock)
	{
		_lockZoomControl = isLock;
	}

	private void UpdateCamera()
	{
		Vector3 vector = _target.Update().Get();
		Vector3 vector2 = _offset.Update();
		Vector3 angleOffset = _angle.Update();
		float zoom = _zoom.Update();
		_zoomRatio.Update();
		_zoomRange.Update();
		SetZoom(zoom);
		MainCamera mainCamera = Singleton<MainCamera>.Instance();
		mainCamera.AngleOffset = angleOffset;
		mainCamera.UpdateCameraTarget(vector + vector2);
		Vector2 screenOffset = _screenOffset.Update();
		Vector3 offset = ScreenOffsetToWorldOffset(screenOffset);
		mainCamera.PostUpdateCameraTarget(offset);
	}

	private void OnGestureZoomProcess(InputCommandMessage message)
	{
		if (!_lockZoomControl && !message.GestureTouchedUI && !UICamera.Raycast(message.GestureVector))
		{
			Vector3 gestureVector = message.GestureVector;
			_smothScroll += gestureVector.z;
			GameSystem<InputSystem>.Instance().Gesture.NotifyGestureProcessed();
		}
	}

	public CameraController Zoom(float zoom, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_zoom.Add(zoom, duration, type);
		return this;
	}

	public CameraController ZoomRatio(float zoomRatio, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_zoomRatio.Add(zoomRatio, duration, type);
		return this;
	}

	public CameraController Offset(Vector3 offset, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_offset.Add(offset, duration, type);
		return this;
	}

	public CameraController Angle(Vector3 offset, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_angle.Add(offset, duration, type);
		return this;
	}

	public CameraController ClearAngle()
	{
		_angle.Clear();
		_angle.Value = Vector3.zero;
		return this;
	}

	public CameraController Target(GameObject target, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_target.Add(new Target(target), duration, type);
		return this;
	}

	public CameraController Target(Vector3 target, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_target.Add(new Target(target), duration, type);
		return this;
	}

	public CameraController ZoomRange(float min, float max, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_zoomRange.Add(new ZoomRange(min, max), duration, type);
		return this;
	}

	public CameraController ScreenOffset(Vector2 offset, float duration, NgInterpolate.EaseType type = NgInterpolate.EaseType.Linear)
	{
		_screenOffset.Add(offset, duration, type);
		return this;
	}

	public CameraController Next()
	{
		return Delay(0f);
	}

	public CameraController Delay(float delay)
	{
		for (int i = 0; i < _sequences.Length; i++)
		{
			_sequences[i].Delay(delay);
		}
		return this;
	}
}
