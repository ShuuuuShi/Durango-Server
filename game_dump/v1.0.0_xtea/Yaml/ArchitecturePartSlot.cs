using System.Collections.Generic;

namespace Yaml;

public class ArchitecturePartSlot
{
	public string name;

	public int count;

	public string description;

	public Dictionary<string, Dictionary<string, string>> filters;

	public static string NameKey(string id, string slotId)
	{
		return $"#architecture_part_{id}_{slotId}";
	}
}
