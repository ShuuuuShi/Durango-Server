using Durango.Logic.Faction;
using Messages;

namespace Durango.Logic.PlayGuide;

public class MissionStartToDo : ToDoBase
{
	private readonly string _id;

	public MissionStartToDo(string id)
	{
		_id = id;
	}

	public override void OnAddItem()
	{
		if (CheckMissionStarted())
		{
			CallComplete();
			return;
		}
		FactionSystem factionSystem = GameSystem<FactionSystem>.Instance();
		factionSystem.FactionsUpdated += FactionSystem_FactionsUpdated;
		factionSystem.CheckSequenceMissionCleared(_id, delegate(bool cleared)
		{
			if (cleared)
			{
				CallComplete();
			}
		});
	}

	public override void OnRemoveItem()
	{
		GameSystem<FactionSystem>.Instance().FactionsUpdated -= FactionSystem_FactionsUpdated;
	}

	private void FactionSystem_FactionsUpdated()
	{
		if (CheckMissionStarted())
		{
			CallComplete();
		}
	}

	private bool CheckMissionStarted()
	{
		foreach (Durango.Logic.Faction.Faction faction in GameSystem<FactionSystem>.Instance().GetFactions())
		{
			if (faction.Mission.HasValue)
			{
				Mission value = faction.Mission.Value;
				if ((string.IsNullOrEmpty(_id) || value.Id == _id) && value.StartedAt.HasValue)
				{
					return true;
				}
			}
		}
		return false;
	}
}
