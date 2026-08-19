using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Durango.Utils;

public static class Reflection
{
	public static IEnumerable<Type> GetAllNamespaceMembers(string @namespace)
	{
		return from t in Assembly.GetExecutingAssembly().GetTypes()
			where t.Namespace == @namespace
			orderby t.Name
			select t;
	}

	public static IEnumerable<Type> GetAllDerivedTypes(Type parent)
	{
		return from t in Assembly.GetExecutingAssembly().GetTypes()
			where t.IsSubclassOf(parent) && !t.IsAbstract
			select t;
	}

	public static IEnumerable<Type> GetAllDerivedGenericTypes(Type parent)
	{
		return from t in Assembly.GetExecutingAssembly().GetTypes()
			where t.IsSubClassOfGeneric(parent) && !t.IsAbstract
			select t;
	}

	public static bool IsSubClassOfGeneric(this Type child, Type parent)
	{
		if (child == parent)
		{
			return false;
		}
		if (child.IsSubclassOf(parent))
		{
			return true;
		}
		Type[] genericArguments = parent.GetGenericArguments();
		bool flag = genericArguments.Length == 0 || (genericArguments[0].Attributes & TypeAttributes.BeforeFieldInit) != TypeAttributes.BeforeFieldInit;
		Type parentFullType = GetFullTypeDefinition(parent);
		while (child != null && child != typeof(object))
		{
			Type fullTypeDefinition = GetFullTypeDefinition(child);
			if (parent == fullTypeDefinition || (flag && fullTypeDefinition.GetInterfaces().Select(GetFullTypeDefinition).Contains(parentFullType)))
			{
				return true;
			}
			if (!flag)
			{
				if (parentFullType == fullTypeDefinition && !fullTypeDefinition.IsInterface)
				{
					if (VerifyGenericArguments(parentFullType, fullTypeDefinition) && VerifyGenericArguments(parent, child))
					{
						return true;
					}
				}
				else if ((from type in child.GetInterfaces()
					where parentFullType == GetFullTypeDefinition(type)
					select type).Any((Type item) => VerifyGenericArguments(parent, item)))
				{
					return true;
				}
			}
			child = child.BaseType;
		}
		return false;
	}

	private static Type GetFullTypeDefinition(Type type)
	{
		if (type.IsGenericType)
		{
			return type.GetGenericTypeDefinition();
		}
		return type;
	}

	private static bool VerifyGenericArguments(Type parent, Type child)
	{
		Type[] genericArguments = child.GetGenericArguments();
		Type[] genericArguments2 = parent.GetGenericArguments();
		if (genericArguments.Length == genericArguments2.Length)
		{
			for (int i = 0; i < genericArguments.Length; i++)
			{
				if ((genericArguments[i].Assembly != genericArguments2[i].Assembly || genericArguments[i].Name != genericArguments2[i].Name || genericArguments[i].Namespace != genericArguments2[i].Namespace) && !genericArguments[i].IsSubclassOf(genericArguments2[i]))
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void Invoke(Type type, string methodName)
	{
		while (type != null)
		{
			MethodInfo method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.Public);
			if (method == null)
			{
				method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
			}
			if (method != null)
			{
				method.Invoke(null, null);
				break;
			}
			type = type.BaseType;
		}
	}
}
