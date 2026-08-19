using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
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

	[CompilerGenerated]
	private sealed class _003CCollectUri_003Ed__5 : IEnumerable<string>, IEnumerable, IEnumerator<string>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private string _003C_003E2__current;

		private int _003C_003El__initialThreadId;

		public UriMethods _003C_003E4__this;

		private int _003Ci_003E5__2;

		string IEnumerator<string>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCollectUri_003Ed__5(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
			_003C_003El__initialThreadId = Thread.CurrentThread.ManagedThreadId;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			UriMethods uriMethods = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				_003Ci_003E5__2 = 0;
				break;
			case 1:
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
				break;
			}
			if (_003Ci_003E5__2 < uriMethods._methods.Count)
			{
				UriMethod uriMethod = uriMethods._methods[_003Ci_003E5__2];
				string text = ((KUtility.GetSize(uriMethod.Tokens) != 0) ? string.Join("/", uriMethod.Tokens) : string.Empty);
				if (uriMethod.ParamCount > 0)
				{
					ParameterInfo[] parameters = uriMethod.Method.GetParameters();
					foreach (ParameterInfo parameterInfo in parameters)
					{
						string text2 = "{" + parameterInfo.Name + "}";
						if (text.Length > 0)
						{
							text += "/";
						}
						text += text2;
					}
				}
				_003C_003E2__current = text;
				_003C_003E1__state = 1;
				return true;
			}
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}

		[DebuggerHidden]
		IEnumerator<string> IEnumerable<string>.GetEnumerator()
		{
			_003CCollectUri_003Ed__5 result;
			if (_003C_003E1__state == -2 && _003C_003El__initialThreadId == Thread.CurrentThread.ManagedThreadId)
			{
				_003C_003E1__state = 0;
				result = this;
			}
			else
			{
				result = new _003CCollectUri_003Ed__5(0)
				{
					_003C_003E4__this = _003C_003E4__this
				};
			}
			return result;
		}

		[DebuggerHidden]
		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable<string>)this).GetEnumerator();
		}
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
			foreach (MethodInfo methodInfo in methods)
			{
				object[] customAttributes = methodInfo.GetCustomAttributes(typeof(UriAttribute), inherit: false);
				if (customAttributes.Length == 0)
				{
					continue;
				}
				object[] array = customAttributes;
				foreach (object obj in array)
				{
					UriAttribute uriAttribute = (UriAttribute)obj;
					string[] tokens = ((!string.IsNullOrEmpty(uriAttribute.Key)) ? uriAttribute.Key.Split(UriParser.Separator, StringSplitOptions.RemoveEmptyEntries) : null);
					int paramCount = KUtility.GetSize(methodInfo.GetParameters());
					if (_methods.FindIndex(delegate(UriMethod o)
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
					}) == -1)
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
			int num = KUtility.GetSize(m2.Tokens) - KUtility.GetSize(m1.Tokens);
			if (num == 0)
			{
				num = m2.ParamCount - m1.ParamCount;
			}
			return num;
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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCollectUri_003Ed__5(-2)
		{
			_003C_003E4__this = this
		};
	}
}
