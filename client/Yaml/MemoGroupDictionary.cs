using System.Collections.Generic;
using Shared.Memo;

namespace Yaml;

public class MemoGroupDictionary : Dictionary<MemoType, Dictionary<int, MemoInfo>>
{
	public MemoGroupDictionary()
		: base((IEqualityComparer<MemoType>)default(MemoTypeComparer))
	{
	}
}
