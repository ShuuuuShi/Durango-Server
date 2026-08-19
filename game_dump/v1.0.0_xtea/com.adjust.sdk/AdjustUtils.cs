using System.Collections.Generic;
using SimpleJSON;

namespace com.adjust.sdk;

public class AdjustUtils
{
	public static string KeyAdid = "adid";

	public static string KeyMessage = "message";

	public static string KeyNetwork = "network";

	public static string KeyAdgroup = "adgroup";

	public static string KeyCampaign = "campaign";

	public static string KeyCreative = "creative";

	public static string KeyWillRetry = "willRetry";

	public static string KeyTimestamp = "timestamp";

	public static string KeyEventToken = "eventToken";

	public static string KeyClickLabel = "clickLabel";

	public static string KeyTrackerName = "trackerName";

	public static string KeyTrackerToken = "trackerToken";

	public static string KeyJsonResponse = "jsonResponse";

	public static int ConvertLogLevel(AdjustLogLevel? logLevel)
	{
		if (!logLevel.HasValue)
		{
			return -1;
		}
		return (int)logLevel.Value;
	}

	public static int ConvertBool(bool? value)
	{
		if (!value.HasValue)
		{
			return -1;
		}
		if (value.Value)
		{
			return 1;
		}
		return 0;
	}

	public static double ConvertDouble(double? value)
	{
		if (!value.HasValue)
		{
			return -1.0;
		}
		return value.Value;
	}

	public static string ConvertListToJson(List<string> list)
	{
		if (list == null)
		{
			return null;
		}
		JSONArray jSONArray = new JSONArray();
		foreach (string item in list)
		{
			jSONArray.Add(new JSONData(item));
		}
		return jSONArray.ToString();
	}

	public static string GetJsonResponseCompact(Dictionary<string, object> dictionary)
	{
		string empty = string.Empty;
		if (dictionary == null)
		{
			return empty;
		}
		int num = 0;
		empty += "{";
		foreach (KeyValuePair<string, object> item in dictionary)
		{
			if (item.Value is string text)
			{
				if (++num > 1)
				{
					empty += ",";
				}
				string text2 = empty;
				empty = text2 + "\"" + item.Key + "\":\"" + text + "\"";
			}
			else
			{
				Dictionary<string, object> dictionary2 = item.Value as Dictionary<string, object>;
				if (++num > 1)
				{
					empty += ",";
				}
				empty = empty + "\"" + item.Key + "\":";
				empty += GetJsonResponseCompact(dictionary2);
			}
		}
		return empty + "}";
	}

	public static string GetJsonString(JSONNode node, string key)
	{
		if (node == null)
		{
			return null;
		}
		JSONData jSONData = node[key] as JSONData;
		if (jSONData == null)
		{
			return null;
		}
		return jSONData.Value;
	}

	public static void WriteJsonResponseDictionary(JSONClass jsonObject, Dictionary<string, object> output)
	{
		foreach (KeyValuePair<string, JSONNode> item in jsonObject)
		{
			JSONClass asObject = item.Value.AsObject;
			string key = item.Key;
			if (asObject == null)
			{
				string value = item.Value.Value;
				output.Add(key, value);
			}
			else
			{
				Dictionary<string, object> dictionary = new Dictionary<string, object>();
				output.Add(key, dictionary);
				WriteJsonResponseDictionary(asObject, dictionary);
			}
		}
	}
}
