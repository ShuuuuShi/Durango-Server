namespace Durango.UI;

public class ActionTooltip_PC : ActionTooltipBase
{
	protected override void UpdateLayout()
	{
		_titleLabel.UpdateLayout(310);
		_descriptionLabel.width = 297;
		_layout.UpdateLayout(310f, 0f);
		UIUtility.UpdateAnchors(base.transform);
	}
}
