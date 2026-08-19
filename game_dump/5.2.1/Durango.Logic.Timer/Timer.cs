using System;
using Durango.UI;
using UnityEngine;

namespace Durango.Logic.Timer;

public class Timer
{
	public InterruptCondition InterruptCondition;

	private bool _isStop = true;

	public string EntityId { get; set; }

	public string Subject { get; set; }

	public float Since { get; set; }

	public float Until { get; set; }

	public float Duration
	{
		get
		{
			if (!float.IsPositiveInfinity(Until))
			{
				return Until - Since;
			}
			return -1f;
		}
	}

	public float Remain
	{
		get
		{
			if (!float.IsPositiveInfinity(Until))
			{
				return Until - Now;
			}
			return float.PositiveInfinity;
		}
	}

	public float Now => Time.time;

	public bool IsStop
	{
		get
		{
			return _isStop;
		}
		private set
		{
			_isStop = value;
		}
	}

	public bool IsInterrupt { get; private set; }

	public float InterruptAt { get; private set; }

	public event Action<Timer> Finished;

	public Timer()
	{
	}

	public Timer(float since, float until, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		Set(string.Empty, null, since, until, interruptCondition);
	}

	public Timer(float duration, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		Set(duration, interruptCondition);
	}

	public Timer(string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		SetDuration(subject, duration, ratio, interruptCondition);
	}

	public Timer(string entityId, string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		SetDuration(entityId, subject, duration, ratio, interruptCondition);
	}

	public void Set(Timer timer)
	{
		Set(timer.EntityId, timer.Subject, timer.Since, timer.Until, timer.InterruptCondition);
		this.Finished = timer.Finished;
	}

	public void SetDuration(string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		SetDuration(GameManager.PlayerId, subject, duration, ratio, interruptCondition);
	}

	public void SetDuration(string entityId, string subject, float duration, float ratio = 0f, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		float now = Now;
		float since = now - duration * ratio;
		float until = now + duration * (1f - ratio);
		Set(entityId, subject, since, until, interruptCondition);
	}

	public void Set(float since, float until, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		Set(string.Empty, null, since, until, interruptCondition);
	}

	public void Set(float duration, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		float now = Now;
		Set(string.Empty, null, now, now + duration, interruptCondition);
	}

	public void Set(string entityId, string subject, float since, float until, InterruptCondition interruptCondition = InterruptCondition.All)
	{
		bool num = entityId != EntityId || subject != Subject;
		EntityId = entityId;
		Subject = subject;
		Since = since;
		Until = until;
		InterruptCondition = interruptCondition;
		IsStop = false;
		IsInterrupt = false;
		InterruptAt = 0f;
		if (num)
		{
			this.Finished = null;
		}
	}

	public void Stop(float delay)
	{
		if (delay > 0f)
		{
			InterruptAt = Now + delay;
		}
		else
		{
			Stop();
		}
	}

	public void Stop(bool raiseEvent = true)
	{
		if (!IsStop)
		{
			IsStop = true;
			float now = Now;
			IsInterrupt = now < Until;
			if (now > Since && raiseEvent && this.Finished != null)
			{
				this.Finished(this);
			}
		}
	}

	public static T Play<T>(Timer timer) where T : ProgressGauge
	{
		GameSystem<TimerSystem>.Instance().Register(timer);
		ProgressGaugeGroup progressGaugeGroup = UIManager.FindScript<ProgressGaugeGroup>();
		if (progressGaugeGroup != null)
		{
			return progressGaugeGroup.Play<T>(timer);
		}
		return null;
	}
}
