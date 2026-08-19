using System;

namespace Durango.Logic.Timer;

public class PredictTimer
{
	private bool _hasMotion;

	private string _motion;

	private string _equip;

	private ItemColor _equipColor;

	private float _motionPeriodOffset;

	public InterruptCondition InterruptCondition = InterruptCondition.All;

	public Timer Timer { get; private set; }

	public event Action<PredictTimer> Started;

	public event Action<PredictTimer> Ended;

	public PredictTimer(string entityId, string subject)
	{
		Timer = new Timer();
		Timer.EntityId = entityId;
		Timer.Subject = subject;
		Timer.Finished += OnFinished;
	}

	public void SetMotion(string motion, string equipment = null, ItemColor color = default(ItemColor), float periodOffset = 0f)
	{
		_hasMotion = true;
		_motion = motion;
		_equip = equipment;
		_equipColor = color;
		_motionPeriodOffset = periodOffset;
		PlayMotion();
	}

	public void Play(float duration)
	{
		bool isStop = Timer.IsStop;
		float num = ((!Timer.IsStop) ? GetRatio() : 0f);
		Timer.SetDuration(Timer.EntityId, Timer.Subject, duration / (1f - num), num, InterruptCondition);
		PlayMotion();
		if (isStop && this.Started != null)
		{
			this.Started(this);
		}
	}

	public void Pause()
	{
		if (!Timer.IsStop)
		{
			Play(1000f);
		}
	}

	public void Stop(bool raiseEvent = true)
	{
		Timer.Stop(raiseEvent);
	}

	private float GetRatio()
	{
		if (Timer.IsStop)
		{
			return 0f;
		}
		float duration = Timer.Duration;
		return (Timer.Now - Timer.Since) / duration;
	}

	private void PlayMotion()
	{
		if (_hasMotion && !Timer.IsStop)
		{
			LocalMotionUpdater motionUpdater = PlayerController.MotionUpdater;
			string motion = _motion;
			float time = Timer.Remain + _motionPeriodOffset;
			string equip = _equip;
			ItemColor equipColor = _equipColor;
			motionUpdater.Motion(motion, time, 1f, forceTransition: false, overrideIdleMotion: false, equip, equipColor);
		}
	}

	private void OnFinished(Timer timer)
	{
		if (_hasMotion)
		{
			LocalMotionUpdater motionUpdater = PlayerController.MotionUpdater;
			bool isInterrupt = timer.IsInterrupt;
			motionUpdater.RefreshMotion(null, force: false, isInterrupt);
		}
		_hasMotion = false;
		if (this.Ended != null)
		{
			this.Ended(this);
		}
	}
}
