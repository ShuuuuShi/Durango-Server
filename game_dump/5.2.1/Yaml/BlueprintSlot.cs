using System.Collections.Generic;

namespace Yaml;

public class BlueprintSlot
{
	public string slot_id;

	public Gettext slot_name;

	public string size_factor;

	public int count;

	public Dictionary<string, int> required_tags;

	public Dictionary<string, int> required_materials;

	public Dictionary<string, ArtifactLook> looks;

	public SlotSourceInfo[] source_info;

	public string default_look_tag;
}
