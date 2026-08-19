using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Durango.Utils;

public static class Enums<T> where T : struct, IComparable, IFormattable, IConvertible
{
	[StructLayout(LayoutKind.Explicit)]
	private struct EnumUnion32
	{
		[FieldOffset(0)]
		public T Enum;

		[FieldOffset(0)]
		public int Int;
	}

	private static T[] _values;

	private static T[] _greaterValues;

	private static int _greaterInt;

	private static T? _maxValue;

	public static int ToInt(T e)
	{
		EnumUnion32 enumUnion = default(EnumUnion32);
		enumUnion.Enum = e;
		return enumUnion.Int;
	}

	public static T ToEnum(int value)
	{
		EnumUnion32 enumUnion = default(EnumUnion32);
		enumUnion.Int = value;
		return enumUnion.Enum;
	}

	public static T[] All()
	{
		if (_values != null)
		{
			return _values;
		}
		Type typeFromHandle = typeof(T);
		Array values = Enum.GetValues(typeFromHandle);
		_values = (T[])values;
		return _values;
	}

	public static T[] Greater(T greater)
	{
		int num = ToInt(greater);
		if (_greaterValues != null && _greaterInt == num)
		{
			return _greaterValues;
		}
		_greaterInt = num;
		Type typeFromHandle = typeof(T);
		IEnumerable<T> source = from T x in Enum.GetValues(typeFromHandle)
			where ToInt(x) > _greaterInt
			select x;
		_greaterValues = source.ToArray();
		return _greaterValues;
	}

	public static T Max()
	{
		if (_maxValue.HasValue)
		{
			return _maxValue.Value;
		}
		T[] array = All();
		int? num = null;
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = ToInt(array[i]);
			if (!num.HasValue)
			{
				num = num2;
			}
			else if (num.Value < num2)
			{
				num = num2;
			}
		}
		if (!num.HasValue)
		{
			return ToEnum(0);
		}
		_maxValue = ToEnum(num.Value);
		return _maxValue.Value;
	}
}
