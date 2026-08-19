using System.Collections.Generic;
using Messages;
using Shared.Laboratory;
using Yaml;
using Yaml.Util;

public static class AvailablePersonalResearchExtension
{
	public static ResearchCategory GetCategory(this AvailablePersonalResearch msg)
	{
		foreach (Pair<string, int?> item in msg.ResearchableIds())
		{
			PersonalResearch personalResearch = SingletonDict<string, PersonalResearch>.Get(item.Item1);
			if (personalResearch == null)
			{
				continue;
			}
			return personalResearch.Category;
		}
		return ResearchCategory.Invalid;
	}

	public static IEnumerable<Pair<string, int?>> ResearchableIds(this AvailablePersonalResearch msg)
	{
		if (msg.AvailableResearchIds != null)
		{
			string[] availableResearchIds = msg.AvailableResearchIds;
			foreach (string id in availableResearchIds)
			{
				yield return new Pair<string, int?>(id, null);
			}
		}
		if (msg.UnavailableResearchIds != null)
		{
			Pair<string, int>[] unavailableResearchIds = msg.UnavailableResearchIds;
			for (int j = 0; j < unavailableResearchIds.Length; j++)
			{
				Pair<string, int> pair = unavailableResearchIds[j];
				yield return new Pair<string, int?>(pair.Item1, pair.Item2);
			}
		}
	}
}
