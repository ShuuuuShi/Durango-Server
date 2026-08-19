using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class PlayerActions : SingletonDict<string, PlayerAction>
{
	protected override void OnInitalized()
	{
		base.OnInitalized();
		using Enumerator enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<string, PlayerAction> current = enumerator.Current;
			current.Value.Id = current.Key;
		}
	}
}
