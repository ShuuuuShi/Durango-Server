using System.Collections.Generic;
using Shared.Ability;

namespace Yaml;

public class Title
{
	public Gettext name;

	public Gettext description;

	public Dictionary<Basic, int> abilities;

	public Dictionary<string, float> modifiers;
}
