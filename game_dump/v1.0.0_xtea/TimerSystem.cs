using System;
using System.Collections.Generic;
using K1Network;
using L10N;
using Messages;
using TimerData;
using UnityEngine;

public class TimerSystem : GameSystem<TimerSystem>
{
	private List<TimerData.Timer> _timers = new List<TimerData.Timer>();

	private float _prevTime;

	public event Action<string> StartSubjectProgress;

	public event Action<string, bool> FinishedSubjectProgress;

	private void Awake()
	{
		Connections.Frontend.On(delegate(StartTimer msg, PacketHeader header)
		{
			OnStartTimerMsgReceived(msg.EntityId, msg.Subject, msg.Current, msg.Time + msg.AdditionalTime);
		});
		Connections.Frontend.On(delegate(TimerEnded msg, PacketHeader header)
		{
			TimerData.Timer timer = FindTimer(msg.EntityId, msg.Subject);
			timer.Stop();
		});
		KSingleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			KSingleton<PlayerController>.Instance().MoveStarted += PlayerController_MoveStarted;
			PlayerBehavior.LocalPlayer.DamageTaken += LocalPlayer_DamageTaken;
			PlayerBehavior.LocalPlayer.Died += LocalPlayer_Died;
		};
	}

	private void Update()
	{
		TimerUpdate();
	}

	public void Register(TimerData.Timer timer)
	{
		ulong playerId = GameManager.PlayerId;
		if (timer.EntityId == playerId && !string.IsNullOrEmpty(timer.Subject))
		{
			int i = 0;
			for (int count = _timers.Count; i < count; i++)
			{
				if (_timers[i].EntityId == playerId && !string.IsNullOrEmpty(_timers[i].Subject))
				{
					_timers[i].Stop();
				}
			}
		}
		_timers.Add(timer);
	}

	public void UpdateTimer(TimerData.Timer timer)
	{
		FindTimer(timer.EntityId, timer.Subject)?.Set(timer);
	}

	public bool HasTimerExceptPostProcess()
	{
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (_timers[i].Subject != "postprocess")
			{
				return true;
			}
		}
		return false;
	}

	private TimerData.Timer FindTimer(ulong entityId, string subject)
	{
		if (entityId == 0L && subject == null)
		{
			return null;
		}
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (_timers[i].EntityId == entityId && _timers[i].Subject == subject)
			{
				return _timers[i];
			}
		}
		return null;
	}

	private void TimerUpdate()
	{
		float time = Time.time;
		float prevTime = _prevTime;
		_prevTime = time;
		for (int num = _timers.Count - 1; num >= 0; num--)
		{
			TimerData.Timer timer = _timers[num];
			if (!timer.IsStop)
			{
				if (prevTime < timer.Since && timer.Since <= time)
				{
					TimerStarted(timer);
				}
				if (timer.Until < time)
				{
					timer.Stop();
				}
				if (0f < timer.InterruptAt && timer.InterruptAt < time)
				{
					timer.Stop();
				}
			}
			if (timer.IsStop)
			{
				_timers.RemoveAt(num);
				TimerFinished(timer);
			}
		}
	}

	private void TimerStarted(TimerData.Timer timer)
	{
		if (timer.EntityId == GameManager.PlayerId && !string.IsNullOrEmpty(timer.Subject) && this.StartSubjectProgress != null)
		{
			this.StartSubjectProgress(timer.Subject);
		}
	}

	private void TimerFinished(TimerData.Timer timer)
	{
		if (timer.EntityId == GameManager.PlayerId && !string.IsNullOrEmpty(timer.Subject))
		{
			if (timer.IsInterrupt)
			{
				Connections.Frontend.Send(default(Cancel));
			}
			else
			{
				SendTimerEndedMsg(timer.Subject);
			}
			if (this.FinishedSubjectProgress != null)
			{
				this.FinishedSubjectProgress(timer.Subject, timer.IsInterrupt);
			}
		}
	}

	public void OnStartTimerMsgReceived(ulong entityId, string subject, float ratio, float duration)
	{
		GameObject val = KSingleton<ObjectManager>.Instance().FindObject(entityId);
		if (!((Object)(object)val == (Object)null))
		{
			TimerData.Timer timer = new TimerData.Timer(entityId, subject, duration, ratio);
			ProgressGauge progressGauge = ((!(subject == "prepare_craft") && !(subject == "craft")) ? ((ProgressGauge)TimerData.Timer.Play<IconProgressGauge>(timer)) : ((ProgressGauge)TimerData.Timer.Play<ItemWindowProgressGauge>(timer)));
			progressGauge.SetTarget(val);
			string icon = IconMap.Get($"#interaction_{subject}", "icon_question");
			IconProgressGauge iconProgressGauge = progressGauge as IconProgressGauge;
			if ((Object)(object)iconProgressGauge != (Object)null)
			{
				iconProgressGauge.SetIcon(icon);
			}
			switch (subject)
			{
			case "occupy":
				BuildSystem.OccupyBuildingSite_OnPlay(progressGauge);
				break;
			case "watering":
				KSingleton<PlayerController>.Instance().Motion("Farming_Water", progressGauge.RemainTime());
				DeathActionDescriptor.SetLastAction(DeathActionDescriptor.ActionType.Watering, T._("밭"));
				break;
			}
		}
	}

	private void SendTimerEndedMsg(string subject)
	{
		switch (subject)
		{
		case "prepare_craft":
			return;
		case "craft":
			return;
		case "item_crafting":
			return;
		}
		TimerEnded msg = default(TimerEnded);
		msg.EntityId = PlayerBehavior.LocalPlayer.EntityId;
		msg.Subject = subject;
		Connections.Frontend.Send(msg);
	}

	private void PlayerController_MoveStarted()
	{
		ulong playerId = GameManager.PlayerId;
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (_timers[i].EntityId == playerId && (_timers[i].InterruptCondition & InterruptCondition.MoveStart) != 0)
			{
				_timers[i].Stop();
			}
		}
	}

	private void LocalPlayer_DamageTaken(CharacterBehavior attacker, Damage damage)
	{
		ulong playerId = GameManager.PlayerId;
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (_timers[i].EntityId == playerId && (_timers[i].InterruptCondition & InterruptCondition.TakeDamage) != 0)
			{
				_timers[i].Stop();
			}
		}
	}

	private void LocalPlayer_Died(PlayerBehavior player)
	{
		ulong playerId = GameManager.PlayerId;
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (_timers[i].EntityId == playerId && (_timers[i].InterruptCondition & InterruptCondition.Dead) != 0)
			{
				_timers[i].Stop();
			}
		}
	}

	public static string Timeago(double time)
	{
		double totalSeconds = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;
		int num = (int)(totalSeconds - time);
		if (num < 60)
		{
			return T._("방금");
		}
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		dictionary.Add("time", TimedeltaFormatter.FormatTimedelta(num, 2, "min"));
		Dictionary<string, string> dictionary2 = dictionary;
		return T._("{time} 전", dictionary2);
	}

	public static string TimeToString(double time, TimePeriod period = TimePeriod.Sec, int maxPeriodCount = -1, float threshold = 0.85f)
	{
		return TimedeltaFormatter.FormatTimedelta(time, maxPeriodCount, period switch
		{
			TimePeriod.Sec => "sec", 
			TimePeriod.Min => "min", 
			TimePeriod.Hour => "hour", 
			_ => "day", 
		}, threshold);
	}

	public static TimerData.Timer SetGaugeAndPlayMotion(float duration, string icon, string motionState, string equip = null)
	{
		TimerData.Timer timer = new TimerData.Timer(GameManager.PlayerId, string.Empty, duration);
		IconProgressGauge iconProgressGauge = TimerData.Timer.Play<IconProgressGauge>(timer);
		iconProgressGauge.SetIcon(icon);
		KSingleton<PlayerController>.Instance().Motion(motionState, duration, 1f, forceTransition: true, equip);
		return timer;
	}
}
