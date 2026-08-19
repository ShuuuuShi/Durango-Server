using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Durango.Development;

public class WatchDocs : MonoBehaviour
{
	public class ResultSet
	{
		public string key;

		public bool isValid;

		public object value;

		public string failkey;
	}

	private Component _suspect;

	private List<ResultSet> _watchingParts = new List<ResultSet>();

	private ResultSet _processResult = new ResultSet();

	public Component Suspect
	{
		get
		{
			return _suspect;
		}
		set
		{
			if (_suspect != value)
			{
				_suspect = value;
				Reset();
			}
		}
	}

	public List<ResultSet> WatchingParts => _watchingParts;

	public string processValue { get; set; }

	public ResultSet ProcessResult => _processResult;

	public static object GetValue(string key, object obj)
	{
		TryGetValue(key, obj, out var value);
		return value;
	}

	public static bool TryGetValue(string str, object obj, out object value)
	{
		string failString;
		return TryGetValue(str, obj, out value, out failString, obj);
	}

	public static bool TryGetValue(string str, object obj, out object value, out string failString)
	{
		return TryGetValue(str, obj, out value, out failString, obj);
	}

	private static bool TryGetValue(string str, object obj, out object value, out string failString, object root)
	{
		value = obj;
		failString = str;
		if (string.IsNullOrEmpty(str) || obj == null)
		{
			return false;
		}
		string[] array = SplitToken(str);
		if (array == null)
		{
			return false;
		}
		if (root == null)
		{
			root = obj;
		}
		if (array.Length >= 3)
		{
			return TryGetFunction(array, obj, out value, out failString, root);
		}
		return TryGetField(array, obj, out value, out failString, root);
	}

