namespace PlayGuide;

public class ClanToDo : ToDoBase
{
	public override void OnAddItem()
	{
		GameSystem<ClanSystem>.Instance().ClanChanged += ClanToDo_ClanChanged;
		ClanToDo_ClanChanged(0uL, PlayerBehavior.LocalPlayer.ClanId);
	}

	private void ClanToDo_ClanChanged(ulong prev, ulong cur)
	{
		if (cur != 0)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<ClanSystem>.Instance().ClanChanged += ClanToDo_ClanChanged;
	}
}
