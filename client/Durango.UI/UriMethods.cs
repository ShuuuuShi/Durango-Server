using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Durango.UI;

public class UriMethods : IUriInvokable
{
	private struct UriMethod
	{
		public string[] Tokens;

		public MethodInfo Method;

		public int ParamCount;
	}

	private readonly List<UriMethod> _methods = new List<UriMethod>();

	private readonly object _parent;

	public UriMethods([NotNull] object parent)
	{
		_parent = parent;
		Type type = parent.GetType();
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		while (type != null)
		{
			MethodInfo[] methods = type.GetMethods(bindingFlags);
			MethodInfo[] array = methods;
			foreach (MethodInfo methodInfo in array)
			{
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(UriAttribute), inherit: false);
				if (customAttributes.Length == 0)
				{
					continue;
				}
				object[] array2 = customAttributes;
				foreach (object obj in array2)
				{
					UriAttribute uriAttribute = (UriAttribute)obj;
					string[] tokens = ((!string.IsNullOrEmpty(uriAttribute.Key)) ? uriAttribute.Key.Split(UriParser.Separator, StringSplitOptions.RemoveEmptyEntries) : null);
					int paramCount = KUtility.GetSize(methodInfo.GetParameters());
					int num = _methods.FindIndex(delegate(UriMethod o)
					{
						if (o.ParamCount != paramCount)
						{
							return false;
						}
						if (KUtility.GetSize(tokens) != KUtility.GetSize(o.Tokens))
						{
							return false;
						}
						return (tokens == null || tokens.SequenceEqual(o.Tokens)) ? true : false;
					});
					if (num == -1)
					{
						_methods.Add(new UriMethod
						{
							Tokens = tokens,
							Method = methodInfo,
							ParamCount = paramCount
						});
					}
				}
			}
			type = type.BaseType;
			bindingFlags &= ~BindingFlags.Public;
		}
		_methods.Sort(delegate(UriMethod m1, UriMethod m2)
		{
			int num2 = KUtility.GetSize(m2.Tokens) - KUtility.GetSize(m1.Tokens);
			if (num2 == 0)
			{
				num2 = m2.ParamCount - m1.ParamCount;
			}
			return num2;
		});
	}

	public int InvokeUri(string[] tokens, int start)
	{
		int num = KUtility.GetSize(tokens) - start;
		int num2 = -1;
		for (int i = 0; i < _methods.Count; i++)
		{
			UriMethod uriMethod = _methods[i];
			int size = KUtility.GetSize(uriMethod.Tokens);
			if (size + uriMethod.ParamCount != num)
			{
				continue;
			}
			bool flag = true;
			for (int j = 0; j < size; j++)
			{
				if (!uriMethod.Tokens[j].Equals(tokens[start + j], StringComparison.OrdinalIgnoreCase))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				num2 = i;
				break;
			}
		}
		if (num2 == -1)
		{
			return 0;
		}
		UriMethod uriMethod2 = _methods[num2];
		object[] array = new object[uriMethod2.ParamCount];
		int size2 = KUtility.GetSize(uriMethod2.Tokens);
		for (int k = 0; k < uriMethod2.ParamCount; k++)
		{
			array[k] = tokens[start + size2 + k];
		}
		uriMethod2.Method.Invoke(_parent, array);
		return size2 + uriMethod2.ParamCount;
	}

	public IEnumerable<string> CollectUri()
	{
		for (int i = 0; i < _methods.Count; i++)
		{
			UriMethod j = _methods[i];
			string value = ((KUtility.GetSize(j.Tokens) != 0) ? string.Join("/", j.Tokens) : string.Empty);
			if (j.ParamCount > 0)
			{
				ParameterInfo[] parameters = j.Method.GetParameters();
				ParameterInfo[] array = parameters;
				foreach (ParameterInfo parameterInfo in array)
				{
					string text = "{" + parameterInfo.Name + "}";
					if (value.Length > 0)
					{
						value += "/";
					}
					value += text;
				}
			}
			yield return value;
		}
	}
}
