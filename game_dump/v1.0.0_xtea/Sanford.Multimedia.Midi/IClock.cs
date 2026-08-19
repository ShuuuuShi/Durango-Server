using System;

namespace Sanford.Multimedia.Midi;

public interface IClock
{
	bool IsRunning { get; }

	int Ticks { get; }

	event EventHandler Tick;

	event EventHandler Started;

	event EventHandler Continued;

	event EventHandler Stopped;
}
