using System;
using System.Collections.Generic;
using Durango.Logic.Event;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class MontlyCalendarWidget : CalendarWidget, IUIInitializable
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subTitleLabel;

	[SerializeField]
	private KGridScrollView _scrollView;

	[SerializeField]
	private GameObject _touchBox;

	private Calendar _calendar;

	void IUIInitializable.Init()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_touchBox);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickTouchBox));
		alpha = 0f;
	}

	public override void Set(Calendar calendar)
	{
		_calendar = calendar;
		_titleLabel.text = calendar.TitleName;
		_subTitleLabel.text = Times.GetDateString(calendar.Since, calendar.Until);
		calendar.GetRewards(SetRewards);
	}

	private void SetRewards([NotNull] List<CalenderReward> rewards, [NotNull] List<CalenderReward> appendices)
	{
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
		_touchBox.gameObject.SetActive(active);
	}

	private void OnClickTouchBox(GameObject obj)
	{
		TakeTodayAtendanceReward(_calendar, restore: false, delegate
		{
			if (_calendar != null)
			{
				_calendar.GetRewards(SetRewards);
			}
		});
	}

	public override CalendarNodeWidget GetNodeWidget(int index)
	{
		if (index < 0 || _scrollView.Nodes.Count <= index)
		{
			return null;
		}
		return _scrollView.Nodes.Get<CalendarNodeWidget>(index);
	}
}
