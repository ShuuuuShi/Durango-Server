using System.Collections.Generic;
using UnityEngine;

public class TimelineLogContainer : MonoBehaviour
{
	[SerializeField]
	private KScrollView _logView;

	[SerializeField]
	private GameObject _noData;

	private void OnEnable()
	{
		GameSystem<TimelineLogSystem>.Instance().LogUpdated += TimelineLogSystem_LogUpdated;
	}

	private void OnDisable()
	{
		GameSystem<TimelineLogSystem>.Instance().LogUpdated -= TimelineLogSystem_LogUpdated;
	}

	public void Init()
	{
		_logView.Nodes.Init(null);
		_logView.DragFinishedOnLast = delegate
		{
			GameSystem<TimelineLogSystem>.Instance().RequestMoreLogs();
		};
	}

	public void Clear()
	{
		GameSystem<TimelineLogSystem>.Instance().Clear();
		RefreshLogs();
	}

	public void SetTimeline(ulong entityId, TimelineLogSystem.TimelineType type)
	{
		GameSystem<TimelineLogSystem>.Instance().SetTimeline(entityId, type);
		RefreshLogs();
	}

	private void RefreshLogs()
	{
		TimelineLogSystem timelineLogSystem = GameSystem<TimelineLogSystem>.Instance();
		IList<TimelineLogSystem.LogInfo> logs = timelineLogSystem.Logs;
		int width = _logView.Widget.width;
		_logView.Nodes.Set(timelineLogSystem.ValidLogCount);
		for (int i = 0; i < _logView.Nodes.Count; i++)
		{
			TimelineLog component = _logView.Nodes[i].GetComponent<TimelineLog>();
			component.SetWidth(width);
			component.SetLog(logs[i]);
		}
		_noData.gameObject.SetActive(timelineLogSystem.LogNotFound);
		_logView.Reposition();
	}

	private void TimelineLogSystem_LogUpdated()
	{
		RefreshLogs();
	}

	private void OnPortraitMode(bool isPortrait)
	{
		_logView.RefreshBox();
		int width = _logView.Widget.width;
		for (int i = 0; i < _logView.Nodes.Count; i++)
		{
			TimelineLog component = _logView.Nodes[i].GetComponent<TimelineLog>();
			component.SetWidth(width);
		}
	}
}
