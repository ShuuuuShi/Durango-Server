using Durango.Logic.PlayGuide;
using Durango.Utils.Extensions;
using InteractionData;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorContextAction : Locator
{
	private Interaction _action;

	private ContextActionGroupBase _actionGroup;

	protected override void OnInitialized()
	{
		Parameter parameter = Parameters.Get("current");
		if (parameter != null)
		{
			_action = parameter.id.ToEnum(Interaction.None);
		}
		_actionGroup = UIManager.FindScript<ContextActionGroupBase>();
	}

	protected override void UpdateTargetTransform()
	{
		base.TargetTransform = _actionGroup.GetActionTransform(_action, out var index);
		if (index == 0 || index == 1 || index == 2 || index == 8)
		{
			base.CurrentParameter.rotate = 180f;
		}
		else
		{
			base.CurrentParameter.rotate = 0f;
		}
	}
}
