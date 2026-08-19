using System;
using System.Collections.Generic;
using Building_;
using K1Network;
using L10N;
using Messages;
using Player;
using Shared.System;
using TimerData;
using Yaml;

public class TimelineLogSystem : GameSystem<TimelineLogSystem>
{
	public enum TimelineType
	{
		Entity,
		Clan
	}

	public class LogInfo
	{
		public bool IsNegative { get; private set; }

		public Player.PlayerInfo PlayerInfo { get; private set; }

		public Building_.Blueprint Blueprint { get; private set; }

		public string Text { get; private set; }

		public double Time { get; private set; }

		public LogInfo()
		{
			Text = string.Empty;
		}

		public void Set(bool negative, Player.PlayerInfo playerInfo, Building_.Blueprint blueprint, string text, double time)
		{
			IsNegative = negative;
			PlayerInfo = playerInfo;
			Blueprint = blueprint;
			Text = text;
			Time = time;
		}
	}

	private delegate string[] TimelineLogParameterHandler(string[] parameters);

	private const int MaxLogCountPerPage = 30;

	private readonly Dictionary<TimelineEvent, TimelineLogParameterHandler> _parameterHandlers = new Dictionary<TimelineEvent, TimelineLogParameterHandler>();

	private ulong _timelineId;

	private TimelineType _type;

	private int _category;

	private readonly List<LogInfo> _listLogs = new List<LogInfo>();

	private bool _isLoading;

	private int _remainUpdateCount;

	public bool LogNotFound { get; private set; }

	public int ValidLogCount { get; private set; }

	public IList<LogInfo> Logs => _listLogs;

	public event Action LogUpdated;

	private void Awake()
	{
		Connections.Frontend.On<Messages.TimelineLog>(OnTimelineLog);
		_parameterHandlers.Add(TimelineEvent.HelpPostprocess, HandleHelpPostprocess);
		_parameterHandlers.Add(TimelineEvent.ClanFundUsed, HandleClanFundUsed);
	}

	public void Clear()
	{
		ValidLogCount = 0;
		LogNotFound = false;
	}

	public void SetTimeline(ulong timelineId, TimelineType type, int category = 0)
	{
		_timelineId = timelineId;
		_type = type;
		_category = category;
		_isLoading = false;
		Clear();
		RequestMoreLogs();
	}

	public void RequestMoreLogs()
	{
		if (_isLoading)
		{
			return;
		}
		_isLoading = true;
		int num = ValidLogCount / 30;
		string url = string.Format("{0}{1}/{2}/timeline?page={3}&page_size={4}&category={5}", KSingleton<GameManager>.Instance().GatewayUrl, (_type != 0) ? "clans" : "entities", _timelineId, num, 30, _category);
		KUtility.RequestYml(url, delegate(TimelineLogSet logSet)
		{
			_isLoading = false;
			if (logSet == null || logSet.logs == null)
			{
				LogNotFound = true;
				if (this.LogUpdated != null)
				{
					this.LogUpdated();
				}
			}
			else
			{
				Messages.TimelineLog[] array = new Messages.TimelineLog[logSet.logs.Length];
				for (int i = 0; i < logSet.logs.Length; i++)
				{
					ref Messages.TimelineLog reference = ref array[i];
					reference = logSet.logs[i].ToMessage();
				}
				RefreshPage(logSet.current_page, array);
			}
		});
	}

	private void SetLogCount(int logCount)
	{
		while (_listLogs.Count < logCount)
		{
			_listLogs.Add(new LogInfo());
		}
		if (ValidLogCount < logCount)
		{
			ValidLogCount = logCount;
		}
		LogNotFound = ValidLogCount == 0;
	}

	private void RefreshPage(int page, Messages.TimelineLog[] timelineLogs)
	{
		int num = page * 30;
		int size = KUtility.GetSize(timelineLogs);
		SetLogCount(num + size);
		_remainUpdateCount = size;
		for (int i = 0; i < size; i++)
		{
			LogInfo logInfo = _listLogs[num + i];
			Messages.TimelineLog msg = timelineLogs[i];
			ArtifactDigest? targetArtifact = msg.TargetArtifact;
			Building_.Blueprint blueprint = ((!targetArtifact.HasValue) ? null : GameSystem<RecipeSystem>.Instance().GetBlueprint(msg.TargetArtifact.Value.PrototypeId));
			ulong? agentEntityId = msg.AgentEntityId;
			if (!agentEntityId.HasValue)
			{
				ulong? targetEntityId = msg.TargetEntityId;
				if (!targetEntityId.HasValue)
				{
					UpdateLog(logInfo, msg, null, null, blueprint);
					continue;
				}
			}
			ulong[] entityIds = new ulong[2]
			{
				msg.AgentEntityId.GetValueOrDefault(0uL),
				msg.TargetEntityId.GetValueOrDefault(0uL)
			};
			KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfos(entityIds, delegate(Player.PlayerInfo[] infos)
			{
				UpdateLog(logInfo, msg, infos[0], infos[1], blueprint);
			});
		}
	}

