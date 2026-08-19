using System.Collections.Generic;
using Durango.Utils;
using NCalc;
using Newtonsoft.Json;
using Shared.Estate;

namespace Yaml;

public struct EstateCost
{
	[JsonProperty(PropertyName = "extending_cost")]
	public Dictionary<OwnerType, string> ExtendingCost;

	[JsonProperty(PropertyName = "expanding_cost")]
	public Dictionary<OwnerType, string> ExpandingCost;

	private bool _extendingCostFlag;

	private Dictionary<OwnerType, Expression> _extendingCost;

	private bool _expandingCostFlag;

	private Dictionary<OwnerType, Expression> _expandingCost;

	public long GetExtendingCost(OwnerType type, int size)
	{
		if (!_extendingCostFlag)
		{
			_extendingCostFlag = true;
			_extendingCost = new Dictionary<OwnerType, Expression>();
			foreach (KeyValuePair<OwnerType, string> item in ExtendingCost)
			{
				_extendingCost.Add(item.Key, ExpressionParser.Parse(item.Value));
			}
		}
		if (_extendingCost.TryGetValue(type, out var value))
		{
			value.Parameters["size"] = size;
			return value.ToInt64(0L);
		}
		return 0L;
	}

	public long GetExpandingCost(OwnerType type, int size)
	{
		if (!_expandingCostFlag)
		{
			_expandingCostFlag = true;
			_expandingCost = new Dictionary<OwnerType, Expression>();
			foreach (KeyValuePair<OwnerType, string> item in ExpandingCost)
			{
				_expandingCost.Add(item.Key, ExpressionParser.Parse(item.Value));
			}
		}
		if (_expandingCost.TryGetValue(type, out var value))
		{
			value.Parameters["size"] = size;
			return value.ToInt64(0L);
		}
		return 0L;
	}
}
