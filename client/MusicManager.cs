using System;
using System.Collections.Generic;
using System.IO;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Logic.Music;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Snappy;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{
	[Serializable]
	public class Instrument
	{
		public string Id;

		private string _name;

		public SpriteData Icon;

		public SoundEventType MidiEvent;

		public GameObjectType InstrumentObject;

		public string Name
		{
			get
			{
				if (_name == null)
				{
					switch (Id)
					{
					case "guitar":
						_name = T.GetParticularString("악기", "기타");
						break;
					case "bass":
						_name = T._("베이스");
						break;
					case "xylophone":
						_name = T._("실로폰");
						break;
					case "pianoelec":
						_name = T._("신시사이저");
						break;
					case "piano":
						_name = T._("피아노");
						break;
					case "smalldrum":
						_name = T._("퍼커션");
						break;
					case "drum":
						_name = T._("드럼");
						break;
					case "horn":
						_name = T._("호른");
						break;
					default:
						_name = Id;
						break;
					}
				}
				return _name;
			}
		}
	}

	public const int NoteMin = 21;

	public const int NoteMax = 108;

	public const int LimitNoteCount = 6000;

	public const int LimitDuration = 300;

	public const uint InvalidInstanceId = 0u;

	[SerializeField]
	private Instrument[] _instruments;

	[SerializeField]
	private GameObject _akSoundObjectTemplate;

	private uint _instanceIdGenerator = 1u;

	private readonly Stack<MidiEventInstance> _midiInstancePool = new Stack<MidiEventInstance>();

	private readonly Dictionary<uint, MidiEventInstance> _midiInstanceDictionary = new Dictionary<uint, MidiEventInstance>();

	private bool _musicEditMode;

	private bool _instrumentsModeState;

	private Action<List<KeyValuePair<MusicId, Messages.Music>>> _musicsCallbacks;

	private List<KeyValuePair<MusicId, Messages.Music>> _musics;

	private AsyncCachedDictionary<string, SharedMusic> _sharedMusicAsyncDict;

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
			text = text + " " + (num - 1);
		}
		return text;
	}

	public static string GetTimbreName(int timbre)
	{
		return timbre switch
		{
			0 => T._("그랜드 피아노"), 
			1 => T._("브라이트 피아노"), 
			2 => T._("전자 그랜드 피아노"), 
			3 => T._("홍키통크 피아노"), 
			4 => T._("전자 피아노 1"), 
			5 => T._("전자 피아노 2"), 
			6 => T._("하프시코드"), 
			7 => T._("클라비넷"), 
			8 => T._("첼레스타"), 
			9 => T._("글로켄슈필"), 
			10 => T._("뮤직박스"), 
			11 => T._("비브라폰"), 
			12 => T._("마림바"), 
			13 => T._("실로폰"), 
			14 => T._("튜블러 벨"), 
			15 => T._("덜시머"), 
			16 => T._("드로우바 오르간"), 
			17 => T._("퍼커시브 오르간"), 
			18 => T._("록 오르간"), 
			19 => T._("파이프 오르간"), 
			20 => T._("풍금"), 
			21 => T._("아코디언"), 
			22 => T._("하모니카"), 
			23 => T._("탱고 아코디언"), 
			24 => T._("어쿠스틱 기타 (나일론 줄)"), 
			25 => T._("어쿠스틱 기타 (금속 줄)"), 
			26 => T._("전기 기타 (재즈)"), 
			27 => T._("전기 기타 (클린)"), 
			28 => T._("전기 기타 (음소거)"), 
			29 => T._("전기 기타 (오버드라이브)"), 
			30 => T._("전기 기타 (디스토션)"), 
			31 => T._("기타 배음"), 
			32 => T._("어쿠스틱 베이스"), 
			33 => T._("베이스 기타 (손가락)"), 
			34 => T._("베이스 기타 (피크)"), 
			35 => T._("무프렛 베이스 기타"), 
			36 => T._("슬랩 베이스 1"), 
			37 => T._("슬랩 베이스 2"), 
			38 => T._("신스 베이스 1"), 
			39 => T._("신스 베이스 2"), 
			40 => T._("바이올린"), 
			41 => T._("비올라"), 
			42 => T._("첼로"), 
			43 => T._("더블 베이스"), 
			44 => T._("트레몰로"), 
			45 => T._("피치카토"), 
			46 => T._("하프"), 
			47 => T._("팀파니"), 
			48 => T._("현악기 앙상블 1"), 
			49 => T._("현악기 앙상블 2"), 
			50 => T._("신스 현악기 1"), 
			51 => T._("신스 현악기 2"), 
			52 => T._("목소리 '아~'"), 
			53 => T._("목소리 '오~'"), 
			54 => T._("신스 목소리"), 
			55 => T._("오케스트라 히트"), 
			56 => T._("트럼펫"), 
			57 => T._("트럼본"), 
			58 => T._("튜바"), 
			59 => T._("뮤트 트럼펫"), 
			60 => T._("호른"), 
			61 => T._("금관 섹션"), 
			62 => T._("신스 금관 1"), 
			63 => T._("신스 금관 2"), 
			64 => T._("소프라노 색소폰"), 
			65 => T._("알토 색소폰"), 
			66 => T._("테너 색소폰"), 
			67 => T._("바리톤 색소폰"), 
			68 => T._("오보에"), 
			69 => T._("잉글리시 호른"), 
			70 => T._("바순"), 
			71 => T._("클라리넷"), 
			72 => T._("피콜로"), 
			73 => T._("플루트"), 
			74 => T._("리코더"), 
			75 => T._("팬플루트"), 
			76 => T._("병 부는 소리"), 
			77 => T._("샤쿠하치"), 
			78 => T._("휘파람"), 
			79 => T._("오카리나"), 
			80 => T._("리드 1 (구형파)"), 
			81 => T._("리드 2 (톱니파)"), 
			82 => T._("리드 3 (칼리오페)"), 
			83 => T._("리드 4 (치프)"), 
			84 => T._("리드 5 (챠랑)"), 
			85 => T._("리드 6 (목소리)"), 
			86 => T._("리드 7 (5도)"), 
			87 => T._("리드 8 (베이스+리드)"), 
			88 => T._("패드 1 (뉴에이지)"), 
			89 => T._("패드 2 (따뜻한)"), 
			90 => T._("패드 3 (폴리신스)"), 
			91 => T._("패드 4 (합창)"), 
			92 => T._("패드 5 (굽은)"), 
			93 => T._("패드 6 (메탈)"), 
			94 => T._("패드 7 (후광)"), 
			95 => T._("패드 8 (쓸어내림)"), 
			96 => T._("음향효과 1 (비)"), 
			97 => T._("음향효과 2 (사운드트랙)"), 
			98 => T._("음향효과 3 (크리스털)"), 
			99 => T._("음향효과 4 (분위기)"), 
			100 => T._("음향효과 5 (밝음)"), 
			101 => T._("음향효과 6 (고블린)"), 
			102 => T._("음향효과 7 (메아리)"), 
			103 => T._("음향효과 8 (사이파이)"), 
			104 => T._("시타르"), 
			105 => T._("밴조"), 
			106 => T._("샤미센"), 
			107 => T._("고토"), 
			108 => T._("칼림바"), 
			109 => T._("백파이프"), 
			110 => T._("피들"), 
			111 => T._("샤나이"), 
			112 => T._("팅클 벨"), 
			113 => T._("아고고"), 
			114 => T._("스틸 드럼"), 
			115 => T._("우드블록"), 
			116 => T._("태고"), 
			117 => T._("멜로딕 톰"), 
			118 => T._("신스 드럼"), 
			119 => T._("역방향 심벌즈"), 
			120 => T._("기타 프렛 노이즈"), 
			121 => T._("브레스 노이즈"), 
			122 => T._("해변"), 
			123 => T._("새소리"), 
			124 => T._("전화 벨"), 
			125 => T._("헬리콥터"), 
			126 => T._("박수"), 
			127 => T._("총소리"), 
			_ => timbre.ToString(), 
		};
	}

	public static string GetCopyrightWarningText()
	{
		return T._("다른 사람의 허락 없이 그 사람이 보유한 음악 등을 무단으로 복제하거나, 일부분을 편집하여 새롭게 제작하는 경우, 저작권 침해로 법적 제재 및 게임 이용의 제재를 받으실 수 있습니다. 모든 이용자들의 저작권 침해를 방지하기 위해 노력하고 있으며, 저작권 침해가 발생했을 경우 게임 내 고객센터 1:1 문의를 통해 신고 가능합니다.");
	}

	public static uint PlayMidi(string instrument, byte note, float length, byte velocity)
	{
		if (!Singleton<MusicManager>.HasInstance() || string.IsNullOrEmpty(instrument))
		{
			return 0u;
		}
		return Singleton<MusicManager>.Instance().PlayMidiNote(instrument, note, length, velocity);
	}

	public static uint PlayMidi(string instrument, [NotNull] Durango.Logic.Music.Music music, float startAt = 0f)
	{
		return PlayMidi(instrument, music, SoundPosition.Empty, startAt);
	}

	public static uint PlayMidi(string instrument, [NotNull] Durango.Logic.Music.Music music, SoundPosition soundPosition, float startAt = 0f)
	{
		if (!Singleton<MusicManager>.HasInstance() || string.IsNullOrEmpty(instrument))
		{
			return 0u;
		}
		return Singleton<MusicManager>.Instance().PlayMidiInstance(instrument, music, soundPosition, startAt);
	}

	public static void StopMidi(uint id)
	{
		if (Singleton<MusicManager>.HasInstance() && id != 0)
		{
			Singleton<MusicManager>.Instance().StopMidiInstance(id);
		}
	}

	public static bool IsPlaying(uint id)
	{
		if (!Singleton<MusicManager>.HasInstance() || id == 0)
		{
			return false;
		}
		return Singleton<MusicManager>.Instance().IsPlayingInstace(id);
	}

	public static void SetMusicEditMode(bool editMode)
	{
		if (Singleton<MusicManager>.HasInstance())
		{
			Singleton<MusicManager>.Instance()._musicEditMode = editMode;
			Singleton<MusicManager>.Instance().RefreshInstrumentsModeState();
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		for (int i = 0; i < _instruments.Length; i++)
		{
			SoundManager.PrepareEvent(_instruments[i].MidiEvent);
		}
		_sharedMusicAsyncDict = new AsyncCachedDictionary<string, SharedMusic>(delegate(string key, SharedMusic value, Action<string, SharedMusic> result)
		{
			Connections.Frontend.Send(new GetSharedMusic
			{
				SheetId = key
			}).On(delegate(SharedMusic music, PacketHeader header)
			{
				result(key, music);
			}).Rest(delegate
			{
				result(key, default(SharedMusic));
			});
		}, 60f);
	}

	private void Update()
	{
		foreach (KeyValuePair<uint, MidiEventInstance> item in _midiInstanceDictionary)
		{
			MidiEventInstance value = item.Value;
			if (value.FinishAt < Time.time)
			{
				value.Stop();
				if (RemoveMidiInstance(item.Key, value))
				{
					break;
				}
			}
		}
	}

	public Instrument[] GetInstruments()
	{
		return _instruments;
	}

	public Instrument GetInstrument(string id)
	{
		if (string.IsNullOrEmpty(id))
		{
			return null;
		}
		for (int i = 0; i < _instruments.Length; i++)
		{
			if (_instruments[i].Id == id)
			{
				return _instruments[i];
			}
		}
		return null;
	}

	public void ClearAll()
	{
		foreach (MidiEventInstance item in _midiInstancePool)
		{
			item.DestroySoundObject();
		}
		_midiInstancePool.Clear();
		foreach (MidiEventInstance value in _midiInstanceDictionary.Values)
		{
			value.DestroySoundObject();
		}
		_midiInstanceDictionary.Clear();
		SetMusicEditMode(editMode: false);
	}

	private uint PlayMidiNote(string instrument, byte note, float length, byte velocity)
	{
		Instrument instrument2 = GetInstrument(instrument);
		if (instrument2 == null)
		{
			return 0u;
		}
		uint uNumSamplesPerSecond = Singleton<SoundManager>.Instance().AudioSettings.uNumSamplesPerSecond;
		AkMIDIPostArray akMIDIPostArray = new AkMIDIPostArray(2);
		AkMIDIPost akMIDIPost = akMIDIPostArray[0];
		akMIDIPost.byType = AkMIDIEventTypes.NOTE_ON;
		akMIDIPost.byChan = 0;
		akMIDIPost.byOnOffNote = note;
		akMIDIPost.byVelocity = velocity;
		akMIDIPost.uOffset = 0u;
		AkMIDIPost akMIDIPost2 = akMIDIPostArray[1];
		akMIDIPost2.byType = AkMIDIEventTypes.NOTE_OFF;
		akMIDIPost2.byChan = 0;
		akMIDIPost2.byOnOffNote = note;
		akMIDIPost2.byVelocity = 0;
		akMIDIPost2.uOffset = (uint)((float)uNumSamplesPerSecond * length);
		uint newId;
		MidiEventInstance orCreateMidiInstance = GetOrCreateMidiInstance(out newId);
		if (orCreateMidiInstance.Play(instrument2.MidiEvent, akMIDIPostArray, length, SoundPosition.Empty))
		{
			return newId;
		}
		RemoveMidiInstance(newId, orCreateMidiInstance);
		return 0u;
	}

	private uint PlayMidiInstance(string instrument, Durango.Logic.Music.Music music, SoundPosition soundPosition, float startAt)
	{
		Instrument instrument2 = GetInstrument(instrument);
		if (instrument2 == null)
		{
			return 0u;
		}
		float duration;
		AkMIDIPostArray akMIDIPostArray = MidiEventInstance.CreateMidiPostArray(music, startAt, out duration);
		if (akMIDIPostArray != null)
		{
			uint newId;
			MidiEventInstance orCreateMidiInstance = GetOrCreateMidiInstance(out newId);
			if (orCreateMidiInstance.Play(instrument2.MidiEvent, akMIDIPostArray, duration, soundPosition))
			{
				return newId;
			}
			RemoveMidiInstance(newId, orCreateMidiInstance);
		}
		return 0u;
	}

	private void StopMidiInstance(uint id)
	{
		if (_midiInstanceDictionary.TryGetValue(id, out var value))
		{
			value.Stop();
			RemoveMidiInstance(id, value);
		}
	}

	private bool IsPlayingInstace(uint id)
	{
		return _midiInstanceDictionary.ContainsKey(id);
	}

	[NotNull]
	private MidiEventInstance GetOrCreateMidiInstance(out uint newId)
	{
		MidiEventInstance midiEventInstance = ((_midiInstancePool.Count <= 0) ? new MidiEventInstance(_akSoundObjectTemplate, base.transform) : _midiInstancePool.Pop());
		newId = _instanceIdGenerator++;
		_midiInstanceDictionary.Add(newId, midiEventInstance);
		RefreshInstrumentsModeState();
		return midiEventInstance;
	}

	private bool RemoveMidiInstance(uint id, [NotNull] MidiEventInstance instance)
	{
		if (_midiInstanceDictionary.Remove(id))
		{
			_midiInstancePool.Push(instance);
			RefreshInstrumentsModeState();
			return true;
		}
		return false;
	}

	private void RefreshInstrumentsModeState()
	{
		bool flag = _musicEditMode || _midiInstanceDictionary.Count > 0;
		if (_instrumentsModeState != flag)
		{
			_instrumentsModeState = flag;
			SoundManager.SetState(new SoundStates("instruments_mode", (!_instrumentsModeState) ? "off" : "on"));
		}
	}

	public static void PlayMusic(MusicId musicId, Messages.Music music, ItemData item)
	{
		if (item == null)
		{
			return;
		}
		Performance? performanceData = item.GetPerformanceData("instrument");
		if (!performanceData.HasValue || performanceData.Value.Strs == null)
		{
			return;
		}
		string value = performanceData.Value.Strs.Get("timbre");
		if (!string.IsNullOrEmpty(value))
		{
			string id = item.Id;
			if (musicId.Slot.HasValue)
			{
				Connections.Frontend.Send(new PlayMusic
				{
					Slot = musicId.Slot.Value,
					InstrumentItemId = id
				});
			}
			else if (!string.IsNullOrEmpty(musicId.SharedId))
			{
				Connections.Frontend.Send(new PlaySharedMusic
				{
					SharedSheetId = musicId.SharedId,
					InstrumentItemId = id
				});
			}
		}
	}

	public static void StopMusic()
	{
		Connections.Frontend.Send(default(StopMusic));
	}

	public static void GetMusic(int id, [NotNull] Action<Messages.Music?> callback)
	{
		Connections.Frontend.Send(new GetMusic
		{
			Slot = id
		}).On(delegate(Messages.Music music, PacketHeader header)
		{
			callback(music);
		}).Rest(delegate
		{
			callback(null);
		});
	}

	public void GetSharedMusic(string id, [NotNull] Action<SharedMusic> callback)
	{
		_sharedMusicAsyncDict.Request(id, callback);
	}

	public void GetMusics([NotNull] Action<List<KeyValuePair<MusicId, Messages.Music>>> callback, bool disableCached = false)
	{
		if (_musics != null && !disableCached)
		{
			callback(_musics);
			return;
		}
		if (_musicsCallbacks != null)
		{
			_musicsCallbacks = (Action<List<KeyValuePair<MusicId, Messages.Music>>>)Delegate.Combine(_musicsCallbacks, callback);
			return;
		}
		_musicsCallbacks = (Action<List<KeyValuePair<MusicId, Messages.Music>>>)Delegate.Combine(_musicsCallbacks, callback);
		Connections.Frontend.Send(default(GetMusics)).On(delegate(Musics msg, PacketHeader header)
		{
			bool flag = false;
			if (_musics == null)
			{
				_musics = new List<KeyValuePair<MusicId, Messages.Music>>();
				flag = GameManager.ClusterMode == Mode.Online;
			}
			_musics.Clear();
			_musics.AddRange(msg.GetAllMusics());
			_musics.Sort(Durango.Logic.Music.Music.CompareMusic);
			Action<List<KeyValuePair<MusicId, Messages.Music>>> musicsCallbacks = _musicsCallbacks;
			_musicsCallbacks = null;
			musicsCallbacks(_musics);
			if (flag)
			{
				SaveMusicsToLocal();
			}
		});
	}

	private void SaveMusicsToLocal()
	{
		if (_musics == null)
		{
			return;
		}
		string text = $"Midi\\{GameManager.PlayerId}";
		AppData.DeleteFolder(text);
		foreach (KeyValuePair<MusicId, Messages.Music> music in _musics)
		{
			Messages.Music value = music.Value;
			string arg = value.Name;
			byte[] array = SnappyCodec.Uncompress(value.Data);
			using FileStream fileStream = AppData.OpenFile($"{text}\\{arg}.mid");
			fileStream?.Write(array, 0, array.Length);
		}
	}

	public static void SaveMusic(int id, Messages.Music msg, Action<bool> result)
	{
		Connections.Frontend.Send(new SaveMusicToSlot
		{
			Slot = id,
			Music = msg
		}).All(delegate(Packet p)
		{
			if (result != null)
			{
				result(Packet.IsSuccess(p));
			}
		});
	}

	public static void RemoveMusic(int id, Action<bool> result)
	{
		Connections.Frontend.Send(new RemoveMusicFromSlot
		{
			Slot = id
		}).All(delegate(Packet p)
		{
			if (result != null)
			{
				result(Packet.IsSuccess(p));
			}
		});
	}

	public static void ChangeFollowMusic(string sheetId, bool follow, Action<bool> result)
	{
		Connections.Frontend.Send(new ChangeFollowMusic
		{
			SharedSheetId = sheetId,
			WantFollow = follow
		}).All(delegate(Packet p)
		{
			if (result != null)
			{
				result(Packet.IsSuccess(p));
			}
		});
	}

	public static void PublishMusic(int slot, Action<SharedSheet?> result)
	{
		Connections.Frontend.Send(new PublishMusic
		{
			Slot = slot
		}).On(delegate(SharedSheet msg, PacketHeader header)
		{
			if (result != null)
			{
				result(msg);
			}
		}).Rest(delegate
		{
			if (result != null)
			{
				result(null);
			}
		});
	}

	public static void PlayConcert(PropKey prop)
	{
		Connections.Frontend.Send(new PlayConcert
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile
		});
	}

	public static void FinishConcert(PropKey prop)
	{
		Connections.Frontend.Send(new FinishConcert
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile
		});
	}

	public static void HostConcert(PropKey prop)
	{
		Connections.Frontend.Send(new HostConcert
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile
		});
	}

	public static void RegisterConcert(PropKey prop, int order, string instrumentId)
	{
		Connections.Frontend.Send(new RegisterConcert
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile,
			Order = order,
			InstrumentItemId = instrumentId
		});
	}

	public static void UnregisterConcert(PropKey prop)
	{
		Connections.Frontend.Send(new RegisterConcert
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile
		});
	}

	public static void SetConcertMusic(PropKey prop, int order, MusicId id, string musicName)
	{
		if (id.Slot.HasValue)
		{
			Connections.Frontend.Send(new SetConcertMusic
			{
				EntityId = prop.EntityId,
				Tile = prop.Tile,
				Order = order,
				Slot = id.Slot.Value,
				MusicName = musicName
			});
		}
		else if (!string.IsNullOrEmpty(id.SharedId))
		{
			Connections.Frontend.Send(new SetSharedConcertMusic
			{
				EntityId = prop.EntityId,
				Tile = prop.Tile,
				Order = order,
				SharedSheetId = id.SharedId,
				MusicName = musicName
			});
		}
	}

	public static void ClearConcertMusic(PropKey prop, int order)
	{
		Connections.Frontend.Send(new SetConcertMusic
		{
			EntityId = prop.EntityId,
			Tile = prop.Tile,
			Order = order,
			Slot = null
		});
	}
}
