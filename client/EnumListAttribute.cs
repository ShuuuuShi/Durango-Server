using System;

[AttributeUsage(AttributeTargets.Field)]
public class EnumListAttribute : Attribute
{
	public Type Type;

	public bool AllowEmptyIndex;

	public int BeginIndex;

	public int EndIndex;

	public EnumListAttribute(Type type, bool allowEmptyIndex = false, int beginIndex = 0, int endIndex = -1)
	{
		Type = type;
		AllowEmptyIndex = allowEmptyIndex;
		BeginIndex = beginIndex;
		EndIndex = endIndex;
	}
}