	private void UpdateLog(LogInfo logInfo, Messages.TimelineLog msg, Player.PlayerInfo playerInfo, Player.PlayerInfo targetInfo, Building_.Blueprint blueprint)
	{
		logInfo.Set(msg.Type == TimelineEvent.Destroy, playerInfo, blueprint, TimelineLogToString(msg.Type, msg.Params, playerInfo, targetInfo, blueprint), msg.At);
		if (--_remainUpdateCount <= 0 && this.LogUpdated != null)
		{
			this.LogUpdated();
		}
	}

	private void OnTimelineLog(Messages.TimelineLog msg, PacketHeader header)
	{
		ArtifactDigest? targetArtifact = msg.TargetArtifact;
		Building_.Blueprint blueprint = ((!targetArtifact.HasValue) ? null : GameSystem<RecipeSystem>.Instance().GetBlueprint(msg.TargetArtifact.Value.PrototypeId));
		ulong? agentEntityId = msg.AgentEntityId;
		if (!agentEntityId.HasValue)
		{
			ulong? targetEntityId = msg.TargetEntityId;
			if (!targetEntityId.HasValue)
			{
				string text = TimelineLogToString(msg.Type, msg.Params, null, null, blueprint);
				if (!string.IsNullOrEmpty(text))
				{
					UIManager.Popup.Alarm.ShowAlarm(text, "alarm_memo", 4f);
					GameSystem<SocialSystem>.Instance().AddSystemChat(text, string.Empty, remainColor: true);
				}
				return;
			}
		}
		ulong[] entityIds = new ulong[2]
		{
			msg.AgentEntityId.GetValueOrDefault(0uL),
			msg.TargetEntityId.GetValueOrDefault(0uL)
		};
		KSingleton<PlayerInfoManager>.Instance().RequestPlayerInfos(entityIds, delegate(Player.PlayerInfo[] infos)
		{
			string text2 = TimelineLogToString(msg.Type, msg.Params, infos[0], infos[1], blueprint);
			if (!string.IsNullOrEmpty(text2))
			{
				UIManager.Popup.Alarm.ShowAlarm(text2, "alarm_memo", 4f);
				GameSystem<SocialSystem>.Instance().AddSystemChat(text2, string.Empty, remainColor: true);
			}
		}, useOldCache: true);
	}

	private string TimelineLogToString(TimelineEvent eventType, string[] param, Player.PlayerInfo playerInfo, Player.PlayerInfo targetInfo, Building_.Blueprint blueprint)
	{
		TimelineLogParameterHandler timelineLogParameterHandler = _parameterHandlers.Get(eventType);
		if (timelineLogParameterHandler != null)
		{
			param = timelineLogParameterHandler(param);
		}
		string text = "#timeline_" + eventType;
		List<string> list = new List<string>();
		if (playerInfo != null && playerInfo.Valid)
		{
			list.Add(playerInfo.Name);
		}
		else
		{
			text += "_agent";
		}
		if (targetInfo != null && targetInfo.Valid)
		{
			list.Add(targetInfo.Name);
		}
		else if (blueprint != null)
		{
			list.Add(blueprint.Name);
		}
		else
		{
			text += "_target";
		}
		list.AddRange(param);
		return GetTimelineLocalizeFormat(text, list.ToArray());
	}

	private static string GetTimelineLocalizeFormat(string key, string[] param)
	{
		try
		{
			return LocalizeSystem.Format(key, param);
		}
		catch (Exception ex)
		{
			return ex.Message;
		}
	}

	private static string[] HandleHelpPostprocess(string[] parameters)
	{
		if (parameters == null || parameters.Length == 0)
		{
			return new string[0];
		}
		if (!double.TryParse(parameters[0], out var result))
		{
			return new string[0];
		}
		string text = TimerSystem.TimeToString(result, TimePeriod.Sec, 2);
		return new string[1] { text };
	}

	private static string[] HandleClanFundUsed(string[] parameters)
	{
		if (parameters == null || parameters.Length == 0)
		{
			return new string[0];
		}
		if (!int.TryParse(parameters[1], out var result))
		{
			return new string[0];
		}
		string text = string.Empty;
		switch (result)
		{
		case 1:
			text = T._("영토 선언");
			break;
		case 0:
			text = T._("연구비");
			break;
		case 2:
			text = T._("선전포고");
			break;
		}
		return new string[2]
		{
			parameters[0],
			text
		};
	}
}
