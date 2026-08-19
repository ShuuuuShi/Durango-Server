using System;

namespace Sanford.Multimedia.Midi;

public class MidiEvent
{
	private object owner;

	private int absoluteTicks;

	private IMidiMessage message;

	private MidiEvent next;

	private MidiEvent previous;

	internal object Owner => owner;

	public int AbsoluteTicks => absoluteTicks;

	public int DeltaTicks
	{
		get
		{
			if (Previous != null)
			{
				return AbsoluteTicks - previous.AbsoluteTicks;
			}
			return AbsoluteTicks;
		}
	}

	public IMidiMessage MidiMessage => message;

	internal MidiEvent Next
	{
		get
		{
			return next;
		}
		set
		{
			next = value;
		}
	}

	internal MidiEvent Previous
	{
		get
		{
			return previous;
		}
		set
		{
			previous = value;
		}
	}

	internal MidiEvent(object owner, int absoluteTicks, IMidiMessage message)
	{
		if (owner == null)
		{
			throw new ArgumentNullException("owner");
		}
		if (absoluteTicks < 0)
		{
			throw new ArgumentOutOfRangeException("absoluteTicks", absoluteTicks, "Absolute ticks out of range.");
		}
		if (message == null)
		{
			throw new ArgumentNullException("e");
		}
		this.owner = owner;
		this.absoluteTicks = absoluteTicks;
		this.message = message;
	}

	internal void SetAbsoluteTicks(int absoluteTicks)
	{
		this.absoluteTicks = absoluteTicks;
	}
}
