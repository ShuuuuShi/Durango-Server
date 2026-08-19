using System.Collections.Generic;

namespace Crafting;

public class CategoryGeneric<T> : Category where T : CategoryItem
{
	public List<T> Recipes;

	public override CategoryItem[] Items => Recipes.ToArray();
}
