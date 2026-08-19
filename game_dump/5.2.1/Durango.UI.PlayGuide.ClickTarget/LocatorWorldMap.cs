using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorWorldMap : Locator
{
	private MinimapGroupBase _minimapGroup;

	private WorldMapGroup _worldMapGroup;

	private WorldMapGroup.ButtonType _buttonType;

	protected override void OnInitialized()
	{
		Parameter parameter = Parameters.Get("select_button");
		if (parameter != null)
		{
			_buttonType = parameter.id.ToEnum(WorldMapGroup.ButtonType.WarpBack);
		}
		_minimapGroup = UIManager.FindScript<MinimapGroupBase>();
		_worldMapGroup = UIManager.FindScript<WorldMapGroup>();
	}

	protected override string SelectPhase()
	{
		if (_worldMapGroup == null || !_worldMapGroup.IsOpened)
		{
			return "minimap";
		}
		return "select_button";
	}

	protected override void UpdateTargetTransform()
	{
		string currentPhase = base.CurrentPhase;
		if (!(currentPhase == "minimap"))
		{
			if (currentPhase == "select_button")
			{
				base.TargetTransform = _worldMapGroup.GetButtonTransform(_buttonType);
			}
		}
		else
		{
			base.TargetTransform = _minimapGroup.GetTouchTransform();
		}
	}
}
