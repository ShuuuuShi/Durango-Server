using System.Collections.Generic;
using Shared.Item;

namespace Yaml;

public class Prototype
{
	public int min_level;

	public int max_level;

	public Gettext description;

	public Gettext name;

	public Gettext category;

	public string icon;

	public List<ColorChannel> dyeables;
}
