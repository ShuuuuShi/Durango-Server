using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MsgPack;
using UnityEngine;

namespace Durango.Development;

public static class ClassToString
{
	private static bool _includeProperty;

	private static bool _includePrimitive;

	private static int _depth;

	public static int MaxDepth = 10;

	private static readonly Type[] NodeTypes = new Type[6]
	{
		typeof(string),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(Point2),
		typeof(Color)
	};

	public static string Get(object obj)
	{
		return Get(obj, includeProperty: false, includePrimitive: false);
	}

	public static string Get(object obj, bool includeProperty, bool includePrimitive)
	{
		_includeProperty = includeProperty;
		_includePrimitive = includePrimitive;
		_depth = 0;
		StringBuilder stringBuilder = new StringBuilder();
		ToString(stringBuilder, obj);
		if (stringBuilder.ToString().Trim().Length == 0)
		{
			return obj.ToString();
		}
		return stringBuilder.ToString().Trim();
	}

	private static void ToString(StringBuilder str, object obj)
	{
		if (str.Length > 0 && str[str.Length - 1] == '\n')
		{
			ToDepth(str, _depth);
		}
		if (obj == null)
		{
			str.Append("null");
			return;
		}
		if (_depth == MaxDepth)
		{
			str.Append(obj);
			return;
		}
		if (obj is IDictionary)
		{
			ToStringForDict(str, (IDictionary)obj);
			return;
		}
		if (obj is IList)
		{
			ToStringForList(str, (IList)obj);
			return;
		}
		if (obj is MessagePackObject messagePackObject)
		{
			if (messagePackObject.IsDictionary)
			{
				ToStringForDict(str, messagePackObject.AsDictionary());
			}
			else if (messagePackObject.IsList || messagePackObject.IsArray)
			{
				ToStringForList(str, new List<MessagePackObject>(messagePackObject.AsList()));
			}
			else
			{
				str.Append(messagePackObject);
			}
			return;
		}
		if (obj is Gauge)
		{
			ToStringGauge(str, (Gauge)obj);
			return;
		}
		if (IsNodeType(obj.GetType()))
		{
			ToStringForNodeValue(str, obj);
			return;
		}
		Type type = obj.GetType();
		if (type.IsGenericType)
		{
			Type genericTypeDefinition = type.GetGenericTypeDefinition();
			if (genericTypeDefinition == typeof(KeyValuePair<, >))
			{
				ToStringForKeyValue(str, obj);
			}
			else if (genericTypeDefinition == typeof(Pair<, >))
			{
				ToStringForKeyValue("Item1", "Item2", str, obj);
			}
		}
		else
		{
			ToStringForClass(str, obj);
		}
	}

	private static void ToStringForDict(StringBuilder str, IDictionary data)
	{
		foreach (object datum in data)
		{
			str.AppendLine();
			ToDepth(str, _depth);
			ToStringForKeyValue(str, datum);
		}
	}

	private static void ToStringForList(StringBuilder str, IList list)
	{
		int count = list.Count;
		if (count == 0)
		{
			str.Append("[]");
			return;
		}
		for (int i = 0; i < count; i++)
		{
			str.AppendLine();
			ToDepth(str, _depth);
			str.Append("- ");
			_depth += 2;
			ToString(str, list[i]);
			_depth -= 2;
		}
	}

	private static void ToStringForClass(StringBuilder str, object obj)
	{
		Type type = obj.GetType();
		FieldInfo[] array = ((!_includePrimitive) ? type.GetFields() : type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		for (int i = 0; i < array.Length; i++)
		{
			if ((array[i].Attributes & FieldAttributes.Static) == 0)
			{
				dictionary.Add(array[i].Name, array[i].GetValue(obj));
			}
		}
		if (_includeProperty)
		{
			PropertyInfo[] properties = type.GetProperties();
			for (int j = 0; j < properties.Length; j++)
			{
				MethodInfo getMethod = properties[j].GetGetMethod();
				if (getMethod != null && (getMethod.Attributes & MethodAttributes.Static) == 0 && getMethod.GetParameters().Length == 0)
				{
					try
					{
						dictionary.Add(properties[j].Name, getMethod.Invoke(obj, null));
					}
					catch (Exception ex)
					{
						dictionary.Add(properties[j].Name, ex.Message);
					}
				}
			}
		}
		str.AppendFormat("({0})", type);
		ToStringForDict(str, dictionary);
	}

	private static void ToStringGauge(StringBuilder str, Gauge gauge)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary.Add("Determination", gauge.Determination);
		if (gauge.MaxGauge == null)
		{
			dictionary.Add("Max", gauge.Max());
		}
		else
		{
			dictionary.Add("Max", gauge.MaxGauge);
		}
		if (gauge.MinGauge == null)
		{
			dictionary.Add("Min", gauge.Min());
		}
		else
		{
			dictionary.Add("Min", gauge.MinGauge);
		}
		ToStringForDict(str, dictionary);
	}

	private static void ToStringForKeyValue(StringBuilder str, object obj)
	{
		ToStringForKeyValue("Key", "Value", str, obj);
	}

	private static void ToStringForKeyValue(string keyName, string valueName, StringBuilder str, object obj)
	{
		object value = WatchDocs.GetValue(keyName, obj);
		object value2 = WatchDocs.GetValue(valueName, obj);
		if (IsNodeType(value))
		{
			str.Append(value).Append(": ");
			_depth++;
			ToString(str, value2);
			_depth--;
			return;
		}
		_depth++;
		str.Append(keyName);
		str.Append(": ");
		ToString(str, value);
		str.AppendLine();
		ToDepth(str, _depth);
		str.Append(valueName);
		str.Append(": ");
		ToString(str, value2);
		_depth--;
	}

	private static void ToStringForNodeValue(StringBuilder str, object obj)
	{
		bool num = obj is string;
		if (num)
		{
			str.Append('\'');
		}
		str.Append(obj);
		if (num)
		{
			str.Append('\'');
		}
	}

	private static void ToDepth(StringBuilder str, int depth)
	{
		for (int i = 0; i < depth; i++)
		{
			str.Append("  ");
		}
	}

	private static bool IsNodeType(object obj)
	{
		if (obj == null)
		{
			return true;
		}
		return IsNodeType((!(obj is MessagePackObject messagePackObject)) ? obj.GetType() : messagePackObject.ToObject().GetType());
	}

	private static bool IsNodeType(Type type)
	{
		if (type.IsPrimitive || type.IsEnum || Array.IndexOf(NodeTypes, type) != -1)
		{
			return true;
		}
		return false;
	}
}
