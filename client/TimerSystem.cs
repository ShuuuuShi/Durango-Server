using System;
using System.Collections.Generic;
using Durango.Logic.Timer;
using Durango.Network;
using Durango.UI;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

public class TimerSystem : GameSystem<TimerSystem>
{
	private readonly List<Durango.Logic.Timer.Timer> _timers = new List<Durango.Logic.Timer.Timer>();

	private float _prevTime;

	public event Action<string> StartSubjectProgress;

	public event Action<string, bool> FinishedSubjectProgress;

	private void Awake()
	{
		Connections.Frontend.On<StartTimer>(OnStartTimer);
		Connections.Frontend.On<TimerEnded>(OnTimerEnded);
		Connections.Frontend.On<Canceled>(OnCanceled);
		Singleton<GameManager>.Instance().MainSceneLoaded += delegate
		{
			Singleton<PlayerController>.Instance().MoveStarted += delegate
			{
				StopLocalPlayerTimer(InterruptCondition.MoveStart);
			};
			PlayerBehavior.LocalPlayer.TakenDamage += delegate
			{
				StopLocalPlayerTimer(InterruptCondition.TakeDamage);
			};
			PlayerBehavior.LocalPlayer.Died += delegate
			{
				StopLocalPlayerTimer(InterruptCondition.Dead);
			};
			GameSystem<CombatSystem>.Instance().DamagedProcesser.ControllLost += delegate
			{
				StopLocalPlayerTimer(InterruptCondition.Blow);
			};
		};
	}

	private void Update()
	{
		TimerUpdate();
	}

	public void Register(Durango.Logic.Timer.Timer timer)
	{
		if (!string.IsNullOrEmpty(timer.Subject))
		{
			int i = 0;
			for (int count = _timers.Count; i < count; i++)
			{
				if (_timers[i] == timer)
				{
					return;
				}
				if (_timers[i].EntityId == timer.EntityId && !string.IsNullOrEmpty(_timers[i].Subject))
				{
					_timers[i].Stop();
				}
			}
		}
		_timers.Add(timer);
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

	[CanBeNull]
	private Durango.Logic.Timer.Timer FindTimer(string entityId, string subject)
	{
		if (string.IsNullOrEmpty(entityId) && subject == null)
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

	private void StopLocalPlayerTimer(InterruptCondition condition)
	{
		string playerId = GameManager.PlayerId;
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (!(_timers[i].EntityId != playerId) && (_timers[i].InterruptCondition & condition) != 0)
			{
				_timers[i].Stop();
			}
		}
	}

	private void TimerUpdate()
	{
		float time = Time.time;
		float prevTime = _prevTime;
		_prevTime = time;
		for (int num = _timers.Count - 1; num >= 0; num--)
		{
			Durango.Logic.Timer.Timer timer = _timers[num];
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
				else if (0f < timer.InterruptAt && timer.InterruptAt < time)
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

	private void TimerStarted(Durango.Logic.Timer.Timer timer)
	{
		if (timer.EntityId == GameManager.PlayerId && !string.IsNullOrEmpty(timer.Subject) && this.StartSubjectProgress != null)
		{
			this.StartSubjectProgress(timer.Subject);
		}
	}

	private void TimerFinished(Durango.Logic.Timer.Timer timer)
	{
		if (!(timer.EntityId == GameManager.PlayerId) || string.IsNullOrEmpty(timer.Subject))
		{
			return;
		}
		if (!timer.IsInterrupt)
		{
			string subject = timer.Subject;
			if (subject != null && subject == "occupy_cargo_warphole")
			{
				PlayerController.MotionUpdater.Motion("Occupy_Success");
			}
		}
		if (this.FinishedSubjectProgress != null)
		{
			this.FinishedSubjectProgress(timer.Subject, timer.IsInterrupt);
		}
	}

	private void OnStartTimer(StartTimer msg, PacketHeader header)
	{
		GameObject gameObject = Singleton<ObjectManager>.Instance().FindObject(msg.EntityId);
		if (!(gameObject == null))
		{
			float num = msg.Time + msg.AdditionalTime;
			float ratio = msg.Current / num;
			Durango.Logic.Timer.Timer timer = new Durango.Logic.Timer.Timer(msg.EntityId, msg.Subject, num, ratio);
			string subject = msg.Subject;
			ProgressGauge progressGauge;
			if (subject != null && subject == "occupy_cargo_warphole")
			{
				IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer);
				iconProgressGauge.AddIcon("act_occupy_warphole");
				progressGauge = iconProgressGauge;
			}
			else
			{
				IconProgressGauge iconProgressGauge2 = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer);
				iconProgressGauge2.AddIcon("icon_question");
				progressGauge = iconProgressGauge2;
			}
			progressGauge.SetTarget(gameObject);
			string subject2 = msg.Subject;
			if (subject2 != null && subject2 == "occupy_cargo_warphole")
			{
				PlayerController.MotionUpdater.Motion("Occupy_Try", num, 1f, forceTransition: true);
			}
		}
	}

	private void OnTimerEnded(TimerEnded msg, PacketHeader header)
	{
		Stop(msg.EntityId, msg.Subject);
	}

	private void OnCanceled(Canceled msg, PacketHeader header)
	{
		int i = 0;
		for (int count = _timers.Count; i < count; i++)
		{
			if (_timers[i].EntityId == GameManager.PlayerId)
			{
				_timers[i].Stop();
			}
		}
	}

	public bool Stop(string entityId, string subject)
	{
		Durango.Logic.Timer.Timer timer = FindTimer(entityId, subject);
		if (timer != null)
		{
			timer.Stop();
			return true;
		}
		return false;
	}

	public static Durango.Logic.Timer.Timer SetGaugeAndPlayMotion(float duration, string icon, string motionState, string subject = null, string equip = null)
	{
		Durango.Logic.Timer.Timer timer = new Durango.Logic.Timer.Timer(GameManager.PlayerId, subject, duration);
		IconProgressGauge iconProgressGauge = Durango.Logic.Timer.Timer.Play<IconProgressGauge>(timer);
		iconProgressGauge.AddIcon(icon);
		PlayerController.MotionUpdater.Motion(motionState, duration, 1f, forceTransition: true, overrideIdleMotion: false, equip);
		return timer;
	}
}
