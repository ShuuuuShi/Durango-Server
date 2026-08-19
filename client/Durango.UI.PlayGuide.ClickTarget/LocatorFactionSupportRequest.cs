using Durango.Logic;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorFactionSupportRequest : LocatorMenu
{
	private FactionGroup _factionGroup;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_factionGroup = UIManager.FindScript<FactionGroup>();
		SetMenuType(MenuType.Faction);
	}

	protected override string SelectPhase()
	{
		if (_factionGroup != null && _factionGroup.IsOpened)
		{
			return (_factionGroup.CurrentMode != FactionGroup.Mode.SupportRequest) ? "support_page" : "request_button";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "support_page":
			base.TargetTransform = _factionGroup.GetSupportAvailableButtonTransform();
			break;
		case "request_button":
			base.TargetTransform = _factionGroup.GetRequestAvailableButtonTransform();
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}
}
