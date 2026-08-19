using System;

[AttributeUsage(AttributeTargets.Field)]
public class EnumListAttribute : Attribute
{
	public Type Type;

	public bool AllowEmptyIndex;

	public int MaxCount;

	public EnumListAttribute(Type type, bool allowEmptyIndex = false, int maxCount = -1)
	{
		Type = type;
		AllowEmptyIndex = allowEmptyIndex;
		MaxCount = maxCount;
	}
}
