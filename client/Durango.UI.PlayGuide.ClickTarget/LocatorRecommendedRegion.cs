using Durango.Logic;
using Durango.UI.Popup;
using Shared.Region;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorRecommendedRegion : LocatorMenu
{
	private EstateGroup _estateGroup;

	private RouteInfoTooltip _routeInfoTooltip;

	private RouteInfoTooltip RouteInfoTooltip
	{
		get
		{
			if (_routeInfoTooltip == null)
			{
				InfoTooltip infoTooltip = UIManager.Popup.Tooltip<InfoTooltip>();
				_routeInfoTooltip = infoTooltip.GetComponent<RouteInfoTooltip>();
			}
			return _routeInfoTooltip;
		}
	}

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_estateGroup = UIManager.FindScript<EstateGroup>();
		SetMenuType(MenuType.Estate);
	}

	protected override string SelectPhase()
	{
		if (_estateGroup != null && _estateGroup.IsOpened)
		{
			if (RouteInfoTooltip != null && RouteInfoTooltip.IsVisible && RouteInfoTooltip.GetRouteRole() == GetRecommendedRole())
			{
				return "touch_sail_button";
			}
			return "select_region";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "open_recommended_region":
			base.CurrentParameter.rotate = 90f;
			break;
		case "select_region":
			base.TargetTransform = _estateGroup.GetRecommendedRegionNodeTransform(GetRecommendedRole());
			break;
		case "touch_sail_button":
			base.TargetTransform = GetSailRegionButtonTransform();
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}

	private Transform GetSailRegionButtonTransform()
	{
		return (!(RouteInfoTooltip != null)) ? null : RouteInfoTooltip.ButtonTransform;
	}

	private static Role GetRecommendedRole()
	{
		int num = Singleton<Constants>.Instance.Sailing.RoleOpeningLevels.Get(6, 0);
		return (GameSystem<StatisticsSystem>.Instance().Level < num) ? Role.Rural : Role.Urban;
	}
}
