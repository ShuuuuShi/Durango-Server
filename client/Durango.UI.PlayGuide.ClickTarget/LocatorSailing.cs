using System.Collections.Generic;
using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;
using Shared.Region;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorSailing : Locator
{
	private readonly LocatorInteraction _interaction = new LocatorInteraction();

	private Role _role = Role.Invalid;

	private Biome _biome = Biome.Invalid;

	private int _level;

	private ExploreGroup _exploreGroup;

	public override void Initialize(Dictionary<string, Parameter> dict)
	{
		_interaction.Initialize(dict);
		base.Initialize(dict);
	}

	protected override void OnInitialized()
	{
		Parameter parameter = Parameters.Get("select_island");
		if (parameter != null)
		{
			int.TryParse(parameter.param, out _level);
			if (parameter.id != null)
			{
				string[] array = parameter.id.Split(':');
				_role = array[0].Trim().ToEnum(Role.Invalid);
				if (array.Length > 1)
				{
					_biome = array[1].Trim().ToEnum(Biome.Invalid);
				}
			}
		}
		_exploreGroup = UIManager.FindScript<ExploreGroup>();
	}

	protected override string SelectPhase()
	{
		_interaction.Process();
		if (IsInteractionPhase())
		{
			return _interaction.CurrentPhase;
		}
		return (!_exploreGroup.IsTargetToolTipVisible(_role, _biome, _level)) ? "select_island" : "click_tooltip";
	}

	protected override void UpdateTargetTransform()
	{
		if (IsInteractionPhase())
		{
			base.TargetTransform = _interaction.TargetTransform;
			return;
		}
		switch (base.CurrentPhase)
		{
		case "select_island":
			base.TargetTransform = _exploreGroup.GetIslandTransoform(_role, _biome, _level);
			break;
		case "click_tooltip":
			base.TargetTransform = _exploreGroup.GetTooltipButtonTransoform();
			break;
		}
	}

	private bool IsInteractionPhase()
	{
		return _exploreGroup == null || !_exploreGroup.IsOpened;
	}
}
