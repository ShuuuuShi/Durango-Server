using UnityEngine;

namespace Durango.Prologue;

public class TriggerSetNextGuide : TriggerOnce
{
	public PrologueGuideSystem.PrologueGuideState _nextGuideName;

	protected override bool TriggerEntered(Collider other)
	{
		GameSystem<PrologueGuideSystem>.Instance().SetNextGuide(_nextGuideName);
		return true;
	}

	protected override bool TriggerExited(Collider other)
	{
		return true;
	}
}
