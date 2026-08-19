using Durango.Logic.PlayGuide;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;

namespace Durango.Logic.WarpRush;

public class EntryTodoCollection : Durango.Logic.PlayGuide.ToDoCollection
{
	public EntryTodoCollection()
	{
		Icon = "todo_icon_npc_Brian";
		base.Key = WarpRushSystem.GetEntryCollectionKey();
		base.IsSubIconRotational = true;
	}

	public override void OnAddItem()
	{
		base.OnAddItem();
		GameSystem<WarpRushSystem>.Instance().EntreeInfoUpdated += WarpRushSystem_EntreeInfoUpdated;
	}

	public override void OnRemoveItem()
	{
		base.OnRemoveItem();
		GameSystem<WarpRushSystem>.Instance().EntreeInfoUpdated -= WarpRushSystem_EntreeInfoUpdated;
	}

	private void WarpRushSystem_EntreeInfoUpdated(S02EntreeInfo _)
	{
		GameSystem<ToDoListSystem>.Instance().SetUpdated(this, textOnly: true);
	}

	public override bool IsMessageOnly()
	{
		return false;
	}

	public override SyncString GetMessage()
	{
		return new SyncString(delegate(out string text, out float period)
		{
			S02EntreeInfo entreeInfo = GameSystem<WarpRushSystem>.Instance().EntreeInfo;
			int warpRushEntryCount = OptionSystem.GetWarpRushEntryCount();
			bool flag = OptionSystem.GetS02WaitingQueueMin() <= entreeInfo.QueueCount;
			double num = entreeInfo.DepartureAt - Connections.Frontend.GetPredictedServerTime();
			string arg = T._("난투섬 입장을 위한\n대기열에 등록했습니다.");
			string arg2 = T._("대기 인원 <em>{0}</em>[FFFFFF7F]/{1}[-]", entreeInfo.QueueCount, warpRushEntryCount);
			string arg3 = string.Format("{0} {1}", T._("남은 시간"), (!flag) ? " - " : TimedeltaFormatter.Format(num));
			text = $"\n[d4cebe][size=20]{arg}[/size][-]<br>9</br>[c][size=24]{arg2}[/size][/c]<br>5</br>[ffffff7f]{arg3}[-]";
			int num2 = TimedeltaFormatter.CurrentMinUnit();
			period = (float)(num % (double)num2);
		});
	}

	public override string GetSubIcon()
	{
		return "loading_waiting_line";
	}

	public override Detail? GetDetail()
	{
		Detail value = default(Detail);
		value.CommonText = GetMessage();
		value.CommonTextAlignment = NGUIText.Alignment.Center;
		value.ButtonText = T._("취소");
		value.ButtonClicked = GameSystem<WarpRushSystem>.Instance().DequeueWarpRushEntry;
		value.ButtonStyle = PresetButton.Style.Border;
		return value;
	}
}
