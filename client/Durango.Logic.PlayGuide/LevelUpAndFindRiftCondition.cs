using Shared.System;

namespace Durango.Logic.PlayGuide;

public class LevelUpAndFindRiftCondition : LevelUpCondition
{
	private bool _levelUp;

	protected override void OnRegister()
	{
		base.OnRegister();
		GameSystem<MapSystem>.Instance().ExploredPOI += MapSystem_ExploredRift;
	}

	protected override void OnUnregister()
	{
		base.OnUnregister();
		GameSystem<MapSystem>.Instance().ExploredPOI -= MapSystem_ExploredRift;
	}

	protected override void OnLevelUp()
	{
		_levelUp = true;
	}

	private void MapSystem_ExploredRift(PointOfInterest type, Point2 pos)
	{
		if (type == PointOfInterest.Rift && _levelUp)
		{
			Interrupt();
		}
	}
}
