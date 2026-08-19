using System.Collections.Generic;
using Shared.Rank;
using Yaml.Util;

namespace Yaml;

public class RankingRewards : SingletonDict<Category, Dictionary<string, List<RankingReward>>>
{
}
