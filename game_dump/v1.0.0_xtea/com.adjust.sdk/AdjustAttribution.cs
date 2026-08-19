using System.Collections.Generic;
using SimpleJSON;

namespace com.adjust.sdk;

public class AdjustAttribution
{
	public string network { get; set; }

	public string adgroup { get; set; }

	public string campaign { get; set; }

	public string creative { get; set; }

	public string clickLabel { get; set; }

	public string trackerName { get; set; }

	public string trackerToken { get; set; }

	public AdjustAttribution()
	{
	}

	public AdjustAttribution(string jsonString)
	{
		JSONNode jSONNode = JSON.Parse(jsonString);
		if (!(jSONNode == null))
		{
			trackerName = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyTrackerName);
			trackerToken = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyTrackerToken);
			network = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyNetwork);
			campaign = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyCampaign);
			adgroup = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyAdgroup);
			creative = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyCreative);
			clickLabel = AdjustUtils.GetJsonString(jSONNode, AdjustUtils.KeyClickLabel);
		}
	}

	public AdjustAttribution(Dictionary<string, string> dicAttributionData)
	{
		if (dicAttributionData != null)
		{
			trackerName = TryGetValue(dicAttributionData, AdjustUtils.KeyTrackerName);
			trackerToken = TryGetValue(dicAttributionData, AdjustUtils.KeyTrackerToken);
			network = TryGetValue(dicAttributionData, AdjustUtils.KeyNetwork);
			campaign = TryGetValue(dicAttributionData, AdjustUtils.KeyCampaign);
			adgroup = TryGetValue(dicAttributionData, AdjustUtils.KeyAdgroup);
			creative = TryGetValue(dicAttributionData, AdjustUtils.KeyCreative);
			clickLabel = TryGetValue(dicAttributionData, AdjustUtils.KeyClickLabel);
		}
	}

	private static string TryGetValue(Dictionary<string, string> dic, string key)
	{
		if (dic.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}
}