	private static bool TryGetField(string[] arg, object obj, out object value, out string failString, object root)
	{
		value = obj;
		failString = null;
		if (arg == null || obj == null)
		{
			return false;
		}
		bool result = arg.Length != 2 || arg[1] != null;
		int length = arg[0].Length;
		if (length > 2 && ((arg[0][0] == '\'' && arg[0][length - 1] == '\'') || (arg[0][0] == '"' && arg[0][length - 1] == '"')))
		{
			value = arg[0].Substring(1, length - 2);
			return result;
		}
		if (int.TryParse(arg[0], out var result2))
		{
			value = result2;
			return result;
		}
		if (float.TryParse(arg[0], out var result3))
		{
			value = result3;
			return result;
		}
		MemberInfo[] member = obj.GetType().GetMember(arg[0], MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (member.Length == 0)
		{
			failString = arg[0];
			return false;
		}
		object obj2 = null;
		if (member[0] is FieldInfo)
		{
			obj2 = (member[0] as FieldInfo).GetValue(obj);
		}
		else if (member[0] is PropertyInfo)
		{
			obj2 = (member[0] as PropertyInfo).GetValue(obj, null);
		}
		if (arg.Length == 1 || arg[1] == null)
		{
			value = obj2;
			return result;
		}
		return TryGetValue(arg[1], obj2, out value, out failString, root);
	}

	private static bool TryGetFunction(string[] arg, object obj, out object value, out string failString, object root)
	{
		value = obj;
		failString = null;
		if (arg == null || obj == null)
		{
			return false;
		}
		int num = 0;
		string[] array = null;
		if (!string.IsNullOrEmpty(arg[1]))
		{
			arg[1] = arg[1].Trim();
			if (arg[1][0] == '[')
			{
				if (arg[1][arg[1].Length - 1] == ']')
				{
					arg[1] = arg[1].Substring(1, arg[1].Length - 2);
					if (TryGetValue(arg[0], obj, out var value2, out failString, root))
					{
						string text = ((!(value2 is Array)) ? "get_Item" : "GetValue");
						return TryGetFunction(new string[3]
						{
							text,
							arg[1],
							arg[2]
						}, value2, out value, out failString, root);
					}
					failString = arg[0];
					return false;
				}
				failString = arg[1];
				return false;
			}
			array = arg[1].Replace(" ", string.Empty).Split(',');
			num = array.Length;
		}
		object[] array2 = new object[num];
		Type[] array3 = new Type[num];
		for (int i = 0; i < array2.Length; i++)
		{
			if (!TryGetValue(array[i], root, out array2[i], out failString, root))
			{
				failString = array[i];
				return false;
			}
			if (array2[i] == null)
			{
				array3[i] = typeof(Nullable);
			}
			else
			{
				array3[i] = array2[i].GetType();
			}
		}
		MethodInfo method = obj.GetType().GetMethod(arg[0], BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, array3, null);
		if (method == null)
		{
			failString = arg[0];
			return false;
		}
		object obj2 = method.Invoke(obj, array2);
		if (string.IsNullOrEmpty(arg[2]))
		{
			value = obj2;
			return true;
		}
		return TryGetValue(arg[2], obj2, out value, out failString, root);
	}

	private static string[] SplitToken(string str)
	{
		if (string.IsNullOrEmpty(str))
		{
			return null;
		}
		string[] array = null;
		int num = -1;
		int length = str.Length;
		for (int i = 0; i < length - 1; i++)
		{
			if (str[i] == '.' && !char.IsDigit(str[i + 1]))
			{
				array = new string[2];
				num = i;
				break;
			}
			if (str[i] == '(' || str[i] == '[')
			{
				array = new string[3];
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			array = ((length <= 0 || str[length - 1] != '.') ? new string[1] { str } : new string[2]
			{
				str.Substring(0, str.Length - 1),
				null
			});
		}
		else
		{
			array[0] = str.Substring(0, num);
			if (array.Length == 2)
			{
				array[1] = str.Substring(num + 1);
			}
			else
			{
				bool flag = false;
				for (int j = num + 1; j < str.Length; j++)
				{
					if (str[num] == '(' && str[j] == ')')
					{
						array[1] = str.Substring(num + 1, j - num - 1);
						flag = true;
					}
					else if (str[num] == '[' && str[j] == ']')
					{
						array[1] = str.Substring(num, j - num + 1);
						flag = true;
					}
					if (flag)
					{
						if (j + 2 < str.Length && str[j + 1] == '.')
						{
							array[2] = str.Substring(j + 2);
						}
						break;
					}
				}
				if (!flag)
				{
					array = null;
				}
			}
		}
		return array;
	}

	public static List<string> GetAvilableList(object obj, string filter = null)
	{
		if (obj == null)
		{
			return null;
		}
		MemberInfo[] members = obj.GetType().GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		List<string> list = new List<string>();
		for (int i = 0; i < members.Length; i++)
		{
			if (string.IsNullOrEmpty(filter) || members[i].Name.Contains(filter))
			{
				if (members[i] is MethodInfo)
				{
					list.Add(members[i].ToString());
				}
				else
				{
					list.Add(members[i].Name);
				}
			}
		}
		return list;
	}

	private void Reset()
	{
		ProcessResult.key = null;
		WatchingParts.Clear();
		Calc();
	}

	public void Calc()
	{
		int count = WatchingParts.Count;
		for (int i = 0; i < count; i++)
		{
			ResultSet resultSet = WatchingParts[i];
			resultSet.isValid = TryGetValue(resultSet.key, Suspect, out resultSet.value, out resultSet.failkey, Suspect);
		}
		if (WatchingParts.Count == 0 || !string.IsNullOrEmpty(WatchingParts[count - 1].key))
		{
			WatchingParts.Add(new ResultSet());
		}
		else if (WatchingParts.Count >= 2 && string.IsNullOrEmpty(WatchingParts[count - 1].key) && string.IsNullOrEmpty(WatchingParts[count - 2].key))
		{
			WatchingParts.RemoveAt(count - 1);
		}
		for (int num = WatchingParts.Count - 2; num >= 0; num--)
		{
			if (string.IsNullOrEmpty(WatchingParts[num].key))
			{
				WatchingParts.RemoveAt(num);
			}
		}
	}
}
