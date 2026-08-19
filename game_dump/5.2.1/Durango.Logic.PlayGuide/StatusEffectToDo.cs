namespace Durango.Logic.PlayGuide;

public class StatusEffectToDo : ToDoBase
{
	private readonly string _statusEffectId;

	public StatusEffectToDo(string id)
	{
		_statusEffectId = id;
	}

	public override void OnAddItem()
	{
		GameSystem<StatusEffectSystem>.Instance().StatusEffectAdded += PlayerStatusEffectSystem_StatusEffectAdded;
		if (GameSystem<StatusEffectSystem>.Instance().GetStatusEffect(_statusEffectId) != null)
		{
			CallComplete();
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<StatusEffectSystem>.Instance().StatusEffectAdded -= PlayerStatusEffectSystem_StatusEffectAdded;
	}

	private void PlayerStatusEffectSystem_StatusEffectAdded(string entityId, string id)
	{
		if (!(entityId != GameManager.PlayerId) && id == _statusEffectId)
		{
			CallComplete();
		}
	}
}
