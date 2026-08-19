using Durango.Logic.Timeline;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class TimelineLogContainer : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KInfiniteScrollView _logView;

	[SerializeField]
	private GameObject _noData;

	private KInfiniteScrollView.View<TimelineLogBuilder, TimelineLog> _view;

	private readonly TimelineLogList _logList = new TimelineLogList();

	void IUIInitializable.Init()
	{
		_logList.Updated += RefreshLogs;
		_view = _logView.Initialize(delegate(TimelineLog comp, TimelineLogBuilder data)
		{
			comp.SetLog(data);
		});
		_view.SetList(_logList.Logs);
		_logView.DragFinishedOnLast += delegate
		{
			_logList.RequestNextPage();
		};
	}

	public void Clear()
	{
		_logList.Clear();
		RefreshLogs();
	}

	public void SetTimeline(string entityId, TimelineType type, string category = null)
	{
		_logList.Set(entityId, type, category);
		_logView.ResetPosition();
		_logList.RequestNextPage();
		_noData.gameObject.SetActive(value: false);
		UIManager.Popup.LoadingRing.AttachToWidget(_noData, base.gameObject);
	}

	private void RefreshLogs()
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(base.gameObject);
		_logView.UpdateLayout();
		_noData.gameObject.SetActive(_view.Count == 0);
	}
}
