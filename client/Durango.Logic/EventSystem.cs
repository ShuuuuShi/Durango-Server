using System;
using System.Collections.Generic;
using Durango.Logic.Event;
using Durango.Network;
using Messages;
using Shared.Attendance;

namespace Durango.Logic;

public class EventSystem : GameSystem<EventSystem>
{
	public Calendar[] Calendars { get; private set; }

	public event Action CalendarUpdated;

	private void Start()
	{
		GameSystem<MenuSystem>.Instance().EnableMenu(MenuType.Event, enable: false);
		Connections.Frontend.On<TodayAttendanceRewards>(OnTodayAttendanceRewards);
	}

	private void OnCalendarsUpdated()
	{
		GameSystem<MenuSystem>.Instance().EnableMenu(MenuType.Event, KUtility.GetSize(Calendars) > 0);
		if (this.CalendarUpdated != null)
		{
			this.CalendarUpdated();
		}
	}

	private void OnTodayAttendanceRewards(TodayAttendanceRewards msg, PacketHeader header)
	{
		if (msg.Rewards == null)
		{
			Calendars = null;
		}
		else
		{
			Calendars = new Calendar[msg.Rewards.Count];
			int num = 0;
			foreach (KeyValuePair<CategoryType, TodayAttendanceReward> reward in msg.Rewards)
			{
				Calendars[num] = new Calendar(reward.Key, reward.Value);
				num++;
			}
		}
		OnCalendarsUpdated();
	}

	public void RequestAttendanceRewards(CategoryType category, Action<AttendanceRewards> onResult)
	{
		Connections.Frontend.Send(new GetAttendanceRewards
		{
			Category = category
		}).On(delegate(AttendanceRewards msg, PacketHeader header)
		{
			if (onResult != null)
			{
				onResult(msg);
			}
			OnCalendarsUpdated();
		});
	}

	public void TakeTodayAttendanceReward(CategoryType category, int index, bool restore, Action<bool> onResult)
	{
		Connections.Frontend.Send(new GiveAttendanceReward
		{
			Category = category,
			RewardNumber = index,
			IsRestore = restore
		}).All(delegate(Packet packet)
		{
			bool obj = false;
			uint typeCode = packet.Header.TypeCode;
			if (typeCode == 1231)
			{
				obj = true;
			}
			if (onResult != null)
			{
				onResult(obj);
			}
			OnCalendarsUpdated();
		});
	}

	public void TakeAppendixReward(CategoryType category, int index, Action<bool> onResult)
	{
		Connections.Frontend.Send(new GiveAttendanceAppendix
		{
			Category = category,
			SelectedReward = index
		}).All(delegate(Packet packet)
		{
			bool obj = false;
			uint typeCode = packet.Header.TypeCode;
			if (typeCode == 1231)
			{
				obj = true;
			}
			if (onResult != null)
			{
				onResult(obj);
			}
			OnCalendarsUpdated();
		});
	}

	public RewardState GetRewardState(CategoryType category, int index)
	{
		if (Calendars == null)
		{
			return RewardState.Invalid;
		}
		if (Calendars != null)
		{
			Calendar[] calendars = Calendars;
			foreach (Calendar calendar in calendars)
			{
				if (calendar.Category == category)
				{
					return calendar.GetRewardState(index);
				}
			}
		}
		return RewardState.None;
	}
}
