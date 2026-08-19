namespace Sanford.Multimedia.Midi;

public interface IMidiMessage
{
	int Status { get; }

	MessageType MessageType { get; }

	byte[] GetBytes();
}
