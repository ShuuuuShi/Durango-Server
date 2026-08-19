using System;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;

[Serializable]
public class PropertyReference
{
	[SerializeField]
	private Component mTarget;

	[SerializeField]
	private string mName;

	private FieldInfo mField;

	private PropertyInfo mProperty;

	private static int s_Hash = "PropertyBinding".GetHashCode();

	public Component target
	{
		get
		{
			return mTarget;
		}
		set
		{
			mTarget = value;
			mProperty = null;
			mField = null;
		}
	}

	public string name
	{
		get
		{
			return mName;
		}
		set
		{
			mName = value;
			mProperty = null;
			mField = null;
		}
	}

	public bool isValid => (Object)(object)mTarget != (Object)null && !string.IsNullOrEmpty(mName);

	public bool isEnabled
	{
		get
		{
			if ((Object)(object)mTarget == (Object)null)
			{
				return false;
			}
			Component obj = mTarget;
			MonoBehaviour val = (MonoBehaviour)(object)((obj is MonoBehaviour) ? obj : null);
			return (Object)(object)val == (Object)null || ((Behaviour)val).enabled;
		}
	}

	public PropertyReference()
	{
	}

	public PropertyReference(Component target, string fieldName)
	{
		mTarget = target;
		mName = fieldName;
	}

	public Type GetPropertyType()
	{
		if ((object)mProperty == null && (object)mField == null && isValid)
		{
			Cache();
		}
		if ((object)mProperty != null)
		{
			return mProperty.PropertyType;
		}
		if ((object)mField != null)
		{
			return mField.FieldType;
		}
		return typeof(void);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return !isValid;
		}
		if (obj is PropertyReference)
		{
			PropertyReference propertyReference = obj as PropertyReference;
			return (Object)(object)mTarget == (Object)(object)propertyReference.mTarget && string.Equals(mName, propertyReference.mName);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return s_Hash;
	}

	public void Set(Component target, string methodName)
	{
		mTarget = target;
		mName = methodName;
	}

	public void Clear()
	{
		mTarget = null;
		mName = null;
	}

	public void Reset()
	{
		mField = null;
		mProperty = null;
	}

	public override string ToString()
	{
		return ToString(mTarget, name);
	}

	public static string ToString(Component comp, string property)
	{
		if ((Object)(object)comp != (Object)null)
		{
			string text = ((object)comp).GetType().ToString();
			int num = text.LastIndexOf('.');
			if (num > 0)
			{
				text = text.Substring(num + 1);
			}
			if (!string.IsNullOrEmpty(property))
			{
				return text + "." + property;
			}
			return text + ".[property]";
		}
		return null;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public object Get()
	{
		if ((object)mProperty == null && (object)mField == null && isValid)
		{
			Cache();
		}
		if ((object)mProperty != null)
		{
			if (mProperty.CanRead)
			{
				return mProperty.GetValue(mTarget, null);
			}
		}
		else if ((object)mField != null)
		{
			return mField.GetValue(mTarget);
		}
		return null;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public bool Set(object value)
	{
		if ((object)mProperty == null && (object)mField == null && isValid)
		{
			Cache();
		}
		if ((object)mProperty == null && (object)mField == null)
		{
			return false;
		}
		if (value == null)
		{
			try
			{
				if ((object)mProperty == null)
				{
					mField.SetValue(mTarget, null);
					return true;
				}
				if (mProperty.CanWrite)
				{
					mProperty.SetValue(mTarget, null, null);
					return true;
				}
			}
			catch (Exception)
			{
				return false;
			}
		}
		if (!Convert(ref value))
		{
			if (Application.isPlaying)
			{
				Debug.LogError((object)string.Concat("Unable to convert ", value.GetType(), " to ", GetPropertyType()));
			}
		}
		else
		{
			if ((object)mField != null)
			{
				mField.SetValue(mTarget, value);
				return true;
			}
			if (mProperty.CanWrite)
			{
				mProperty.SetValue(mTarget, value, null);
				return true;
			}
		}
		return false;
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	private bool Cache()
	{
		if ((Object)(object)mTarget != (Object)null && !string.IsNullOrEmpty(mName))
		{
			Type type = ((object)mTarget).GetType();
			mField = type.GetField(mName);
			mProperty = type.GetProperty(mName);
		}
		else
		{
			mField = null;
			mProperty = null;
		}
		return (object)mField != null || (object)mProperty != null;
	}

	private bool Convert(ref object value)
	{
		if ((Object)(object)mTarget == (Object)null)
		{
			return false;
		}
		Type propertyType = GetPropertyType();
		Type from;
		if (value == null)
		{
			if (!propertyType.IsClass)
			{
				return false;
			}
			from = propertyType;
		}
		else
		{
			from = value.GetType();
		}
		return Convert(ref value, from, propertyType);
	}

	public static bool Convert(Type from, Type to)
	{
		object value = null;
		return Convert(ref value, from, to);
	}

	public static bool Convert(object value, Type to)
	{
		if (value == null)
		{
			value = null;
			return Convert(ref value, to, to);
		}
		return Convert(ref value, value.GetType(), to);
	}

	public static bool Convert(ref object value, Type from, Type to)
	{
		if (to.IsAssignableFrom(from))
		{
			return true;
		}
		if ((object)to == typeof(string))
		{
			value = ((value == null) ? "null" : value.ToString());
			return true;
		}
		if (value == null)
		{
			return false;
		}
		float result2;
		if ((object)to == typeof(int))
		{
			if ((object)from == typeof(string))
			{
				if (int.TryParse((string)value, out var result))
				{
					value = result;
					return true;
				}
			}
			else if ((object)from == typeof(float))
			{
				value = Mathf.RoundToInt((float)value);
				return true;
			}
		}
		else if ((object)to == typeof(float) && (object)from == typeof(string) && float.TryParse((string)value, out result2))
		{
			value = result2;
			return true;
		}
		return false;
	}
}
