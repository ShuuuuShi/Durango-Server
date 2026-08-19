using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Durango.Logic.Clusters;

public class Cluster
{
	public static Cluster Null = new Cluster();

	[JsonProperty(PropertyName = "name")]
	public Dictionary<string, string> Names;

	[JsonProperty(PropertyName = "gateway_url_root")]
	public string GatewayUrlRoot;

	[JsonProperty(PropertyName = "hard_cap")]
	public string HardCap;

	[JsonProperty(PropertyName = "countries")]
	public string[] Countries;

	[JsonProperty(PropertyName = "timed_ticket_url")]
	public string TimedTicketUrl;

	[CanBeNull]
	[JsonProperty(PropertyName = "maintenance")]
	private Maintenance _maintenance;

	public Action<Action<Account>> OnRequestAccount { get; set; }

	public Action<string> OnDeletePlayer { get; set; }

	public Action<string> OnConfirm { get; set; }

	public bool IsRecommendable { get; set; }

	public Mode Mode { get; set; }

	public string LocalPlayer { get; set; }

	public string GetName([CanBeNull] string locale)
	{
		if (Names == null)
		{
			return string.Empty;
		}
		if (string.IsNullOrEmpty(locale) || !Names.ContainsKey(locale))
		{
			locale = "en_US";
		}
		return Names.Get(locale);
	}

	public bool IsInMaintenance()
	{
		if (_maintenance != null)
		{
			return _maintenance.IsInMaintenance();
		}
		return false;
	}

	public string GetMaintenanceText([CanBeNull] string locale, bool em = true)
	{
		if (_maintenance != null)
		{
			return _maintenance.GetMaintenanceText(locale, em);
		}
		return string.Empty;
	}
}
