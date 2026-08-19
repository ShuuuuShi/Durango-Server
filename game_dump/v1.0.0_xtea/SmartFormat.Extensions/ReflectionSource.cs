using System;
using System.Reflection;
using SmartFormat.Core.Extensions;

namespace SmartFormat.Extensions;

public class ReflectionSource : ISource
{
	public ReflectionSource(SmartFormatter formatter)
	{
		formatter.Parser.AddAlphanumericSelectors();
		formatter.Parser.AddAdditionalSelectorChars("_");
		formatter.Parser.AddOperators(".");
	}

	public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
	{
		object currentValue = selectorInfo.CurrentValue;
		string selectorText = selectorInfo.SelectorText;
		if (currentValue == null)
		{
			return false;
		}
		Type type = currentValue.GetType();
		BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
		bindingFlags |= selectorInfo.FormatDetails.Settings.GetCaseSensitivityBindingFlag();
		MemberInfo[] member = type.GetMember(selectorText, bindingFlags);
		MemberInfo[] array = member;
		foreach (MemberInfo memberInfo in array)
		{
			switch (memberInfo.MemberType)
			{
			case MemberTypes.Field:
			{
				FieldInfo fieldInfo = (FieldInfo)memberInfo;
				selectorInfo.Result = fieldInfo.GetValue(currentValue);
				return true;
			}
			case MemberTypes.Method:
			case MemberTypes.Property:
			{
				MethodInfo methodInfo;
				if (memberInfo.MemberType == MemberTypes.Property)
				{
					PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
					if (!propertyInfo.CanRead)
					{
						break;
					}
					methodInfo = propertyInfo.GetGetMethod();
				}
				else
				{
					methodInfo = (MethodInfo)memberInfo;
				}
				if (methodInfo.GetParameters().Length > 0 || (object)methodInfo.ReturnType == typeof(void))
				{
					break;
				}
				selectorInfo.Result = methodInfo.Invoke(currentValue, new object[0]);
				return true;
			}
			}
		}
		return false;
	}
}
