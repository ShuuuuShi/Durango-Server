using System;
using Durango.Render.Camera;
using JetBrains.Annotations;
using Shared.Battle;
using UnityEngine;

namespace Durango.UI.InGame;

public class DamageLabelIndicator : MonoBehaviour
{
	private const int InitState = 0;

	private const int InvalidState = -1;

	public Action<DamageLabelIndicator> Finished;

	[SerializeField]
	private UILabel _damageLabel;

	[SerializeField]
	private float _popPower;

	[SerializeField]
	private float _gravity;

	[SerializeField]
	private float _throwRatio;

	[SerializeField]
	private float[] _stateTime;

	private int _state = -1;

	private Vector3 _targetPos;

	private Vector3 _dir;

	private float _timer;

	private int _stateCount;

	public void Begin(string damage, Color color, [NotNull] DamageableEntity victim, [CanBeNull] DamageableEntity attacker, BodyPart bodyPart)
	{
		if (_stateCount == 0)
		{
			_stateCount = _stateTime.Length;
		}
		_damageLabel.text = damage;
		_damageLabel.color = color;
		_damageLabel.alpha = 0f;
		_timer = 0f;
		_state = 0;
		CalcPosition(victim, bodyPart);
		CalcDirection(attacker);
	}

	private void CalcPosition([NotNull] DamageableEntity victim, BodyPart bodyPart)
	{
		Transform bodyPartTransform = victim.GetBodyPartTransform(bodyPart);
		_targetPos = bodyPartTransform.position;
		base.transform.localPosition = MainCamera.WorldToNGUIPos(_targetPos);
	}

	private void CalcDirection([CanBeNull] DamageableEntity attacker)
	{
		if (attacker == null)
		{
			float f = UnityEngine.Random.value * (float)Math.PI * 2f;
			_dir = new Vector3(Mathf.Cos(f), 0f, Mathf.Sin(f));
		}
		else
		{
			_dir = _targetPos - attacker.GetCurrentPosition();
		}
		_dir.y = 0f;
		_dir.Normalize();
		_dir.y = _throwRatio * Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value);
		_dir *= _popPower * Mathf.Lerp(0.9f, 1.1f, UnityEngine.Random.value);
	}

	private void Update()
	{
		if (_state < 0 || _state > _stateCount)
		{
			return;
		}
		float num = _stateTime[_state];
		float num2 = Mathf.Sqrt(_timer / num);
		float num3 = 1f;
		switch (_state)
		{
		case 0:
			num3 = 0.5f + 0.5f * num2;
			_damageLabel.alpha = num2;
			break;
		case 2:
			_damageLabel.alpha = 1f - num2;
			break;
		}
		Transform obj = base.transform;
		_targetPos += _dir * Time.deltaTime;
		obj.localPosition = MainCamera.WorldToNGUIPos(_targetPos);
		_dir.y -= _gravity * Time.deltaTime;
		obj.localScale = Vector3.one * num3;
		_timer += Time.deltaTime;
		if (!(_timer > num))
		{
			return;
		}
		_timer = 0f;
		_state++;
		if (_state == _stateCount)
		{
			_state = -1;
			if (Finished != null)
			{
				Finished(this);
			}
		}
	}
}
