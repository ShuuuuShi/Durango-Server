using System;

namespace InteractionData;

public class MenuAttribute : Attribute
{
	public int Priority { get; private set; }

	public MenuType Type { get; private set; }

	public MenuAttribute(int value)
		: this(value, MenuType.Normal)
	{
	}

	public MenuAttribute(int value, MenuType type)
	{
		Priority = value;
		Type = type;
	}
}
