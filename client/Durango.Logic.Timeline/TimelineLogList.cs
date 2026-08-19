using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;

namespace Durango.Logic.Timeline;

public class TimelineLogList
{
	private class TimelineLogSet
	{
		public TimelineLog[] logs;

		public int current_page;

		public int max_page;

		public int page_size;
	}

	private const int PageSize = 20;

	private static TimelineOption? _option;

	private readonly List<TimelineLogBuilder> _logs = new List<TimelineLogBuilder>();

	private string _id;

	private TimelineType _type;

	private string _category;

	private int _pageIndex;

	private int _maxPage;

	private bool _isLoading;

	public List<TimelineLogBuilder> Logs => _logs;

	public event Action Updated;

	public void Set(string id, TimelineType type, string category)
	{
		_id = id;
		_type = type;
		_category = category;
		_isLoading = false;
		Clear();
	}

	public void Clear()
	{
		_logs.Clear();
		_pageIndex = 0;
		_maxPage = -1;
	}

	public void RequestNextPage()
	{
		if (_isLoading || !HasNextPage())
		{
			return;
		}
		TimelineType type = _type;
		string id = _id;
		Action<TimelineLogSet> onResult = delegate(TimelineLogSet logs)
		{
			if (logs == null)
			{
				_isLoading = false;
				_maxPage = _pageIndex;
				if (this.Updated != null)
				{
					this.Updated();
				}
			}
			else if (!(id != _id) && type == _type && _pageIndex == logs.current_page)
			{
				_isLoading = false;
				_pageIndex++;
				_maxPage = logs.max_page;
				int i = 0;
				for (int size = KUtility.GetSize(logs.logs); i < size; i++)
				{
					_logs.Add(new TimelineLogBuilder(logs.logs[i]));
				}
				if (this.Updated != null)
				{
					this.Updated();
				}
			}
		};
		_isLoading = true;
		switch (_type)
		{
		case TimelineType.Entity:
			RequestEntityLogs(_id, _pageIndex, 20, onResult);
			break;
		case TimelineType.Clan:
			RequestClanLogs(_id, _pageIndex, 20, onResult);
			break;
		case TimelineType.Estate:
			RequestEstateLogs(_id, _pageIndex, 20, onResult);
			break;
		case TimelineType.ClanEstate:
			RequestClanEstateLogs(_id, _pageIndex, 20, onResult);
			break;
		case TimelineType.Player:
			RequestPlayerLogs(_id, _pageIndex, _category, 20, onResult);
			break;
		}
	}

	public bool HasNextPage()
	{
		return _maxPage < 0 || _pageIndex < _maxPage;
	}

	private static void RequestEntityLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)
	{
		string url = $"{GameManager.GatewayUrl}/entities/{id}/timeline?page={page}&page_size={pageSize}";
		Http.RequestYml(url, onResult);
	}

	private static void RequestClanLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)
	{
		string url = $"{GameManager.GatewayUrl}/clans/{id}/timeline?page={page}&page_size={pageSize}";
		Http.RequestYml(url, onResult);
	}

	private static void RequestEstateLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)
	{
		string url = $"{GameManager.GatewayUrl}/entities/{id}/estate_timeline?page={page}&page_size={pageSize}";
		Http.RequestYml(url, onResult);
	}

	private static void RequestClanEstateLogs(string id, int page, int pageSize, [NotNull] Action<TimelineLogSet> onResult)
	{
		string url = $"{GameManager.GatewayUrl}/clans/{id}/territory_timeline?page={page}&page_size={pageSize}";
		Http.RequestYml(url, onResult);
	}

	private static void RequestPlayerLogs(string id, int page, string category, int pageSize, [NotNull] Action<TimelineLogSet> onResult)
	{
		string text = $"{GameManager.GatewayUrl}/entities/{id}/play_timeline?page={page}&page_size={pageSize}";
		if (!string.IsNullOrEmpty(category))
		{
			text = text + "&category=" + category;
		}
		Http.RequestYml(text, onResult);
	}

	public static void GetOption([NotNull] Action<TimelineOption> onResult)
	{
		TimelineOption? option = _option;
		if (!option.HasValue)
		{
			Connections.Frontend.Send(default(GetTimelineOption)).On(delegate(TimelineOption msg, PacketHeader header)
			{
				_option = msg;
				onResult(msg);
			});
		}
		else
		{
			onResult(_option.Value);
		}
	}

	public static void SetOption(TimelineOption option, Action<bool> onResult)
	{
		TimelineOption? prev = _option;
		_option = option;
		Connections.Frontend.Send(new SetTimelineOption
		{
			EstateNotification = option.EstateNotification
		}).All(delegate(Packet packet)
		{
			bool flag = Packet.IsSuccess(packet);
			if (!flag)
			{
				_option = prev;
			}
			if (onResult != null)
			{
				onResult(flag);
			}
		});
	}
}
