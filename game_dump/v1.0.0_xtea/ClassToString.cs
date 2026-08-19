using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using MsgPack;
using UnityEngine;

public static class ClassToString
{
	private static bool _includeProperty;

	private static bool _includePrimitive;

	private static int _depth;

	public static int MaxDepth = 10;

	private static readonly Type[] NodeTypes = new Type[7]
	{
		typeof(string),
		typeof(Vector2),
		typeof(Vector3),
		typeof(Vector4),
		typeof(Point2),
		typeof(Color),
		typeof(Gauge)
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
		string text = stringBuilder.ToString().Trim();
		if (text.Length == 0)
		{
			return obj.ToString();
		}
		return stringBuilder.ToString().Trim();
	}

	private static void ToString(StringBuilder str, object obj)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		if (str.Length > 0 && str[str.Length - 1] == '\n')
		{
			ToDepth(str, _depth);
		}
		if (obj == null)
		{
			str.Append("null");
		}
		else if (_depth == MaxDepth)
		{
			str.Append(obj);
		}
		else if (obj is IDictionary)
		{
			ToStringForDict(str, (IDictionary)obj);
		}
		else if (obj is IList)
		{
			ToStringForList(str, (IList)obj);
		}
		else if (obj is MessagePackObject val)
		{
			if (((MessagePackObject)(ref val)).IsDictionary)
			{
				ToStringForDict(str, (IDictionary)((MessagePackObject)(ref val)).AsDictionary());
			}
			else if (((MessagePackObject)(ref val)).IsList || ((MessagePackObject)(ref val)).IsArray)
			{
				ToStringForList(str, new List<MessagePackObject>(((MessagePackObject)(ref val)).AsList()));
			}
			else
			{
				str.Append(val);
			}
		}
		else if (IsNodeType(obj.GetType()))
		{
			ToStringForNodeValue(str, obj);
		}
		else
		{
			Type type = obj.GetType();
			if (type.IsGenericType && (object)type.GetGenericTypeDefinition() == typeof(KeyValuePair<, >))
			{
				ToStringForKeyValue(str, obj);
			}
			else
			{
				ToStringForClass(str, obj);
			}
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
			FieldInfo fieldInfo = array[i];
			if ((fieldInfo.Attributes & FieldAttributes.Static) == 0)
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
				if ((object)getMethod != null && (getMethod.Attributes & MethodAttributes.Static) == 0 && getMethod.GetParameters().Length <= 0)
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

	private static void ToStringForKeyValue(StringBuilder str, object obj)
	{
		object value = WatchDocs.GetValue("Key", obj);
		object value2 = WatchDocs.GetValue("Value", obj);
		if (IsNodeType(value))
		{
			str.Append(value).Append(": ");
			_depth++;
			ToString(str, value2);
			_depth--;
			return;
		}
		_depth++;
		str.Append("Key: ");
		ToString(str, value);
		str.AppendLine();
		ToDepth(str, _depth);
		str.Append("Value: ");
		ToString(str, value2);
		_depth--;
	}

	private static void ToStringForNodeValue(StringBuilder str, object obj)
	{
		bool flag = obj is string;
		if (flag)
		{
			str.Append('\'');
		}
		str.Append(obj);
		if (flag)
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
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		if (obj == null)
		{
			return true;
		}
		Type type = null;
		type = ((!(obj is MessagePackObject val)) ? obj.GetType() : ((MessagePackObject)(ref val)).ToObject().GetType());
		return IsNodeType(type);
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
