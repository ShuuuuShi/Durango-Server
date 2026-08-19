using System.Collections.Generic;
using Durango.Utils;
using InteractionData;

namespace Durango.UI;

public class ContextActionButtons : ContextActionButtonsBase
{
	protected override void Start()
	{
		base.Start();
		GameSystem<InteractionSystem>.Instance().RegisterContextActionFinder(ContextActionFinder);
	}

	private void ContextActionFinder(List<InteractionMenuData> result)
	{
		if (Singleton<PlayerController>.Instance().CanTryJump())
		{
			result.Add(Interaction.Dash);
		}
	}
}
