using System;
using System.Collections;
using Messages;
using UnityEngine;

public class DamageWidgetIndicator : MonoBehaviour
{
	private enum PositionType
	{
		None,
		World,
		UI,
		Transform
	}

	public Action<DamageWidgetIndicator> OnFinished;

	[SerializeField]
	private DamageWidget _damageWidget;

	[SerializeField]
	private UISprite _line1;

	[SerializeField]
	private UISprite _line2;

	[SerializeField]
	private int _lineLengthModifier;

	private UIWidget _widget;

	private PositionType _positionType;

	private Vector3 _targetWorldPosition;

	private Vector2 _targetUIPosition;

	private Transform _targetTransform;

	private Vector3 _targetOffset;

	private int _startCorner;

	private int _endCorner;

	public UIWidget Widget => (!((Object)(object)_widget != (Object)null)) ? (_widget = ((Component)this).GetComponent<UIWidget>()) : _widget;

	private bool HasTarget => _positionType != PositionType.None;

	public void Init()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.localPosition = Vector3.zero;
	}

	public void SetTarget(Vector3 worldPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_positionType = PositionType.World;
		_targetWorldPosition = worldPosition;
		((Component)_line1).gameObject.SetActive(true);
		((Component)_line2).gameObject.SetActive(true);
	}

	public void SetTarget(Vector2 uiPosition)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		_positionType = PositionType.UI;
		_targetUIPosition = uiPosition;
		((Component)_line1).gameObject.SetActive(true);
		((Component)_line2).gameObject.SetActive(true);
	}

	public void SetTarget(Transform trans)
	{
		_positionType = PositionType.Transform;
		_targetTransform = trans;
		((Component)_line1).gameObject.SetActive(true);
		((Component)_line2).gameObject.SetActive(true);
	}

	public void ClearTarget()
	{
		_positionType = PositionType.None;
		((Component)_line1).gameObject.SetActive(false);
		((Component)_line2).gameObject.SetActive(false);
	}

	public void SetData(Damage damage)
	{
		_damageWidget.Set(damage);
	}

	private bool GetTargetPos(out Vector2 pos)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		pos = Vector2.zero;
		switch (_positionType)
		{
		case PositionType.None:
			result = false;
			break;
		case PositionType.World:
			pos = Vector2.op_Implicit(MainCamera.WorldToNGUIPos(_targetWorldPosition));
			break;
		case PositionType.UI:
			pos = _targetUIPosition;
			break;
		case PositionType.Transform:
			if ((Object)(object)_targetTransform == (Object)null)
			{
				result = false;
			}
			else
			{
				pos = Vector2.op_Implicit(MainCamera.WorldToNGUIPos(_targetTransform.position));
			}
			break;
		}
		return result;
	}

	private void UpdateLine()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (GetTargetPos(out var pos))
		{
			Vector3 localPosition = ((Component)_damageWidget).transform.localPosition;
			Vector3[] localCorners = _damageWidget.Widget.localCorners;
			Vector2 val = Vector2.op_Implicit(localCorners[_startCorner] + localPosition);
			Vector2 val2 = Vector2.op_Implicit(localCorners[_endCorner] + localPosition);
			Transform transform = ((Component)_line1).transform;
			Vector3 val3 = Vector2.op_Implicit(val2 - val);
			float num = Mathf.Atan2(val3.y, val3.x) * 57.29578f;
			transform.localPosition = Vector2.op_Implicit(val);
			transform.localEulerAngles = Vector3.forward * num;
			_line1.width = (int)((Vector3)(ref val3)).magnitude + _lineLengthModifier;
			Transform transform2 = ((Component)_line2).transform;
			Vector3 val4 = Vector2.op_Implicit(val2 - pos);
			float num2 = Mathf.Atan2(val4.y, val4.x) * 57.29578f;
			transform2.localPosition = Vector2.op_Implicit(pos);
			transform2.localEulerAngles = Vector3.forward * num2;
			_line2.width = (int)((Vector3)(ref val4)).magnitude + _lineLengthModifier;
		}
		else
		{
			ClearTarget();
		}
	}

	public void Begin()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (GetTargetPos(out var pos))
		{
			((MonoBehaviour)this).StartCoroutine(coBegin(pos));
		}
	}

	private IEnumerator coBegin(Vector2 pos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		float finishTime = Time.time + 1f;
		Vector2 labelPos = pos;
		labelPos.x += (float)_damageWidget.Widget.width * Random.Range(-0.8f, 0.8f);
		if (labelPos.y > 200f)
		{
			labelPos.y -= 100f;
		}
		else
		{
			labelPos.y += 100f;
		}
		labelPos.x = Mathf.Clamp(labelPos.x, -300f, 300f);
		labelPos.y = Mathf.Clamp(labelPos.y, -150f, 150f);
		if (labelPos.x > pos.x)
		{
			if (labelPos.y > pos.y)
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
		else if (labelPos.y > pos.y)
		{
			_startCorner = 1;
			_endCorner = 0;
		}
		else
		{
			_startCorner = 1;
			_endCorner = 0;
		}
		((Component)_damageWidget).transform.localPosition = Vector2.op_Implicit(labelPos);
		_damageWidget.ShowAnimation();
		Widget.alpha = 1f;
		float remain = finishTime - Time.time;
		while (remain > 0f)
		{
			remain = finishTime - Time.time;
			UpdateLine();
			if (remain < 0.5f)
			{
				Widget.alpha = remain / 0.5f;
			}
			yield return null;
		}
		if (OnFinished != null)
		{
			OnFinished(this);
		}
	}
}
