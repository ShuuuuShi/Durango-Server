namespace Durango.Logic.PlayGuide;

public class MarketBuyToDo : ToDoBase
{
	private void MarketBuyToDo_SuccessItemBuy()
	{
		CallComplete();
	}

	public override void OnAddItem()
	{
		GameSystem<MarketSystem>.Instance().SuccessItemBuy += MarketBuyToDo_SuccessItemBuy;
	}

	public override void OnRemoveItem()
	{
		GameSystem<MarketSystem>.Instance().SuccessItemBuy -= MarketBuyToDo_SuccessItemBuy;
	}
}
