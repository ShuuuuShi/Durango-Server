using System.Collections.Generic;
using JetBrains.Annotations;

namespace PlayGuide;

public static class ClickTargetFactory
{
	[NotNull]
	public static ClickTargetLocator Create(string type, Dictionary<string, ClickTargetData> dict)
	{
		if (dict == null)
		{
			dict = new Dictionary<string, ClickTargetData>();
		}
		ClickTargetLocator clickTargetLocator = type switch
		{
			"menu" => new ClickTargetLocatorMenu(), 
			"inventory" => new ClickTargetLocatorInventory(), 
			"interaction" => new ClickTargetLocatorInteraction(), 
			"craft" => new ClickTargetLocatorCraft(), 
			"interaction_and_craft" => new ClickTargetLocatorInteractionAndCraft(), 
			"build" => new ClickTargetLocatorBuild(), 
			"sailing" => new ClickTargetLocatorSailing(), 
			_ => new ClickTargetLocator(), 
		};
		clickTargetLocator.Initialize(dict);
		return clickTargetLocator;
	}
}
