namespace PitchDetector;

public class Detector
{
	private PitchTracker tracker;

	public Detector()
	{
		tracker = new PitchTracker();
	}

	public void setSampleRate(int samplerate)
	{
		tracker.SampleRate = samplerate;
	}

	public void DetectPitch(float[] inBuffer)
	{
		tracker.ProcessBuffer(inBuffer);
	}

	public int lastMidiNote()
	{
		return tracker.CurrentPitchRecord.MidiNote;
	}

	public float lastMidiNotePrecise()
	{
		return (float)tracker.CurrentPitchRecord.MidiNote + (float)tracker.CurrentPitchRecord.MidiCents / 100f;
	}

	public float lastFrequency()
	{
		return tracker.CurrentPitchRecord.Pitch;
	}

	public string lastNote()
	{
		return PitchDsp.GetNoteName(tracker.CurrentPitchRecord.MidiNote, sharps: true, showOctave: true);
	}

	public string midiNoteToString(int note)
	{
		return PitchDsp.GetNoteName(note, sharps: true, showOctave: true);
	}
}
