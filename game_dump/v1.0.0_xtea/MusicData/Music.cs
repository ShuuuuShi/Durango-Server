using System.Collections.Generic;
using System.IO;
using Sanford.Multimedia.Midi;
using UnityEngine;

namespace MusicData;

public class Music
{
	public const int MaxChannel = 16;

	public string Name;

	public IList<Note> Notes;

	public int Division;

	public int Tempo;

	public Music()
	{
		Notes = new List<Note>();
		Division = 24;
		Tempo = 500000;
	}

	public static Music Create(Sequence sequence)
	{
		if (sequence == null || sequence.Count == 0)
		{
			return null;
		}
		Music music = new Music();
		music.Tempo = -1;
		music.Division = sequence.Division;
		Track track = new Track();
		for (int i = 0; i < sequence.Count; i++)
		{
			track.Merge(sequence[i]);
		}
		int j = 0;
		for (int count = track.Count; j < count; j++)
		{
			MidiEvent midiEvent = track.GetMidiEvent(j);
			if (midiEvent.MidiMessage is ChannelMessage channelMessage)
			{
				if (channelMessage.Command == ChannelCommand.NoteOn)
				{
					music.Notes.Add(new Note
					{
						Midi = channelMessage.Data1,
						Tick = midiEvent.AbsoluteTicks,
						Volume = (float)channelMessage.Data2 / 127f,
						Channel = channelMessage.MidiChannel
					});
				}
				else if (channelMessage.Command == ChannelCommand.NoteOff)
				{
					music.Notes.Add(new Note
					{
						Midi = channelMessage.Data1,
						Tick = midiEvent.AbsoluteTicks,
						Volume = 0f,
						Channel = channelMessage.MidiChannel
					});
				}
			}
			else if (music.Tempo == -1 && midiEvent.MidiMessage is MetaMessage { MetaType: MetaType.Tempo } metaMessage)
			{
				TempoChangeBuilder tempoChangeBuilder = new TempoChangeBuilder(metaMessage);
				music.Tempo = tempoChangeBuilder.Tempo;
			}
		}
		if (music.Tempo == -1)
		{
			music.Tempo = 500000;
		}
		return music;
	}

	public void Save(string filePath)
	{
		FileStream fileStream = new FileStream(filePath, FileMode.Open);
		Save(fileStream);
		fileStream.Close();
	}

	public void Save(Stream stream)
	{
		Sequence sequence = new Sequence(Division);
		Track track = new Track();
		TempoChangeBuilder tempoChangeBuilder = new TempoChangeBuilder();
		tempoChangeBuilder.Tempo = Tempo;
		tempoChangeBuilder.Build();
		track.Insert(0, new MetaMessage(tempoChangeBuilder.Result.MetaType, tempoChangeBuilder.Result.GetBytes()));
		int i = 0;
		for (int count = Notes.Count; i < count; i++)
		{
			Note note = Notes[i];
			track.Insert(note.Tick, new ChannelMessage(ChannelCommand.NoteOn, 0, note.Midi, (int)(note.Volume * 127f)));
		}
		sequence.Add(track);
		sequence.Save(stream);
	}

	public int TimerToTick(float timer)
	{
		return Mathf.FloorToInt(timer * (float)Division / ((float)Tempo / 1000000f));
	}

	public float TickToTimer(int tick)
	{
		return (float)Tempo / 1000000f * (float)tick / (float)Division;
	}

	public static Color GetChannelColor(int channel)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		return (Color)(channel switch
		{
			0 => Color.white, 
			1 => Color.red, 
			2 => Color.blue, 
			3 => Color.magenta, 
			4 => Color.grey, 
			5 => Color.green, 
			6 => Color.cyan, 
			7 => Color.yellow, 
			8 => PresetColor.UISkyBlue, 
			9 => PresetColor.UILightRed, 
			10 => PresetColor.UIDarkRed, 
			11 => PresetColor.UIDarkGray, 
			12 => PresetColor.UILightGreen, 
			13 => PresetColor.UIMoreLightGray, 
			14 => PresetColor.UIBlue, 
			15 => PresetColor.UIRed, 
			_ => Color.black, 
		});
	}
}
