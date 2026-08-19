using System;

public class EnumIconAttribute : Attribute
{
	public string Icon { get; private set; }

	public string IconPC { get; private set; }

	public EnumIconAttribute(string icon)
	{
		Icon = icon;
	}

	public EnumIconAttribute(string icon, string iconPC)
	{
		Icon = icon;
		IconPC = iconPC;
	}
}
