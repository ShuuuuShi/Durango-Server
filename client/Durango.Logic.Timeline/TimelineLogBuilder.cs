using System;
using System.Collections.Generic;
using Building;
using Durango.Player;
using Durango.Utils;
using L10N;
using Messages;
using Shared.ClanFund;
using Shared.System;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Timeline;

public class TimelineLogBuilder
{
	private readonly Messages.TimelineLog _log;

	private bool _isLoadingAgentPlayer;

	private bool _isLoadingTargetPlayer;

	private Action<TimelineLogBuilder> _completed;

	public double At
	{
		get
		{
			Messages.TimelineLog log = _log;
			return log.At;
		}
	}

	public string Text { get; private set; }

	public Durango.Player.PlayerInfo AgentPlayer { get; private set; }

	public Durango.Player.PlayerInfo TargetPlayer { get; private set; }

	public Building.Blueprint Blueprint { get; private set; }

	private bool IsLoadingParams => _isLoadingAgentPlayer || _isLoadingTargetPlayer;

	private bool IsCompleted => Text != null;

	public TimelineLogBuilder(Messages.TimelineLog log)
	{
		_log = log;
	}

	public TimelineLogBuilder(TimelineLog log)
	{
		_log = new Messages.TimelineLog
		{
			Type = log.Type,
			At = log.At,
			AgentEntityId = log.AgentEntityId,
			TargetEntityId = log.TargetEntityId,
			TargetArtifact = ((!log.TargetArtifact.HasValue) ? null : log.TargetArtifact.Value.ToArtifactDigest())
		};
		if (KUtility.GetSize(log.Params) > 0)
		{
			_log.Params = new string[log.Params.Length];
			for (int i = 0; i < _log.Params.Length; i++)
			{
				_log.Params[i] = log.Params[i];
			}
		}
	}

	public bool IsNegative()
	{
		Messages.TimelineLog log = _log;
		TimelineEvent type = log.Type;
		if (type == TimelineEvent.Destroy)
		{
			return true;
		}
		return false;
	}

	public void Build(Action<TimelineLogBuilder> completed)
	{
		if (IsCompleted)
		{
			completed(this);
			return;
		}
		_completed = (Action<TimelineLogBuilder>)Delegate.Combine(_completed, completed);
		if (!IsLoadingParams)
		{
			Messages.TimelineLog log = _log;
			if (!string.IsNullOrEmpty(log.AgentEntityId) && AgentPlayer == null)
			{
				_isLoadingAgentPlayer = true;
			}
			Messages.TimelineLog log2 = _log;
			if (!string.IsNullOrEmpty(log2.TargetEntityId) && TargetPlayer == null)
			{
				_isLoadingTargetPlayer = true;
			}
			if (_isLoadingAgentPlayer)
			{
				PlayerInfoManager playerInfoManager = Durango.Utils.Singleton<PlayerInfoManager>.Instance();
				Messages.TimelineLog log3 = _log;
				playerInfoManager.RequestPlayerInfo(log3.AgentEntityId, OnResponseAgentPlayer);
			}
			if (_isLoadingTargetPlayer)
			{
				PlayerInfoManager playerInfoManager2 = Durango.Utils.Singleton<PlayerInfoManager>.Instance();
				Messages.TimelineLog log4 = _log;
				playerInfoManager2.RequestPlayerInfo(log4.TargetEntityId, OnResponseTargetPlayer);
			}
			BuildTextIfParamLoaded();
		}
	}

	private void OnResponseAgentPlayer(Durango.Player.PlayerInfo player)
	{
		_isLoadingAgentPlayer = false;
		AgentPlayer = player;
		BuildTextIfParamLoaded();
	}

	private void OnResponseTargetPlayer(Durango.Player.PlayerInfo player)
	{
		_isLoadingTargetPlayer = false;
		TargetPlayer = player;
		BuildTextIfParamLoaded();
	}

	private void BuildTextIfParamLoaded()
	{
		if (IsLoadingParams || IsCompleted)
		{
			return;
		}
		string text = ((AgentPlayer == null || !AgentPlayer.Valid) ? null : AgentPlayer.Name);
		string text2 = null;
		if (TargetPlayer != null && TargetPlayer.Valid)
		{
			text2 = TargetPlayer.Name;
		}
		else if (_log.TargetArtifact.HasValue)
		{
			Blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(_log.TargetArtifact.Value.PrototypeId);
			if (Blueprint != null)
			{
				text2 = Blueprint.Name;
			}
		}
		Messages.TimelineLog log = _log;
		int num = KUtility.GetSize(log.Params);
		if (text != null)
		{
			num++;
		}
		if (text2 != null)
		{
			num++;
		}
		string[] array = new string[num];
		int num2 = 0;
		if (text != null)
		{
			array[num2++] = text;
		}
		if (text2 != null)
		{
			array[num2++] = text2;
		}
		int i = 0;
		Messages.TimelineLog log2 = _log;
		for (int size = KUtility.GetSize(log2.Params); i < size; i++)
		{
			array[num2++] = GetParam(i);
		}
		BuildText(array);
	}

	private string GetParam(int index)
	{
		Messages.TimelineLog log = _log;
		switch (log.Type)
		{
		case TimelineEvent.HelpPostprocess:
			if (index == 0)
			{
				Messages.TimelineLog log3 = _log;
				if (double.TryParse(log3.Params[0], out var result2))
				{
					return TimedeltaFormatter.Format(result2);
				}
			}
			break;
		case TimelineEvent.ClanFundUsed:
		{
			if (index != 1)
			{
				break;
			}
			Messages.TimelineLog log2 = _log;
			if (int.TryParse(log2.Params[1], out var result))
			{
				switch ((FundType)result)
				{
				case FundType.ExtendTerritory:
					return T._("영토 유지비 적립");
				case FundType.Research:
					return T._("연구비");
				case FundType.ClanMatch:
					return T._("선전포고");
				case FundType.ExpandTerritory:
					return T._("영토 확장");
				case FundType.DeclareTerritory:
					return T._("영토 선언");
				}
			}
			break;
		}
		}
		Messages.TimelineLog log4 = _log;
		return log4.Params[index];
	}

	private void BuildText(string[] param)
	{
		Messages.TimelineLog log = _log;
		bool flag = !string.IsNullOrEmpty(log.AgentEntityId);
		Messages.TimelineLog log2 = _log;
		bool flag2 = !string.IsNullOrEmpty(log2.TargetEntityId) || _log.TargetArtifact.HasValue;
		Dictionary<int, TimelineMessage> messages = Yaml.Util.Singleton<TimelineMessagesYaml>.Instance.messages;
		Messages.TimelineLog log3 = _log;
		if (messages.TryGetValue((int)log3.Type, out var value))
		{
			Gettext gettext = Gettext.Empty;
			if (flag && flag2)
			{
				gettext = value.third_person_side;
			}
			else if (flag)
			{
				gettext = value.target_side;
			}
			else if (flag2)
			{
				gettext = value.agent_side;
			}
			if (Gettext.IsEmpty(gettext))
			{
				gettext = value.simple_state;
			}
			if (Gettext.IsEmpty(gettext))
			{
			}
			Text = ((!Gettext.IsEmpty(gettext)) ? T._(gettext, param) : null);
		}
		else
		{
			Text = null;
		}
		if (_completed != null)
		{
			_completed(this);
		}
		_completed = null;
	}
}
