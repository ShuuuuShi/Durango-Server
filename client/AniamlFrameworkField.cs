using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public sealed class AniamlFrameworkField : Attribute
{
	public string ExposedName;

	public AniamlFrameworkField(string itemName)
	{
		ExposedName = itemName;
	}
}
