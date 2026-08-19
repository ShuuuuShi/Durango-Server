using System;

public class EnumTypeAttribute : Attribute
{
	public Type EnumType { get; private set; }

	public EnumTypeAttribute(Type type)
	{
		if (type.IsEnum)
		{
			EnumType = type;
		}
	}
}
