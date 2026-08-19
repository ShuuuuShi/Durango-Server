using System.Collections.Generic;
using JetBrains.Annotations;

namespace Durango.Logic.PlayGuide;

public class Flow
{
	public bool Common { get; private set; }

	[NotNull]
	public List<string> List { get; private set; }

	public Flow([CanBeNull] List<string> list, bool common)
	{
		List = ((list == null) ? new List<string>() : list);
		Common = common;
	}
}
