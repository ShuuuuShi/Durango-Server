namespace Durango.Logic.PlayGuide;

public class JoinClanToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<ClanSystem>.Instance().ClanChanged += ClanToDo_ClanChanged;
		ClanToDo_ClanChanged();
	}

	private void ClanToDo_ClanChanged()
	{
		if (PlayerBehavior.LocalPlayer.HasClan)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<ClanSystem>.Instance().ClanChanged -= ClanToDo_ClanChanged;
	}
}
