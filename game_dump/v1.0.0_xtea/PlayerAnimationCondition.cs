using System;

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
		if ((object)typeFromHandle != typeof(TransitionCondition) && (object)typeFromHandle == typeof(StateClipCondition))
		{
			StateClipCondition conditionType = (StateClipCondition)_conditionType;
			switch (conditionType)
			{
			case StateClipCondition.Framework:
				StringValueToEnumArray(Value, typeof(PlayerBehavior.WeaponFramework), out _values);
				break;
			case StateClipCondition.StandState:
				StringValueToEnumArray(Value, typeof(PlayerBehavior.StandStateEnum), out _values);
				break;
			case StateClipCondition.PrevState:
				break;
			case StateClipCondition.IsMoving:
			case StateClipCondition.IsInWater:
			case StateClipCondition.IsSwimming:
			case StateClipCondition.IsWaterCarried:
			case StateClipCondition.IsBushWhack:
			case StateClipCondition.IsTired:
			case StateClipCondition.IsRoadRunning:
			case StateClipCondition.IsRest:
			case StateClipCondition.IsSleep:
			case StateClipCondition.IsNovice:
				StringValueToBoolean(Value, out _values);
				break;
			case StateClipCondition.RunState:
				StringValueToEnumArray(Value, typeof(PlayerBehavior.RunStateEnum), out _values);
				break;
			case StateClipCondition.TargetSize:
				StringValueToEnumArray(Value, typeof(CharacterBehavior.SizeLevel), out _values);
				break;
			case StateClipCondition.Random:
				StringValueToInt(Value, out _values);
				break;
			default:
				Debug.LogError((object)(conditionType.ToString() + "is not properly parsed!"));
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
}
