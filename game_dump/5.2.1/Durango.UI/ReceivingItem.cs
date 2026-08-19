using Durango.Logic.Item;
using Messages;

namespace Durango.UI;

public struct ReceivingItem
{
	public double WarpStartsAt;

	public double ReceivingAt;

	public ItemData Item;

	public ReceivingItem(Messages.ReceivingItem msg)
	{
		WarpStartsAt = msg.WarpStartsAt;
		ReceivingAt = msg.ReceivingAt;
		Item = new ItemData(msg.Item);
		Item.Unstable = true;
	}
}
