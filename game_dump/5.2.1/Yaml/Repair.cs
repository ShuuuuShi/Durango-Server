using Durango.Utils;
using NCalc;
using Newtonsoft.Json;

namespace Yaml;

public struct Repair
{
	[JsonProperty(PropertyName = "artifact", Required = Required.Always)]
	public RepairItem Artifact;

	[JsonProperty(PropertyName = "item", Required = Required.Always)]
	public RepairItem Item;

	[JsonProperty(PropertyName = "repair_requirement_perf", Required = Required.Always)]
	public string RepairRequirementFormula;

	private bool _repairRequirementPerformanceFlag;

	private Expression _repairRequirementPerformance;

	public int GetRepairRequirementPerformance(int repairRequirement, int level)
	{
		if (!_repairRequirementPerformanceFlag)
		{
			_repairRequirementPerformanceFlag = true;
			_repairRequirementPerformance = ExpressionParser.Parse(RepairRequirementFormula);
		}
		if (_repairRequirementPerformance == null)
		{
			return 0;
		}
		_repairRequirementPerformance.Parameters["repair_requirement"] = repairRequirement;
		_repairRequirementPerformance.Parameters["level"] = level;
		return _repairRequirementPerformance.ToInt32();
	}
}
