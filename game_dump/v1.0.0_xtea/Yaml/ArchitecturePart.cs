using System.Collections.Generic;

namespace Yaml;

public class ArchitecturePart
{
	public Gettext name;

	public string type;

	public string material;

	public int tier;

	public int effort;

	public Dictionary<string, ArchitecturePartSlot> slots;
}
