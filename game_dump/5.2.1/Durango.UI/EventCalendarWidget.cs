using System;
using System.Collections.Generic;
using Durango.Logic.Event;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class EventCalendarWidget : CalendarWidget, IUIInitializable
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private AppendixRewardWidget _appendixRewardWidget;

	[SerializeField]
	private KGridScrollView _scrollView;

	[SerializeField]
	private GameObject _calendarTouchBox;

	[SerializeField]
	private UILabel _bottomLabel;

	[SerializeField]
	private SelectableButton _restoreButton;

	private Calendar _calendar;

	void IUIInitializable.Init()
	{
		alpha = 0f;
		UpdateRestoreButtonText();
		GameSystem<InventorySystem>.Instance().WalletUpdated += UpdateRestoreButtonText;
		SelectableButton restoreButton = _restoreButton;
		restoreButton.Clicked = (Action)Delegate.Combine(restoreButton.Clicked, (Action)delegate
		{
			TakeTodayAttendanceReward(restore: true);
		});
		UIEventListener uIEventListener = UIEventListener.Get(_calendarTouchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnTouchCalendar));
		_appendixRewardWidget.RewardNodeClicked += AppendixRewardNodeClicked;
	}

	private void UpdateRestoreButtonText()
	{
		if (base.enabled)
		{
			if (InventorySystem.Wallet.GetVoucherCount("voucher_event_attendance") > 0)
			{
				_restoreButton.Text = Inventory.ToVoucherButtonText(T._("재출석"), 1, "voucher_event_attendance");
				return;
			}
			RestoreCost restoreCost = Yaml.Util.Singleton<Constants>.Instance.Attendance.RestoreCost;
			_restoreButton.Text = Inventory.ToCurrencyButtonText(T._("재출석"), restoreCost.Amount, restoreCost.Currency);
		}
	}

	public override void Set(Calendar calendar)
	{
		_calendar = calendar;
		_titleLabel.text = Times.GetDateString(calendar.Since, calendar.Until);
		calendar.GetRewards(SetRewards);
	}

	private void SetRewards([NotNull] List<CalenderReward> rewards, [NotNull] List<CalenderReward> appendices)
	{
		string text = T._("[84847D]이벤트 기간 출석[-] {0}일", _calendar.CountDays(RewardState.Completed));
		int num = _calendar.CountDays(RewardState.Restorable);
		string text2 = T._("[84847D]재출석 가능[-] [FFD85B]{0}일[-]", num);
		_bottomLabel.text = ((!UIManager.IsPortraitWidget(base.gameObject)) ? (text + " / " + text2) : (text + "\n" + text2));
		alpha = 1f;
		bool active = false;
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(rewards); i < size; i++)
		{
			CalenderReward reward = rewards[i];
			nodes.GetNext().GetComponent<CalendarNodeWidget>().Set(reward, highlight: false);
			if (reward.State == RewardState.Ready)
			{
				active = true;
			}
		}
		nodes.EndLoad();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.ResetPosition();
		_calendarTouchBox.gameObject.SetActive(active);
		_appendixRewardWidget.Set(appendices);
		_restoreButton.Disabled = num <= 0;
	}

	public override CalendarNodeWidget GetNodeWidget(int index)
	{
		if (index < 0 || _scrollView.Nodes.Count <= index)
		{
			return null;
		}
		return _scrollView.Nodes.Get<CalendarNodeWidget>(index);
	}

	private void OnTouchCalendar(GameObject obj)
	{
		TakeTodayAttendanceReward(restore: false);
	}

	private void TakeTodayAttendanceReward(bool restore)
	{
		TakeTodayAtendanceReward(_calendar, restore, delegate
		{
			if (_calendar != null)
			{
				_calendar.GetRewards(SetRewards);
			}
		});
	}

	private void AppendixRewardNodeClicked(CalenderReward calendarReward)
	{
		if (calendarReward.State != RewardState.Ready)
		{
			return;
		}
		MessageBox messageBox = UIManager.MessageBox;
		string text = null;
		string text2 = null;
		if (calendarReward.Item != null)
		{
			text = calendarReward.Item.Name;
			text2 = calendarReward.ItemCount.ToString();
		}
		else if (calendarReward.Money.Currency != Currency.Invalid && calendarReward.Money.Amount > 0)
		{
			text = calendarReward.Money.Currency.GetName();
			text2 = calendarReward.Money.Amount.ToString();
		}
		else if (calendarReward.Voucher.HasValue)
		{
			Voucher voucher = SingletonDict<string, Voucher>.Get(calendarReward.Voucher.Value.VoucherId);
			if (voucher.IsValid())
			{
				_ = calendarReward.Voucher.Value.Count;
				_ = 0;
			}
			text = voucher.Name;
			text2 = calendarReward.Voucher.Value.Count.ToString();
		}
		messageBox.Show(T._("최종 보상으로 <em>{0} {1}개</em>를 선택하시겠습니까?", text, text2), T._("[icon=icon_make_alert] 한번 선택한 보상은 되돌릴 수 없습니다."), delegate(bool ok)
		{
			if (ok)
			{
				_calendar.TakeAppendixReward(calendarReward, delegate
				{
					ShowRewardAlarm(calendarReward);
					if (_calendar != null)
					{
						_calendar.GetRewards(SetRewards);
					}
				});
			}
		});
	}
}
