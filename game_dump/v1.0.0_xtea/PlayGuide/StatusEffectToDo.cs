namespace PlayGuide;

public class StatusEffectToDo : ToDoBase
{
	private readonly string _statusEffectId;

	public StatusEffectToDo(string id)
	{
		_statusEffectId = id;
	}

	public override void OnAddItem()
	{
		GameSystem<PlayerStatusEffectSystem>.Instance().OnAddStatusEffect += StatusEffectToDo_OnAddStatusEffect;
		if (GameSystem<PlayerStatusEffectSystem>.Instance().GetStatusEffect(_statusEffectId) != null)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<PlayerStatusEffectSystem>.Instance().OnAddStatusEffect -= StatusEffectToDo_OnAddStatusEffect;
	}

	private void StatusEffectToDo_OnAddStatusEffect(string id)
	{
		if (id == _statusEffectId)
		{
			CallComplete();
		}
	}
}
