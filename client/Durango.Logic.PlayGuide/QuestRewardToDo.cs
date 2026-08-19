using Messages;
using Shared.Quest;

namespace Durango.Logic.PlayGuide;

public class QuestRewardToDo : ToDoBase
{
	private readonly string _questId;

	public QuestRewardToDo(string questId)
	{
		_questId = questId;
	}

	public override void OnAddItem()
	{
		GameSystem<QuestSystem>.Instance().Rewarded += QuestSystem_Rewarded;
		GameSystem<QuestSystem>.Instance().GetQuestState(_questId, delegate(Shared.Quest.QuestState state)
		{
			if (state == Shared.Quest.QuestState.Finished || state == Shared.Quest.QuestState.NotActivated)
			{
				CallComplete();
			}
		});
	}

	public override void OnRemoveItem()
	{
		GameSystem<QuestSystem>.Instance().Rewarded -= QuestSystem_Rewarded;
	}

	private void QuestSystem_Rewarded(QuestRewardResults result)
	{
		if (result.QuestId == _questId)
		{
			CallComplete();
		}
	}
}
