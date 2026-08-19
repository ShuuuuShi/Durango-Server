using Shared.Survival;
using Yaml;

namespace FatigueData;

public struct FatigueVelocity
{
	public Shared.Survival.FatigueCategory Category;

	public Yaml.FatigueCategory CategoryData;

	public float Value;
}
