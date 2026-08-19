using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using MML;
using Messages;
using Sanford.Multimedia.Midi;
using Snappy;
using UnityEngine;

namespace Durango.Logic.Music;

public class Music
{
	public MusicId Id;

	public string Name;

	public List<Note> Notes;

	public int Division;

	public int Tempo;

	public Music()
	{
		Notes = new List<Note>();
		Division = 192;
		Tempo = 750000;
	}

	public static Music Create(Messages.Music m)
	{
		using Sequence sequence = new Sequence();
		try
		{
			using MemoryStream stream = new MemoryStream(SnappyCodec.Uncompress(m.Data));
			sequence.Load(stream);
			Music music = Create(sequence);
			music.Name = m.Name;
			return music;
		}
		catch (MidiFileException)
		{
			return null;
		}
	}

	public static Music CreateFromMabinogiMML(string text)
	{
		string text2 = text.Trim(' ', '\t', '\n', '\r');
		if (text2.StartsWith("MML@", StringComparison.InvariantCultureIgnoreCase))
		{
			text2 = text2.Replace("MML@", string.Empty);
			if (text2.EndsWith(";", StringComparison.InvariantCultureIgnoreCase))
			{
				text2 = text2.Remove(text2.Length - 1);
			}
			return CreateFromMML(text2.Split(','));
		}
		return null;
	}

	public static Music CreateFromMs2MML(string text)
	{
		XmlDocument xmlDocument = new XmlDocument();
		try
		{
			xmlDocument.LoadXml(text);
		}
		catch
		{
			return null;
		}
		XmlNode xmlNode = xmlDocument["ms2"];
		if (xmlNode == null)
		{
			return null;
		}
		XmlNode xmlNode2 = xmlNode.FirstChild;
		List<string> list = new List<string>();
		for (int i = 0; i < 16; i++)
		{
			if (xmlNode2 == null)
			{
				break;
			}
			string innerText = xmlNode2.InnerText;
			if (!string.IsNullOrEmpty(innerText))
			{
				list.Add(innerText.Trim());
				xmlNode2 = xmlNode2.NextSibling;
			}
		}
		return CreateFromMML(list);
	}

	private static Music CreateFromMML(IEnumerable<string> tracks)
	{
		if (tracks == null)
		{
			return null;
		}
		Music music = null;
		foreach (string track in tracks)
		{
			if (string.IsNullOrEmpty(track))
			{
				continue;
			}
			int tempo;
			List<KeyValuePair<double, MML.Note>> list = MMLParser.ToNotes(track, out tempo);
			if (music == null)
			{
				music = new Music
				{
					Division = 120
				};
				if (tempo > 0)
				{
					music.Tempo = 60000000 / tempo;
				}
			}
			foreach (KeyValuePair<double, MML.Note> item3 in (IEnumerable<KeyValuePair<double, MML.Note>>)list)
			{
				if (!(item3.Value.Volume <= 0f))
				{
					int num = item3.Value.GetStep() + 12;
					if (num >= 21 && num <= 108)
					{
						Note note = default(Note);
						note.Tick = music.TimerToTick((float)item3.Key);
						note.Midi = num;
						note.On = true;
						note.Volume = Mathf.Clamp01(item3.Value.Volume);
						Note item = note;
						note = default(Note);
						note.Tick = item.Tick + music.TimerToTick((float)item3.Value.Length.TotalSeconds);
						note.Midi = num;
						note.On = false;
						Note item2 = note;
						music.Notes.Add(item);
						music.Notes.Add(item2);
					}
				}
			}
		}
		if (music == null || music.Notes.Count == 0)
		{
			return null;
		}
		music.Notes.Sort((Note n1, Note n2) => n1.Tick - n2.Tick);
		return music;
	}

	public static Music Create(Sequence sequence)
	{
		Dictionary<int, int?> timbre = null;
		return Create(sequence, ref timbre);
	}

