namespace Sanford.Multimedia.Midi;

public enum MetaType
{
	SequenceNumber = 0,
	Text = 1,
	Copyright = 2,
	TrackName = 3,
	InstrumentName = 4,
	Lyric = 5,
	Marker = 6,
	CuePoint = 7,
	ProgramName = 8,
	DeviceName = 9,
	EndOfTrack = 47,
	Tempo = 81,
	SmpteOffset = 84,
	TimeSignature = 88,
	KeySignature = 89,
	ProprietaryEvent = 127
}
