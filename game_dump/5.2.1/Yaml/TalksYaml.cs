using System;
using System.Collections.Generic;
using Shared.Faction;
using Yaml.Util;

namespace Yaml;

public class TalksYaml : SingletonDict<FactionType, Talks[]>, IComparer<Talks>
{
	protected override void OnInitalized()
	{
		base.OnInitalized();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			Array.Sort(enumerator.Current.Value, this);
		}
	}

	public int Compare(Talks t1, Talks t2)
	{
		if (t1 == null && t2 == null)
		{
			return 0;
		}
		if (t1 == null)
		{
			return 1;
		}
		if (t2 == null)
		{
			return -2;
		}
		return t1.FriendshipPoint - t2.FriendshipPoint;
	}
}
