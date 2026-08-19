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
		if (_exploreGroup.IsTargetToolTipVisible(_role, _biome, _level))
		{
			return "click_tooltip";
		}
		return "select_island";
	}

	protected override void UpdateTargetTransform()
	{
		if (IsInteractionPhase())
		{
			base.TargetTransform = _interaction.TargetTransform;
			return;
		}
		string currentPhase = base.CurrentPhase;
		if (!(currentPhase == "select_island"))
		{
			if (currentPhase == "click_tooltip")
			{
				base.TargetTransform = _exploreGroup.GetTooltipButtonTransoform();
			}
		}
		else
		{
			base.TargetTransform = _exploreGroup.GetIslandTransoform(_role, _biome, _level);
		}
	}

	private bool IsInteractionPhase()
	{
		if (!(_exploreGroup == null))
		{
			return !_exploreGroup.IsOpened;
		}
		return true;
	}
}
