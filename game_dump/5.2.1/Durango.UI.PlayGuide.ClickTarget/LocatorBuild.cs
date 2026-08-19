using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Shared.Building;
using UnityEngine;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorBuild : Locator
{
	private readonly LocatorInteraction _interaction;

	private readonly LocatorCraft _craft;

	private bool _isCraftPhase;

	private readonly bool _isTutorial;

	public LocatorBuild(bool tutorial = false)
	{
		_isTutorial = tutorial;
		_interaction = ((!tutorial) ? new LocatorInteraction(InteractionFilter) : new LocatorInteraction());
		_craft = new LocatorCraft(craft: false, tutorial);
	}

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
		_isCraftPhase = _interaction.TargetTransform == null || currentPhase == "select_slot" || currentPhase == "select_item" || currentPhase == "craft_button";
		if (_isTutorial && _interaction.TargetTransform == null)
		{
			_isCraftPhase = false;
		}
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
		Artifact component = target.GetComponent<Artifact>();
		if (component != null)
		{
			return component.BuildState == BuildingState.Occupied;
		}
		return false;
	}
}
