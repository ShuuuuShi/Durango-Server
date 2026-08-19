using System;
using Durango.Network;
using Durango.UI;
using Durango.UI.Popup;
using L10N;
using Messages;

public static class LowEnergyWarning
{
	public enum Result
	{
		IgnoreWarning,
		EatFoodPopupOpened,
		Cancel
	}

	public static void Show(EnergyWarning msg, PacketHeader header, Action<Result> onReply)
	{
		UIManager.Popup.Tooltip<ConfirmPopup>().AddButton(new MessageBox.Button(T._("실행")), delegate
		{
			Connection frontend3 = Connections.Frontend;
			Confirm msg4 = new Confirm
			{
				Confirmation = true
			};
			uint replyOf3 = header.ReplyOf;
			frontend3.Send(msg4, noReply: false, replyOf3);
			onReply(Result.IgnoreWarning);
		}).AddButton(T._("음식 먹기"), delegate
		{
			Connection frontend2 = Connections.Frontend;
			Confirm msg3 = new Confirm
			{
				Confirmation = false
			};
			uint replyOf2 = header.ReplyOf;
			frontend2.Send(msg3, noReply: false, replyOf2);
			onReply(Result.EatFoodPopupOpened);
			UIManager.Inventory.OpenEatFoodPopup();
		})
			.OnCancel(delegate
			{
				Connection frontend = Connections.Frontend;
				Confirm msg2 = new Confirm
				{
					Confirmation = false
				};
				uint replyOf = header.ReplyOf;
				frontend.Send(msg2, noReply: false, replyOf);
				onReply(Result.EatFoodPopupOpened);
			})
			.Show(T._("에너지가 모자라는 상태로 이 행동을 하면 건강이 줄어듭니다."), 3600f);
	}

	public static void Hide()
	{
		UIManager.Popup.Tooltip<ConfirmPopup>().Hide();
	}
}
