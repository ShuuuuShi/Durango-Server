using System.Collections.Generic;
using Messages;

namespace ItemSystem;

public class PerformanceData
{
	public string id;

	public Gettext name;

	public string icon;

	public readonly Dictionary<string, float> num_attrs;

	public readonly Dictionary<string, string> str_attrs;

	public PerformanceData()
	{
		num_attrs = new Dictionary<string, float>();
		str_attrs = new Dictionary<string, string>();
	}

	public PerformanceData(Performance performance)
	{
		id = performance.Id;
		name = performance.Name;
		icon = ((!string.IsNullOrEmpty(performance.Icon)) ? performance.Icon : "icon_question");
		num_attrs = performance.Nums;
		str_attrs = performance.Strs;
	}

	public PerformanceData(PerformanceJson json)
	{
		id = json.id;
		name = json.name;
		icon = ((!string.IsNullOrEmpty(json.icon)) ? json.icon : "icon_question");
		num_attrs = json.nums;
		str_attrs = json.strs;
	}
}
