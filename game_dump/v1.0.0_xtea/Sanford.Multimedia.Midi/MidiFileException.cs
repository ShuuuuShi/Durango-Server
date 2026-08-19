using System;

namespace Sanford.Multimedia.Midi;

public class MidiFileException : ApplicationException
{
	public MidiFileException(string message)
		: base(message)
	{
	}
}
