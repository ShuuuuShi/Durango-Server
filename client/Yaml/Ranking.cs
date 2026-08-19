using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Utils;
using Newtonsoft.Json;

namespace Yaml;

public class Ranking
{
	[JsonProperty(PropertyName = "revision")]
	public Dictionary<string, Revision> Revisions;

	private List<KeyValuePair<string, Revision>> _orderedRevisions;

	private List<KeyValuePair<string, Revision>> OrderedRevisions
	{
		get
		{
			if (_orderedRevisions == null)
			{
				_orderedRevisions = Revisions.OrderBy((KeyValuePair<string, Revision> revision) => revision.Value.FinishAt).ToList();
			}
			return _orderedRevisions;
		}
	}

	public KeyValuePair<string, string> GetCurrentAndPrevRevisionId(DateTime utcNow)
	{
		if (KUtility.GetSize(Revisions) <= 0)
		{
			return new KeyValuePair<string, string>(null, null);
		}
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < OrderedRevisions.Count; i++)
		{
			Revision value = OrderedRevisions[i].Value;
			if (!Times.TryParse(value.FinishAt, out var result))
			{
				continue;
			}
			DateTimeOffset result2;
			if (utcNow > result)
			{
				num3 = i;
			}
			else if (Times.TryParse(value.StartsAt, out result2))
			{
				if (!(utcNow < result2))
				{
					num = i;
					num2 = i - 1;
					break;
				}
				if (num3 > 0)
				{
					num2 = num3;
					num3 = -1;
				}
			}
		}
		if (num < 0 && num2 < 0)
		{
			num2 = OrderedRevisions.Count - 1;
		}
		if (num2 >= 0)
		{
			if (Times.TryParse(OrderedRevisions[num2].Value.RewardAcquireLimitAt, out var result3))
			{
				if (utcNow > result3)
				{
					num2 = -1;
				}
			}
			else
			{
				num2 = -1;
			}
		}
		string key = ((num >= 0) ? OrderedRevisions[num].Key : null);
		string value2 = ((num2 >= 0) ? OrderedRevisions[num2].Key : null);
		return new KeyValuePair<string, string>(key, value2);
	}
}
