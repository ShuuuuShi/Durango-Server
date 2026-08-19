using Messages;

namespace Durango.Logic.PlayGuide;

internal class NoPersonalRegion : FlowCondition
{
	protected override void OnRegister()
	{
		EstateSystem.GetPersonalRegionInfo(delegate(PersonalRegionInfo info)
		{
			PersonalRegion? personalRegion = info.PersonalRegion;
			if (!personalRegion.HasValue)
			{
				Interrupt();
			}
		});
	}
}
