using System.Collections.Generic;

namespace ItemSystem;

public struct PerformanceJson
{
	public string id;

	public Gettext name;

	public string icon;

	public Dictionary<string, string> strs;

	public Dictionary<string, float> nums;
}
