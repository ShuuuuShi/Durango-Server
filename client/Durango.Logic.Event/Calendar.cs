using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Attendance;
using UnityEngine;

namespace Durango.Logic.Event;

public class Calendar
{
	private const double AttendTimeMargin = 30.0;

	private const double RewardInterval = 86400.0;

	public readonly CategoryType Category;

	public readonly string TitleName;

	public readonly string TabName;

	public readonly string CharacterImageName;

	public readonly string BackgroundImageName;

	public readonly int TodayAttendanceReward;

	public readonly int RestorableDays;

	public readonly double Since;

	public readonly double Until;

	public bool CanTakeAppendix;

	private double _nextAttendTime;

	private List<CalenderReward> _rewards;

	private readonly List<CalenderReward> _appendices = new List<CalenderReward>();

	private bool _isLoadingRewards;

	private Action<List<CalenderReward>, List<CalenderReward>> _callbacks;

	private bool RewardInitialized => _rewards != null;

	public Calendar(CategoryType category, TodayAttendanceReward msg)
	{
		Category = category;
		TitleName = msg.Name;
		TabName = msg.ShortName;
		CharacterImageName = msg.Image;
		BackgroundImageName = msg.BgImage;
		TodayAttendanceReward = msg.RewardNumber;
		_nextAttendTime = msg.NextAttendTime;
		Since = msg.Since;
		Until = msg.Until;
		RestorableDays = msg.RestorableDays;
		CanTakeAppendix = msg.AppendixRewardable;
	}

	public bool HasTodayReward()
	{
		if (_rewards == null)
		{
			return TodayAttendanceReward != -1 || CanTakeAppendix;
		}
		for (int i = 0; i < _rewards.Count; i++)
		{
			if (_rewards[i].State == RewardState.Ready)
			{
				return true;
			}
		}
		return CanTakeAppendix;
	}

	public void GetRewards([NotNull] Action<List<CalenderReward>, List<CalenderReward>> onResult)
	{
		if (RewardInitialized)
		{
			onResult(_rewards, _appendices);
			return;
		}
		_callbacks = (Action<List<CalenderReward>, List<CalenderReward>>)Delegate.Combine(_callbacks, onResult);
		if (!_isLoadingRewards)
		{
			_isLoadingRewards = true;
			GameSystem<EventSystem>.Instance().RequestAttendanceRewards(Category, OnAttendanceRewards);
		}
	}

	private void OnAttendanceRewards(AttendanceRewards rewards)
	{
		_isLoadingRewards = false;
		if (rewards.Category == Category)
		{
			InitRewards(rewards.Rewards);
			InitAppendices(rewards.Appendices);
			RunRewardsTimer();
			if (_callbacks != null)
			{
				_callbacks(_rewards, _appendices);
			}
			_callbacks = null;
		}
	}

	public void TakeTodayAttendanceReward(bool restore, Action<CalenderReward> onResult)
	{
		int index = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_rewards); i < size; i++)
		{
			RewardState rewardState = ((!restore) ? RewardState.Ready : RewardState.Restorable);
			if (_rewards[i].State == rewardState)
			{
				index = i;
				break;
			}
		}
		if (index == -1)
		{
			return;
		}
		GameSystem<EventSystem>.Instance().TakeTodayAttendanceReward(Category, index, restore, delegate(bool ok)
		{
			if (ok)
			{
				CalenderReward calenderReward = _rewards[index];
				calenderReward.State = RewardState.Completed;
				_rewards[index] = calenderReward;
				RefreshAppendicesStates();
				if (onResult != null)
				{
					onResult(calenderReward);
				}
			}
		});
	}

	public void TakeAppendixReward(CalenderReward calenderReward, Action onResult)
	{
		if (!CanTakeAppendix)
		{
			return;
		}
		int index = calenderReward.Index;
		GameSystem<EventSystem>.Instance().TakeAppendixReward(Category, index, delegate(bool ok)
		{
			if (ok)
			{
				CalenderReward value = _appendices[index];
				value.State = RewardState.Completed;
				_appendices[index] = value;
				RefreshAppendicesStates();
				if (onResult != null)
				{
					onResult();
				}
			}
		});
	}

	private void InitRewards(AttendanceReward[] rewards)
	{
		_rewards = new List<CalenderReward>();
		int i = 0;
		for (int size = KUtility.GetSize(rewards); i < size; i++)
		{
			CalenderReward item = new CalenderReward(rewards[i], i);
			if (item.State == RewardState.None)
			{
				if (i <= TodayAttendanceReward)
				{
					item.State = RewardState.Ready;
				}
				else if (i <= RestorableDays)
				{
					item.State = RewardState.Restorable;
				}
			}
			_rewards.Add(item);
		}
	}

	private void InitAppendices(AttendanceReward[] appendices)
	{
		_appendices.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(appendices); i < size; i++)
		{
			CalenderReward item = new CalenderReward(appendices[i], i);
			_appendices.Add(item);
		}
		RefreshAppendicesStates();
	}

	private void RefreshAppendicesStates()
	{
		bool flag = _appendices.Count == 0 || _appendices.Any((CalenderReward o) => o.State == RewardState.Completed);
		if (flag)
		{
			for (int i = 0; i < _appendices.Count; i++)
			{
				CalenderReward value = _appendices[i];
				value.State = RewardState.Completed;
				_appendices[i] = value;
			}
		}
		CanTakeAppendix = CountDays(RewardState.Completed) == _rewards.Count && !flag;
		if (!CanTakeAppendix)
		{
			return;
		}
		for (int j = 0; j < _appendices.Count; j++)
		{
			CalenderReward value2 = _appendices[j];
			if (value2.State == RewardState.None)
			{
				value2.State = RewardState.Ready;
				_appendices[j] = value2;
			}
		}
	}

	private void RunRewardsTimer()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		if (predictedServerTime > _nextAttendTime)
		{
			double d = (predictedServerTime - _nextAttendTime) / 86400.0 + 1.0;
			_nextAttendTime += Math.Floor(d) * 86400.0;
		}
		double num = _nextAttendTime - predictedServerTime + 30.0;
		DelayedFunction delayedFunction = new DelayedFunction(ResetRewards, new WaitForSeconds((float)num));
		delayedFunction.Call(GameSystem<EventSystem>.Instance());
	}

	private void ResetRewards()
	{
		_rewards = null;
		_appendices.Clear();
		_nextAttendTime += 86370.0;
	}

	public RewardState GetRewardState(int index)
	{
		if (_rewards == null)
		{
			return RewardState.Invalid;
		}
		return (index >= _rewards.Count) ? RewardState.None : _rewards[index].State;
	}

	public int CountDays(RewardState rewardState)
	{
		if (_rewards == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < _rewards.Count; i++)
		{
			if (_rewards[i].State == rewardState)
			{
				num++;
			}
		}
		return num;
	}
}
