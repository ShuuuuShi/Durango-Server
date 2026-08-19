using System.Collections.Generic;
using Crafting;

namespace Building_;

public class BlueprintCategory : CategoryGeneric<Blueprint>
{
	public List<Blueprint> Blueprints
	{
		get
		{
			return Recipes;
		}
		set
		{
			Recipes = value;
		}
	}
}
