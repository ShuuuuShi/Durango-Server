using System;

[AttributeUsage(AttributeTargets.Field)]
public class SortedUnityObjectListAttribute : Attribute
{
	public bool AllowDuplicate;
}
