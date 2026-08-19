using System;

namespace Sanford.Multimedia.Midi;

public abstract class PpqnClock : IClock
{
	public const int DefaultTempo = 500000;

	public const int PpqnMinValue = 24;

	private const int MicrosecondsPerMillisecond = 1000;

	private int ppqn = 24;

	private int tempo = 500000;

	private int periodResolution;

	private int ticksPerClock;

	private int fractionalTicks;

	private readonly int timerPeriod;

	protected bool running;

	public int Ppqn
	{
		get
		{
			return ppqn;
		}
		set
		{
			if (value < 24)
			{
				throw new ArgumentOutOfRangeException("Ppqn", value, "Pulses per quarter note out of range.");
			}
			if (value % 24 != 0)
			{
				throw new ArgumentException("Pulses per quarter note is not a multiple of 24.");
			}
			ppqn = value;
			CalculatePeriodResolution();
			CalculateTicksPerClock();
		}
	}

	public abstract int Ticks { get; }

	public int TicksPerClock => ticksPerClock;

	public bool IsRunning => running;

	public event EventHandler Tick;

	public event EventHandler Started;

	public event EventHandler Continued;

	public event EventHandler Stopped;

	protected PpqnClock(int timerPeriod)
	{
		if (timerPeriod < 1)
		{
			throw new ArgumentOutOfRangeException("timerPeriod", timerPeriod, "Timer period cannot be less than one.");
		}
		this.timerPeriod = timerPeriod;
		CalculatePeriodResolution();
		CalculateTicksPerClock();
	}

	protected int GetTempo()
	{
		return tempo;
	}

	protected void SetTempo(int tempo)
	{
		if (tempo < 1)
		{
			throw new ArgumentOutOfRangeException("Tempo out of range.");
		}
		this.tempo = tempo;
	}

	protected void Reset()
	{
		fractionalTicks = 0;
	}

	protected int GenerateTicks()
	{
		int num = (fractionalTicks + periodResolution) / tempo;
		fractionalTicks += periodResolution - num * tempo;
		return num;
	}

	private void CalculatePeriodResolution()
	{
		periodResolution = ppqn * timerPeriod * 1000;
	}

	private void CalculateTicksPerClock()
	{
		ticksPerClock = ppqn / 24;
	}

	protected virtual void OnTick(EventArgs e)
	{
		this.Tick?.Invoke(this, EventArgs.Empty);
	}

	protected virtual void OnStarted(EventArgs e)
	{
		this.Started?.Invoke(this, e);
	}

	protected virtual void OnStopped(EventArgs e)
	{
		this.Stopped?.Invoke(this, e);
	}

	protected virtual void OnContinued(EventArgs e)
	{
		this.Continued?.Invoke(this, e);
	}
}
