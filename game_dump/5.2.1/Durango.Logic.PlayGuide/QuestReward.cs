using Messages;
using Shared.Quest;

namespace Durango.Logic.PlayGuide;

public class QuestReward : FlowCondition
{
	private readonly string _questId;

	public QuestReward(string param)
	{
		_questId = param;
	}

	protected override void OnRegister()
	{
		GameSystem<QuestSystem>.Instance().Rewarded += QuestSystem_Rewarded;
		GameSystem<QuestSystem>.Instance().GetQuestState(_questId, delegate(Shared.Quest.QuestState state)
		{
			if (state == Shared.Quest.QuestState.Finished)
			{
				Interrupt();
			}
		});
	}

	protected override void OnUnregister()
	{
		GameSystem<QuestSystem>.Instance().Rewarded -= QuestSystem_Rewarded;
	}

	private void QuestSystem_Rewarded(QuestRewardResults result)
	{
		if (result.QuestId == _questId)
		{
			Interrupt();
		}
	}
}
