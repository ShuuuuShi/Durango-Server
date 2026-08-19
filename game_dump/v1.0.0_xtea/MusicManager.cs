using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MusicData;
using Sanford.Multimedia.Midi;
using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : KSingleton<MusicManager>
{
	private class MidiAudioSource
	{
		public AudioSource Source;

		public Note Note;

		public MidiAudioSource(Note note, AudioSource source)
		{
			Note = note;
			Source = source;
		}
	}

	public const int NoteMin = 21;

	public const int NoteMax = 108;

	public const string InstrumentSoundPath = "Sound/Effect/Instrument/{0}.wav";

	public static readonly float TemperedScale = Mathf.Pow(2f, 1f / 12f);

	[SerializeField]
	private AudioSource _musicAudioSource;

	[SerializeField]
	private float _noteFadeOutDuration;

	[SerializeField]
	private PitchInput _pitchInput;

	[SerializeField]
	private int _pitchShiftStartIndex;

	[SerializeField]
	private AudioMixerGroup _mixer;

	[SerializeField]
	private AudioMixerGroup[] _pitchShifters;

	private Dictionary<string, IList<KeyValuePair<string, int>>> _instrumentSrc;

	private readonly Dictionary<string, AudioClip> _clipDict = new Dictionary<string, AudioClip>();

	public static float Volume { get; private set; }

	private Dictionary<string, IList<KeyValuePair<string, int>>> InstrumentSrc
	{
		get
		{
			if (_instrumentSrc == null)
			{
				InitInstrumentSound();
			}
			return _instrumentSrc;
		}
	}

	public PitchInput PitchInput => _pitchInput;

	private void Start()
	{
		InitInstrumentSound();
		SetVolume(Volume);
	}

	private void CacheSound(string fullPath)
	{
		if (string.IsNullOrEmpty(fullPath) || _clipDict.ContainsKey(fullPath))
		{
			return;
		}
		_clipDict.Add(fullPath, null);
		KSingleton<AssetBundleManager>.Instance().RequestAsset(fullPath, typeof(AudioClip), delegate(Object asset)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Expected O, but got Unknown
			if (asset == (Object)null)
			{
				_clipDict.Remove(fullPath);
			}
			else
			{
				_clipDict[fullPath] = (AudioClip)asset;
			}
		});
		SoundManager.Cache(fullPath);
	}

	public static IList<string> GetInstruments()
	{
		return KSingleton<MusicManager>.Instance().InstrumentSrc.Keys.ToArray();
	}

	private void InitInstrumentSound()
	{
		Dictionary<string, Dictionary<string, int>> dictionary = KUtility.ParseJsonFile<Dictionary<string, Dictionary<string, int>>>("instrument_audio");
		_instrumentSrc = new Dictionary<string, IList<KeyValuePair<string, int>>>();
		foreach (KeyValuePair<string, Dictionary<string, int>> item in dictionary)
		{
			List<KeyValuePair<string, int>> list = item.Value.ToList();
			int i = 0;
			for (int count = list.Count; i < count; i++)
			{
				list[i] = new KeyValuePair<string, int>($"Sound/Effect/Instrument/{list[i].Key}.wav", list[i].Value);
				CacheSound(list[i].Key);
			}
			_instrumentSrc.Add(item.Key, list);
		}
	}

	public void InstrumentSound(string instrument, int midi, out AudioClip clip, out float pitch, out AudioMixerGroup mixer)
	{
		IList<KeyValuePair<string, int>> list = KSingleton<MusicManager>.Instance().InstrumentSrc.Get(instrument);
		int num = int.MaxValue;
		int num2 = -1;
		int i = 0;
		for (int num3 = list?.Count ?? 0; i < num3; i++)
		{
			int num4 = Mathf.Abs(list[i].Value - midi);
			if (num4 < num)
			{
				num = num4;
				num2 = i;
			}
		}
		if (num2 == -1)
		{
			clip = null;
			pitch = 0f;
			mixer = null;
			return;
		}
		int num5 = midi - list[num2].Value;
		if (_pitchShifters == null || _pitchShifters.Length == 0)
		{
			pitch = Mathf.Pow(TemperedScale, (float)num5);
			mixer = null;
		}
		else
		{
			int pitchShiftStartIndex = _pitchShiftStartIndex;
			int num6 = _pitchShiftStartIndex + _pitchShifters.Length - 1;
			if (num5 < pitchShiftStartIndex)
			{
				mixer = _pitchShifters[0];
				pitch = Mathf.Pow(TemperedScale, (float)(num5 - pitchShiftStartIndex));
			}
			else if (num5 <= num6)
			{
				mixer = _pitchShifters[num5 - pitchShiftStartIndex];
				pitch = 1f;
			}
			else
			{
				mixer = _pitchShifters[_pitchShifters.Length - 1];
				pitch = Mathf.Pow(TemperedScale, (float)(num5 - num6));
			}
		}
		clip = _clipDict.Get(list[num2].Key);
	}

	public static string GetNoteName(int note, bool sharps, bool showOctave)
	{
		if (note < 21 || note > 108)
		{
			return null;
		}
		note -= 21;
		int num = (note + 9) / 12;
		note %= 12;
		string text = null;
		switch (note)
		{
		case 0:
			text = "A";
			break;
		case 1:
			text = (sharps ? "A#" : "Bb");
			break;
		case 2:
			text = "B";
			break;
		case 3:
			text = "C";
			break;
		case 4:
			text = (sharps ? "C#" : "Db");
			break;
		case 5:
			text = "D";
			break;
		case 6:
			text = (sharps ? "D#" : "Eb");
			break;
		case 7:
			text = "E";
			break;
		case 8:
			text = "F";
			break;
		case 9:
			text = (sharps ? "F#" : "Gb");
			break;
		case 10:
			text = "G";
			break;
		case 11:
			text = (sharps ? "G#" : "Ab");
			break;
		}
		if (showOctave)
		{
			text = text + " " + num;
		}
		return text;
	}

	public void RequestMidi(string url, Action<Music> callback)
	{
		if (!string.IsNullOrEmpty(url) && callback != null)
		{
			KUtility.RequestUrl(url, delegate(byte[] bytes)
			{
				MemoryStream stream = new MemoryStream(bytes);
				Sequence sequence = new Sequence();
				sequence.Load(stream);
				Music music = Music.Create(sequence);
				music.Name = KFileUtil.GetFileName(url);
				sequence.Dispose();
				callback(music);
			});
		}
	}

	public static void Play(MusicController controller, Music music, string instrument, bool loop, float start = 0f)
	{
		Play(controller, music, instrument, loop, ((Component)KSingleton<PlayerController>.Instance()).transform, start);
	}

	public static void Play(MusicController controller, Music music, string instrument, bool loop, Transform follow, float start = 0f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Play(controller, music, instrument, loop, follow, Vector3.zero, start);
	}

	public static void Play(MusicController controller, Music music, string instrument, bool loop, Vector3 position, float start = 0f)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		Play(controller, music, instrument, loop, null, position, start);
	}

	private static void Play(MusicController controller, Music music, string instrument, bool loop, Transform follow, Vector3 position, float start)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (KSingleton<MusicManager>.HasInstance())
		{
			GameObject parent = ((Component)KSingleton<MusicManager>.Instance()).gameObject.AddChild();
			((MonoBehaviour)KSingleton<MusicManager>.Instance()).StartCoroutine(PlayMusicRoutine(music, controller, instrument, loop, parent, follow, position, start));
		}
	}

	private static IEnumerator PlayMusicRoutine(Music music, MusicController controller, string instrument, bool loop, GameObject parent, Transform follow, Vector3 position, float startTime)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		if (controller == null)
		{
			controller = new MusicController();
		}
		if (controller.IsPlay)
		{
			controller.IsPlay = false;
			yield return null;
		}
		List<MidiAudioSource> sounds = new List<MidiAudioSource>();
		List<MidiAudioSource> fadeOut = new List<MidiAudioSource>();
		Stack<MidiAudioSource> pool = new Stack<MidiAudioSource>();
		Transform t = parent.transform;
		bool useNoteOff = false;
		int j = 0;
		for (int count2 = music.Notes.Count; j < count2; j++)
		{
			if (Math.Abs(music.Notes[j].Volume) < float.Epsilon)
			{
				useNoteOff = true;
				break;
			}
		}
		bool hasParent = (Object)(object)follow != (Object)null;
		float start = startTime;
		while (true)
		{
			int index = 0;
			int prevTick = music.TimerToTick(start) - 1;
			float timer2 = start;
			int k = 0;
			for (int count4 = music.Notes.Count; k < count4; k++)
			{
				if (music.Notes[k].Tick > prevTick)
				{
					index = k;
					break;
				}
			}
			controller.Reset();
			controller.IsPlay = true;
			while (controller.IsPlay && index < music.Notes.Count)
			{
				int tick = music.TimerToTick(timer2);
				controller.Timer = timer2;
				controller.CurrentTick = tick;
				for (int n = fadeOut.Count - 1; n >= 0; n--)
				{
					float d = KSingleton<MusicManager>.Instance()._noteFadeOutDuration;
					float volume2 = fadeOut[n].Source.volume;
					if (Math.Abs(volume2) < float.Epsilon || Math.Abs(d) < float.Epsilon)
					{
						fadeOut[n].Source.Stop();
						pool.Push(fadeOut[n]);
						fadeOut.RemoveAt(n);
					}
					else
					{
						volume2 -= Time.deltaTime / d;
						fadeOut[n].Source.volume = Mathf.Clamp01(volume2);
					}
				}
				if (prevTick != tick)
				{
					bool firstFlag = true;
					for (; index < music.Notes.Count && music.Notes[index].Tick <= tick; index++)
					{
						if (firstFlag && !useNoteOff)
						{
							firstFlag = false;
							int m = 0;
							for (int count5 = sounds.Count; m < count5; m++)
							{
								Note note2 = sounds[m].Note;
								note2.Volume = 0f;
								controller.LastNote = note2;
							}
							fadeOut.AddRange(sounds);
							sounds.Clear();
						}
						if (useNoteOff && Math.Abs(music.Notes[index].Volume) < float.Epsilon)
						{
							int i = 0;
							for (int count = sounds.Count; i < count; i++)
							{
								if (sounds[i].Note.Midi == music.Notes[index].Midi && sounds[i].Note.Channel == music.Notes[index].Channel)
								{
									Note note = sounds[i].Note;
									fadeOut.Add(sounds[i]);
									sounds.RemoveAt(i);
									controller.LastNote = note;
									break;
								}
							}
							continue;
						}
						int soundIndex = -1;
						int l = 0;
						for (int count3 = sounds.Count; l < count3; l++)
						{
							if (sounds[l].Note.Midi == music.Notes[index].Midi && sounds[l].Note.Channel == music.Notes[index].Channel)
							{
								soundIndex = l;
								break;
							}
						}
						if (soundIndex == -1)
						{
							MidiAudioSource src = ((pool.Count <= 0) ? new MidiAudioSource(source: parent.AddChild(((Component)KSingleton<MusicManager>.Instance()._musicAudioSource).gameObject).GetComponent<AudioSource>(), note: music.Notes[index]) : pool.Pop());
							soundIndex = sounds.Count;
							sounds.Add(src);
						}
						KSingleton<MusicManager>.Instance().InstrumentSound(instrument, music.Notes[index].Midi, out var clip, out var pitch, out var mixer);
						AudioSource sound = sounds[soundIndex].Source;
						sound.clip = clip;
						sound.pitch = pitch;
						sound.outputAudioMixerGroup = mixer;
						sound.volume = music.Notes[index].Volume;
						sound.Play();
						sounds[soundIndex].Note = music.Notes[index];
						controller.LastNote = music.Notes[index];
					}
					prevTick = tick;
				}
				if (hasParent)
				{
					if ((Object)(object)follow == (Object)null)
					{
						controller.IsPlay = false;
						break;
					}
					t.position = follow.position;
				}
				else
				{
					t.position = position;
				}
				yield return null;
				timer2 += Time.deltaTime;
			}
			timer2 = 0f;
			float wait = (float)music.Tempo / 1000000f * 2f;
			while (controller.IsPlay && timer2 < wait)
			{
				if (hasParent)
				{
					if ((Object)(object)follow == (Object)null)
					{
						controller.IsPlay = false;
						break;
					}
					t.position = follow.position;
				}
				else
				{
					t.position = position;
				}
				yield return null;
				timer2 += Time.deltaTime;
			}
			if (controller.IsPlay && loop)
			{
				start = 0f;
				continue;
			}
			break;
		}
		Object.Destroy((Object)(object)parent);
		controller.IsPlay = false;
	}

	public static void SetVolume(float val)
	{
		Volume = Mathf.Clamp01(val);
		if (KSingleton<MusicManager>.HasInstance() && (Object)(object)KSingleton<MusicManager>.Instance()._mixer != (Object)null)
		{
			float num = ((Volume != 0f) ? (20f * Mathf.Log10(Volume)) : (-80f));
			KSingleton<MusicManager>.Instance()._mixer.audioMixer.SetFloat("Volume", num);
		}
	}
}
