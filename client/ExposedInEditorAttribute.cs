using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field)]
public class ExposedInEditorAttribute : Attribute
{
	public bool Editable = true;

	public string Name;

	public ExposedInEditorAttribute(string name = null)
	{
		Name = name;
	}

	public ExposedInEditorAttribute(bool editable, string name = null)
	{
		Editable = editable;
		Name = name;
	}
}
