using Durango.Utils;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public struct CargoCost
{
	[JsonProperty(PropertyName = "immediate_receiving_cost")]
	public string ImmediateReceivingCost;

	private bool _immediateReceivingCostFlag;

	private Expression _immediateReceivingCost;

	public long GetImmediateReceivingCost(double leftTime)
	{
		if (!_immediateReceivingCostFlag)
		{
			_immediateReceivingCostFlag = true;
			_immediateReceivingCost = ExpressionParser.Parse(ImmediateReceivingCost);
		}
		if (_immediateReceivingCost == null)
		{
			return 0L;
		}
		_immediateReceivingCost.Parameters["left_time"] = leftTime;
		return _immediateReceivingCost.ToInt64(0L);
	}
}