	public static Music Create(Sequence sequence, ref Dictionary<int, int?> timbre)
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
		HashSet<int> hashSet = new HashSet<int>();
		int j = 0;
		for (int count = track.Count; j < count; j++)
		{
			MidiEvent midiEvent = track.GetMidiEvent(j);
			if (midiEvent.MidiMessage is ChannelMessage { Command: var command } channelMessage)
			{
				switch (command)
				{
				case ChannelCommand.NoteOff:
					music.Notes.Add(new Note
					{
						Midi = channelMessage.Data1,
						Tick = midiEvent.AbsoluteTicks,
						Volume = (float)channelMessage.Data2 / 127f,
						Channel = channelMessage.MidiChannel,
						On = false
					});
					break;
				case ChannelCommand.NoteOn:
					if (channelMessage.Data2 > 0)
					{
						hashSet.Add(channelMessage.MidiChannel);
						music.Notes.Add(new Note
						{
							Midi = channelMessage.Data1,
							Tick = midiEvent.AbsoluteTicks,
							Volume = (float)channelMessage.Data2 / 127f,
							Channel = channelMessage.MidiChannel,
							On = true
						});
					}
					else
					{
						music.Notes.Add(new Note
						{
							Midi = channelMessage.Data1,
							Tick = midiEvent.AbsoluteTicks,
							Volume = 0f,
							Channel = channelMessage.MidiChannel,
							On = false
						});
					}
					break;
				case ChannelCommand.ProgramChange:
					if (timbre != null && !timbre.ContainsKey(channelMessage.MidiChannel))
					{
						timbre[channelMessage.MidiChannel] = channelMessage.Data1;
					}
					break;
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
		if (timbre != null)
		{
			int[] array = timbre.Keys.ToArray();
			foreach (int num in array)
			{
				if (!hashSet.Contains(num))
				{
					timbre.Remove(num);
				}
			}
			foreach (int item in hashSet)
			{
				if (!timbre.ContainsKey(item))
				{
					timbre[item] = null;
				}
			}
		}
		return music;
	}

	public int GetLastTick()
	{
		if (Notes.Count == 0)
		{
			return 0;
		}
		Note note = Notes[Notes.Count - 1];
		if (note.On)
		{
			return note.Tick + Division;
		}
		return note.Tick;
	}

	public Messages.Music ToMessage()
	{
		float duration = TickToTimer(GetLastTick());
		Messages.Music result = default(Messages.Music);
		result.Name = Name;
		result.Duration = duration;
		result.Data = SnappyCodec.Compress(ToBytes());
		return result;
	}

	public byte[] ToBytes()
	{
		using Sequence sequence = new Sequence(Division);
		Track track = new Track();
		TempoChangeBuilder tempoChangeBuilder = new TempoChangeBuilder();
		tempoChangeBuilder.Tempo = Tempo;
		tempoChangeBuilder.Build();
		track.Insert(0, new MetaMessage(tempoChangeBuilder.Result.MetaType, tempoChangeBuilder.Result.GetBytes()));
		int i = 0;
		for (int count = Notes.Count; i < count; i++)
		{
			Note note = Notes[i];
			track.Insert(note.Tick, new ChannelMessage((!note.On) ? ChannelCommand.NoteOff : ChannelCommand.NoteOn, 0, note.Midi, (int)(note.Volume * 127f)));
		}
		sequence.Add(track);
		using MemoryStream memoryStream = new MemoryStream();
		sequence.Save(memoryStream);
		return memoryStream.ToArray();
	}

	public int TimerToTick(float timer)
	{
		return Mathf.FloorToInt(timer * (float)Division / ((float)Tempo / 1000000f));
	}

	public float TickToTimer(int tick)
	{
		return (float)Tempo / 1000000f * (float)tick / (float)Division;
	}

	public static int CompareMusic(KeyValuePair<MusicId, Messages.Music> m1, KeyValuePair<MusicId, Messages.Music> m2)
	{
		int num = string.CompareOrdinal(m1.Value.Name, m2.Value.Name);
		if (num == 0)
		{
			if (!string.IsNullOrEmpty(m1.Key.SharedId) && !string.IsNullOrEmpty(m2.Key.SharedId))
			{
				num = string.CompareOrdinal(m1.Key.SharedId, m2.Key.SharedId);
			}
			else if (!string.IsNullOrEmpty(m1.Key.SharedId))
			{
				num = -1;
			}
			else if (!string.IsNullOrEmpty(m2.Key.SharedId))
			{
				num = 1;
			}
			else if (m1.Key.Slot.HasValue && m2.Key.Slot.HasValue)
			{
				num = m1.Key.Slot.Value - m2.Key.Slot.Value;
			}
		}
		return num;
	}
}
