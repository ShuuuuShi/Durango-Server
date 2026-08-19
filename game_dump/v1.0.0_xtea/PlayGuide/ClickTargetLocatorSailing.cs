using UnityEngine;

namespace PlayGuide;

public class ClickTargetLocatorSailing : ClickTargetLocatorInteraction
{
	private ExploreGroup _exploreGroup;

	protected override void OnInitialized()
	{
		_exploreGroup = UIManager.FindScript<ExploreGroup>();
		base.OnInitialized();
	}

	protected override string SelectPhase()
	{
		if ((Object)(object)_exploreGroup != (Object)null && _exploreGroup.IsOpen)
		{
			return (!_exploreGroup.IsRouteMode()) ? "click_enter" : "click_explore";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "click_enter":
			base.TargetTransform = _exploreGroup.GetEnterButtonTransform();
			break;
		case "click_explore":
			base.TargetTransform = _exploreGroup.GetExploreButtonTransform();
			break;
		}
		base.UpdateTargetTransform();
	}
}
