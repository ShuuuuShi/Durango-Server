using Durango.Logic.Item;
using Durango.UI.Control;
using Messages;

namespace Durango.UI;

public class FactionSupportRequestRewardWidget : Durango.UI.Control.ItemIconWidget
{
	public void Set(ItemSupportReward reward)
	{
		Set(new ItemData(reward.Item), reward.Count);
	}
}
