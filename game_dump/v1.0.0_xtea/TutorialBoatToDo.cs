using Building_;
using L10N;
using Messages;
using PlayGuide;

public class TutorialBoatToDo : ToDoBase
{
	public string SlotId;

	public void Set(BlueprintSlot slot)
	{
		base.Key = $"TutorialBoat.{slot.Id}";
		SlotId = slot.Id;
		base.TargetProgress = slot.RequiredCount;
		base.LocalText = T._("[ffd85b]{0}[-] 뗏목에 넣기", slot.LocalizedName);
	}

	public override void OnAddItem()
	{
		GameSystem<TutorialIslandSystem>.Instance().TutorialBoatSessionUpdated += OnUpdateTutorialBoatSession;
	}

	public override void OnRemoveItem()
	{
		GameSystem<TutorialIslandSystem>.Instance().TutorialBoatSessionUpdated -= OnUpdateTutorialBoatSession;
	}

	public void OnUpdateTutorialBoatSession(TutorialSession session)
	{
		CallProgressChange((session.Materials != null) ? session.Materials.Get(SlotId, 0) : 0);
	}
}
