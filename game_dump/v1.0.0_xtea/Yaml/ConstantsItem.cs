using System.Collections.Generic;
using Shared.Item;

namespace Yaml;

public struct ConstantsItem
{
	public Dictionary<ColorChannel, string> dye_recipe;

	public Dictionary<ColorChannel, string> bleach_recipe;
}
