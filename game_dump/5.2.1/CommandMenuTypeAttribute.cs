using System;
using Durango.Logic;

public class CommandMenuTypeAttribute : Attribute
{
	public MenuType Menu { get; private set; }

	public CommandMenuTypeAttribute(MenuType menu)
	{
		Menu = menu;
	}
}
