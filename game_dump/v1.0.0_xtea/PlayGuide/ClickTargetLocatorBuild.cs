using System.Collections.Generic;
using Shared.Building;
using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocatorBuild : ClickTargetLocator
{
	private readonly ClickTargetLocatorInteraction _interaction = new ClickTargetLocatorInteraction(InteractionFilter);

	private readonly ClickTargetLocatorCraft _craft = new ClickTargetLocatorCraft(craft: false);

	private bool _isCraftPhase;

	public override void Initialize(Dictionary<string, ClickTargetData> dict)
	{
		_interaction.Initialize(dict);
		_craft.Initialize(dict);
		base.Initialize(dict);
	}

	protected override string SelectPhase()
	{
		_interaction.Process();
		_craft.Process();
		string currentPhase = _craft.CurrentPhase;
		_isCraftPhase = (Object)(object)_interaction.TargetTransform == (Object)null || currentPhase == "select_slot" || currentPhase == "select_item" || currentPhase == "craft_button";
		return (!_isCraftPhase) ? _interaction.CurrentPhase : _craft.CurrentPhase;
	}

	protected override void UpdateTargetTransform()
	{
		base.TargetTransform = ((!_isCraftPhase) ? _interaction.TargetTransform : _craft.TargetTransform);
	}

	private static bool InteractionFilter(GameObject target)
	{
		if ((Object)(object)target == (Object)null)
		{
			return false;
		}
		Artifact component = target.GetComponent<Artifact>();
		return (Object)(object)component != (Object)null && component.BuildState == BuildingState.Occupied;
	}
}
