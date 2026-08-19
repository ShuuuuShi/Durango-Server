using Durango.Logic;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorEstate : LocatorMenu
{
	private EstateGroup _estateGroup;

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
			return "declare_estate";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		string currentPhase = base.CurrentPhase;
		if (currentPhase != null && currentPhase == "declare_estate")
		{
			base.TargetTransform = _estateGroup.DelcareEstateButton;
		}
		else
		{
			base.UpdateTargetTransform();
		}
	}
}
