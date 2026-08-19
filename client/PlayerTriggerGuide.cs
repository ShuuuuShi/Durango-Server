using UnityEngine;

public class PlayerTriggerGuide : PlayerTriggerBase
{
	[SerializeField]
	private string _flowNameEntered;

	[SerializeField]
	private string _flowNameExited;

	protected override void DoTriggerEnter(Collider other)
	{
		if (!string.IsNullOrEmpty(_flowNameEntered))
		{
			GameSystem<PlayGuideSystem>.Instance().BeginFlow(_flowNameEntered);
		}
	}

	protected override void DoTriggerExit(Collider other)
	{
		if (!string.IsNullOrEmpty(_flowNameExited))
		{
			GameSystem<PlayGuideSystem>.Instance().BeginFlow(_flowNameExited);
		}
	}
}
