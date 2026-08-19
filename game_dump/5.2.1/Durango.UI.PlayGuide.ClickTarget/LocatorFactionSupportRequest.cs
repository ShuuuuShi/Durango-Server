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
			if (_factionGroup.CurrentMode == FactionGroup.Mode.SupportRequest)
			{
				return "request_button";
			}
			return "support_page";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		string currentPhase = base.CurrentPhase;
		if (!(currentPhase == "support_page"))
		{
			if (currentPhase == "request_button")
			{
				base.TargetTransform = _factionGroup.GetRequestAvailableButtonTransform();
			}
			else
			{
				base.UpdateTargetTransform();
			}
		}
		else
		{
			base.TargetTransform = _factionGroup.GetSupportAvailableButtonTransform();
		}
	}
}
