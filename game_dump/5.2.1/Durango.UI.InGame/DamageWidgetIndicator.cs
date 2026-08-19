using System;
using Durango.Render.Camera;
using Messages;
using UnityEngine;

namespace Durango.UI.InGame;

public class DamageWidgetIndicator : UIWidget
{
	private enum PositionType
	{
		None,
		Transform
	}

	public Action<DamageWidgetIndicator> Finished;

	[SerializeField]
	private DamageWidget _damageWidget;

	[SerializeField]
	private UISprite _line1;

	[SerializeField]
	private UISprite _line2;

	[SerializeField]
	private int _lineLengthModifier;

	[SerializeField]
	private float _duration = 1f;

	private PositionType _positionType;

	private Transform _targetTransform;

	private int _startCorner;

	private int _endCorner;

	private float? _finishedAt;

	protected override void OnUpdate()
	{
		base.OnUpdate();
		float? finishedAt = _finishedAt;
		if (!finishedAt.HasValue)
		{
			return;
		}
		float num = _finishedAt.Value - Time.time;
		UpdateLine();
		if (num < 0.5f)
		{
			alpha = num / 0.5f;
		}
		if (num <= 0f)
		{
			_finishedAt = null;
			if (Finished != null)
			{
				Finished(this);
			}
		}
	}

	private void ClearTarget()
	{
		_positionType = PositionType.None;
		_line1.gameObject.SetActive(value: false);
		_line2.gameObject.SetActive(value: false);
	}

	public void Set(Damage damage, DamageableEntity target)
	{
		_positionType = PositionType.Transform;
		_targetTransform = target.GetBodyPartTransform(damage.Part);
		_line1.gameObject.SetActive(value: true);
		_line2.gameObject.SetActive(value: true);
		_damageWidget.Set(damage);
	}

	private bool GetTargetPos(out Vector2 pos)
	{
		bool result = true;
		pos = Vector2.zero;
		switch (_positionType)
		{
		case PositionType.None:
			result = false;
			break;
		case PositionType.Transform:
			if (_targetTransform == null)
			{
				result = false;
			}
			else
			{
				pos = MainCamera.WorldToNGUIPos(_targetTransform.position);
			}
			break;
		}
		return result;
	}

	private void UpdateLine()
	{
		if (GetTargetPos(out var pos))
		{
			Vector3 localPosition = _damageWidget.transform.localPosition;
			Vector3[] array = _damageWidget.localCorners;
			Vector2 vector = array[_startCorner] + localPosition;
			Vector2 vector2 = array[_endCorner] + localPosition;
			Transform transform = _line1.transform;
			Vector3 vector3 = vector2 - vector;
			float num = Mathf.Atan2(vector3.y, vector3.x) * 57.29578f;
			transform.localPosition = vector;
			transform.localEulerAngles = Vector3.forward * num;
			_line1.width = (int)vector3.magnitude + _lineLengthModifier;
			Transform obj = _line2.transform;
			Vector3 vector4 = vector2 - pos;
			float num2 = Mathf.Atan2(vector4.y, vector4.x) * 57.29578f;
			obj.localPosition = pos;
			obj.localEulerAngles = Vector3.forward * num2;
			_line2.width = (int)vector4.magnitude + _lineLengthModifier;
		}
		else
		{
			ClearTarget();
		}
	}

	public void Begin()
	{
		_finishedAt = Time.time + _duration;
		if (!GetTargetPos(out var pos))
		{
			return;
		}
		Vector2 vector = pos;
		vector.x += (float)_damageWidget.width * UnityEngine.Random.Range(-0.8f, 0.8f);
		if (vector.y > 200f)
		{
			vector.y -= 100f;
		}
		else
		{
			vector.y += 100f;
		}
		vector.x = Mathf.Clamp(vector.x, -300f, 300f);
		vector.y = Mathf.Clamp(vector.y, -150f, 150f);
		if (vector.x > pos.x)
		{
			if (vector.y > pos.y)
			{
				_startCorner = 2;
				_endCorner = 3;
			}
			else
			{
				_startCorner = 3;
				_endCorner = 2;
			}
		}
		else if (vector.y > pos.y)
		{
			_startCorner = 1;
			_endCorner = 0;
		}
		else
		{
			_startCorner = 1;
			_endCorner = 0;
		}
		_damageWidget.transform.localPosition = vector;
		_damageWidget.ShowAnimation();
		alpha = 1f;
	}
}
