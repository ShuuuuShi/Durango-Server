using System.Collections.Generic;
using Shared.Estate;
using Shared.System;

namespace Estate;

public struct InteractionRights
{
	private readonly Dictionary<AccessRights, HashSet<Interaction>> _interactions;

	public InteractionRights(Dictionary<int, int[]> data)
	{
		_interactions = new Dictionary<AccessRights, HashSet<Interaction>>();
		foreach (KeyValuePair<int, int[]> datum in data)
		{
			AccessRights key = (AccessRights)datum.Key;
			HashSet<Interaction> hashSet = new HashSet<Interaction>();
			for (int i = 0; i < datum.Value.Length; i++)
			{
				hashSet.Add((Interaction)datum.Value[i]);
			}
			_interactions.Add(key, hashSet);
		}
	}

	public List<string> NeededRightNames(Interaction interaction)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<AccessRights, HashSet<Interaction>> interaction2 in _interactions)
		{
			if (interaction2.Value.Contains(interaction))
			{
				list.Add($"#Shared.Estate.AccessRights.{interaction2.Key}");
			}
		}
		return list;
	}
}
