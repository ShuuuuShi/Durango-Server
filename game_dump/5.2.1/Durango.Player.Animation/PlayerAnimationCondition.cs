using System;
using System.Linq;
using UnityEngine;

namespace Durango.Player.Animation;

public class PlayerAnimationCondition
{
	private int _conditionType;

	private int[] _values;

	public string Type { get; set; }

	public string Value { get; set; }

	public PlayerAnimationCondition(string type, string value)
	{
		Type = type;
		Value = value;
	}

	public int[] GetValues()
	{
		return _values;
	}

	public int GetConditionType()
	{
		return _conditionType;
	}

	public void Init<T>()
	{
		Type typeFromHandle = typeof(T);
		try
		{
			_conditionType = (int)Enum.Parse(typeFromHandle, Type);
		}
		catch (ArgumentException)
		{
			_conditionType = 0;
		}
		if (typeFromHandle != typeof(TransitionCondition) && typeFromHandle == typeof(StateClipCondition))
		{
			StateClipCondition conditionType = (StateClipCondition)_conditionType;
			switch (conditionType)
			{
			case StateClipCondition.Framework:
				StringValueToEnumArray(Value, typeof(PlayerBehavior.WeaponFramework), out _values);
				break;
			case StateClipCondition.StandState:
				StringValueToEnumArray(Value, typeof(LocalMotionUpdater.StandStateEnum), out _values);
				break;
			case StateClipCondition.IsMoving:
			case StateClipCondition.IsInWater:
			case StateClipCondition.IsSwimming:
			case StateClipCondition.IsWaterCarried:
			case StateClipCondition.IsBushWhack:
			case StateClipCondition.IsTired:
			case StateClipCondition.IsRoadRunning:
			case StateClipCondition.IsSleep:
			case StateClipCondition.IsNovice:
				StringValueToBoolean(Value, out _values);
				break;
			case StateClipCondition.MoveSpeed:
			case StateClipCondition.Random:
				StringValueToInt(Value, out _values);
				break;
			default:
				Debug.LogError(string.Concat(conditionType, "is not properly parsed!"));
				StringValueToBoolean(Value, out _values);
				break;
			}
		}
	}

	private static void StringValueToEnumArray(string str, Type enumType, out int[] result)
	{
		string[] array = str.Split('|');
		result = new int[array.Length];
		int i = 0;
		for (int num = result.Length; i < num; i++)
		{
			try
			{
				result[i] = (int)Enum.Parse(enumType, array[i], ignoreCase: true);
			}
			catch (ArgumentException)
			{
				result[i] = -1;
			}
		}
	}

	private static void StringValueToBoolean(string str, out int[] result)
	{
		result = new int[1];
		if (bool.TryParse(str, out var result2))
		{
			result[0] = (result2 ? 1 : 0);
		}
	}

	private static void StringValueToInt(string str, out int[] result)
	{
		result = new int[1];
		if (int.TryParse(str, out var result2))
		{
			result[0] = result2;
		}
	}

	public float GetContionValue(PlayerAnimationConditionArguments arguments)
	{
		float result = 0f;
		int[] values = GetValues();
		switch ((StateClipCondition)GetConditionType())
		{
		case StateClipCondition.Framework:
			result = CheckCondition(values, arguments.Framework);
			break;
		case StateClipCondition.StandState:
			result = CheckCondition(values, arguments.StandState);
			break;
		case StateClipCondition.IsInWater:
			result = CheckCondition(values, arguments.IsInWater);
			break;
		case StateClipCondition.IsMoving:
			result = CheckCondition(values, arguments.IsMoving);
			break;
		case StateClipCondition.IsSwimming:
			result = CheckCondition(values, arguments.IsSwimming);
			break;
		case StateClipCondition.IsWaterCarried:
			result = CheckCondition(values, arguments.IsWaterCarried);
			break;
		case StateClipCondition.IsBushWhack:
			result = CheckCondition(values, arguments.IsBushWhack);
			break;
		case StateClipCondition.IsTired:
			result = CheckCondition(values, arguments.IsTired);
			break;
		case StateClipCondition.IsRoadRunning:
			result = CheckCondition(values, arguments.IsRoadRunning);
			break;
		case StateClipCondition.IsSleep:
			result = CheckCondition(values, arguments.IsSleep);
			break;
		case StateClipCondition.IsNovice:
			result = CheckCondition(values, arguments.IsNovice);
			break;
		case StateClipCondition.MoveSpeed:
			result = CheckMoveSpeedCondition(values, arguments.MoveSpeed);
			break;
		case StateClipCondition.Random:
			result = ((values != null && values.Length != 0) ? (UnityEngine.Random.value * (float)values[0]) : (-1f));
			break;
		}
		return result;
	}

	private static float CheckCondition(int[] condition, bool current)
	{
		if (condition == null || condition.Length == 0)
		{
			return -1f;
		}
		if (condition[0] != 0 == current)
		{
			return 1f;
		}
		return -1f;
	}

	private static float CheckCondition(int[] condition, int? value)
	{
		if (!value.HasValue)
		{
			return 0f;
		}
		if (condition == null)
		{
			return -1f;
		}
		if (Array.IndexOf(condition, value) == -1)
		{
			return -1f;
		}
		return 1f;
	}

	private static float CheckMoveSpeedCondition(int[] condition, int? value)
	{
		if (!value.HasValue)
		{
			return 0f;
		}
		if (KUtility.GetSize(condition) == 0)
		{
			return -1f;
		}
		int num = condition.First();
		int num2 = Math.Abs(value.Value - num);
		if ((float)num2 > 500f)
		{
			return -1f;
		}
		return 1f - (float)num2 / 500f;
	}
}
