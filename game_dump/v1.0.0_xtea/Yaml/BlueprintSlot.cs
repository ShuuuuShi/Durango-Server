using System.Collections.Generic;

namespace Yaml;

public class BlueprintSlot
{
	public Gettext slot_name;

	public Gettext description;

	public string size_factor;

	public int count;

	public Dictionary<string, int> required_tags;

	public Dictionary<string, int> required_materials;

	public Dictionary<string, ArtifactLook> looks;
}
