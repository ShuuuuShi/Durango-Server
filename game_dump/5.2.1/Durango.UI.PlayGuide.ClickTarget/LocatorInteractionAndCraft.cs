using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Shared.Building;
using UnityEngine;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorInteractionAndCraft : Locator
{
	private readonly LocatorInteraction _interaction = new LocatorInteraction(InteractionFilter);

	private readonly LocatorCraft _craft = new LocatorCraft();

	private bool _isCraftPhase;

	public override void Initialize(Dictionary<string, Parameter> dict)
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
		_isCraftPhase = currentPhase != "bottom_left_menu" && currentPhase != "craft_menu";
		if (_isCraftPhase)
		{
			return _craft.CurrentPhase;
		}
		return _interaction.CurrentPhase;
	}

	protected override void UpdateTargetTransform()
	{
		base.TargetTransform = ((!_isCraftPhase) ? _interaction.TargetTransform : _craft.TargetTransform);
		base.CurrentParameter.rotate = ((!_isCraftPhase) ? _interaction.Rotate() : _craft.Rotate());
		Vector2 vector = ((!_isCraftPhase) ? _interaction.GetOffset() : _craft.GetOffset());
		base.CurrentParameter.x = vector.x;
		base.CurrentParameter.y = vector.y;
	}

	private static bool InteractionFilter(GameObject target)
	{
		if (target == null)
		{
			return false;
		}
		Artifact component = target.GetComponent<Artifact>();
		if (component != null && component.BuildState == BuildingState.Completed)
		{
			return component.Condition != Shared.Building.Condition.Broken;
		}
		return false;
	}
}
