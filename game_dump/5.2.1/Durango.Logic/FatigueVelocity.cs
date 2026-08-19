using Shared.Survival;
using Yaml;

namespace Durango.Logic;

public struct FatigueVelocity
{
	public Shared.Survival.FatigueCategory Category;

	public Yaml.FatigueCategory CategoryData;

	public float Value;

	public float Resistance;
}
