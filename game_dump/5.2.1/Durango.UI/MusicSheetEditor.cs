using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Durango.Logic.Clusters;
using Durango.Logic.Music;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class MusicSheetEditor : MonoBehaviour
{
	private struct MakingNote
	{
		public uint SoundId;

		public Note Note;
	}

	private struct PlayingMusic
	{
		public uint Id;

		public float Since;

		public float Until;
	}

	private enum DuplicatedNoteProcess
	{
		Noting,
		Replace,
		Select
	}

	public const int TermCountPerGroup = 16;

	public static int TickCountPerTerm;

	public Action<Durango.Logic.Music.Music> MusicPlayed;

	public Action<Durango.Logic.Music.Music> MusicSaved;

	public Action<Durango.Logic.Music.Music> MusicShared;

	[SerializeField]
	private MusicSheet _musicSheet;

	[SerializeField]
	private MusicKeyboard _keyboard;

	[SerializeField]
	private GameObject _titleEditButton;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _timerLabel;

	[SerializeField]
	private UIWidget _tempoEditButton;

	[SerializeField]
	private UILabel _tempoLabel;

	[SerializeField]
	private InstrumentSelector _instrumentSelector;

	[SerializeField]
	private SelectableButton _shareButton;

	[SerializeField]
	private SelectableButton _previewButton;

	[SerializeField]
	private SelectableButton _playButton;

	[SerializeField]
	private MusicNoteSelector _noteSelector;

	private Durango.Logic.Music.Music _music;

	private float _playButtonLockedUntil;

	private int _savedVersion;

	private float _currentGuideLineTimer;

	private bool _isSpacePress;

	private bool _isEditPlay;

	private string _instrument;

	private PlayingMusic? _playingMusic;

	private readonly List<MakingNote> _makingNotes = new List<MakingNote>();

	private readonly Dictionary<int, Note> _currentTimerNotes = new Dictionary<int, Note>();

	private bool _isinit;

	public bool IsMusicDirty => _savedVersion != ModifiedVersion;

	public int ModifiedVersion { get; private set; }

	public bool IsPlaying
	{
		get
		{
			PlayingMusic? playingMusic = _playingMusic;
			if (!playingMusic.HasValue)
			{
				return false;
			}
			return Time.time < _playingMusic.Value.Until;
		}
	}

	public void Init()
	{
		if (_isinit)
		{
			return;
		}
		_isinit = true;
		_previewButton.Clicked = OnClickPreviewPlay;
		SelectableButton previewButton = _previewButton;
		previewButton.StateUpdated = (Action<Selectable, Selectable.State>)Delegate.Combine(previewButton.StateUpdated, (Action<Selectable, Selectable.State>)delegate
		{
			_previewButton.Icon = ((!_previewButton.Selected) ? "img_triangle_44" : "img_pause");
		});
		_shareButton.Clicked = OnShareMusic;
		_playButton.Clicked = OnClickPlay;
		_instrumentSelector.InstrumentChanged += OnSelectInstrument;
		_musicSheet.Init();
		_musicSheet.GuidelineChanged += OnChangeGuideline;
		_musicSheet.NoteSelected += SelectNote;
		MusicSheet musicSheet = _musicSheet;
		musicSheet.NoteTickChanged = (Action<Note, int, Note, int>)Delegate.Combine(musicSheet.NoteTickChanged, new Action<Note, int, Note, int>(OnChangeNoteTick));
		_noteSelector.NoteSelected += SelectNote;
		_keyboard.Init(21, 108);
		_keyboard.KeyboardPressed = OnKeyboardPress;
		_keyboard.SpacePressed = OnSpacePress;
		UIEventListener uIEventListener = UIEventListener.Get(_titleEditButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string text)
			{
				text = ResourceSingleton<UILabelStyleTable>.Instance().StripStyle(NGUIText.StripSymbols(text));
				if (!string.IsNullOrEmpty(text))
				{
					_music.Name = text;
					RefreshMusicName();
					SetMusicDirty(dirty: true);
				}
			}, T._("악보의 이름을 적어주세요."), _music.Name);
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_tempoEditButton.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			OnTempoEdit();
		});
		OnMusicStop();
	}

	private void Start()
	{
		MusicManager.Instrument[] instruments = Singleton<MusicManager>.Instance().GetInstruments();
		if (KUtility.GetSize(instruments) > 0)
		{
			OnSelectInstrument(instruments[0].Id);
		}
	}

	private void OnDisable()
	{
		_music = null;
		OnMusicStop();
		_keyboard.ResetKeyboard();
	}

	private void Update()
	{
		if (_music == null)
		{
			return;
		}
		if (IsPlaying)
		{
			float timer = Time.time - _playingMusic.Value.Since;
			SetGuideLine(timer, keyboardSync: true, scrollSync: true, selectNote: false);
			return;
		}
		if (_playingMusic.HasValue)
		{
			int num = _music.TimerToTick(_currentGuideLineTimer);
			num = Mathf.CeilToInt((float)num / (float)TickCountPerTerm) * TickCountPerTerm;
			SetGuideLine(_music.TickToTimer(num), keyboardSync: false, scrollSync: true, selectNote: false);
			OnMusicStop();
		}
		bool isEditPlay = _isEditPlay;
		_isEditPlay = _makingNotes.Count > 0 || _isSpacePress;
		if (_isEditPlay)
		{
			if (!isEditPlay)
			{
				_keyboard.ClearSelectedKeyboard();
			}
			float timer2 = _currentGuideLineTimer + Time.deltaTime;
			int num2 = _music.TimerToTick(timer2);
			for (int num3 = _makingNotes.Count - 1; num3 >= 0; num3--)
			{
				Note note = _makingNotes[num3].Note;
				if (note.Tick + _music.Division <= num2)
				{
					Note note2 = default(Note);
					note2.Channel = note.Channel;
					note2.Midi = note.Midi;
					note2.Tick = note.Tick + _music.Division;
					note2.Volume = 0f;
					note2.On = false;
					Note note3 = note2;
					FinishMakeNote(note3);
				}
			}
			SetGuideLine(timer2, keyboardSync: false, scrollSync: true, selectNote: false);
			_musicSheet.UpdateMusicRunningTime();
		}
		else if (isEditPlay)
		{
			int num4 = _music.TimerToTick(_currentGuideLineTimer);
			num4 = Mathf.RoundToInt((float)num4 / (float)TickCountPerTerm) * TickCountPerTerm;
			SetGuideLine(_music.TickToTimer(num4), keyboardSync: false, scrollSync: true, selectNote: false);
			_musicSheet.UpdateMusicRunningTime();
		}
	}

	public void SetMusicDirty(bool dirty)
	{
		bool isMusicDirty = IsMusicDirty;
		if (dirty)
		{
			ModifiedVersion++;
		}
		else
		{
			_savedVersion = ModifiedVersion;
		}
		bool isMusicDirty2 = IsMusicDirty;
		if (isMusicDirty != isMusicDirty2)
		{
			if (isMusicDirty2)
			{
				_playButton.Text = T._("저장");
				_shareButton.Disabled = true;
			}
			else
			{
				_playButton.Text = T._("연주");
				_shareButton.Disabled = GameManager.ClusterMode != Mode.Online;
			}
		}
	}

	private void ResetMusicSheet()
	{
		SetGuideLine(0f, keyboardSync: true, scrollSync: true, selectNote: false);
		_isEditPlay = false;
		_isSpacePress = false;
		_previewButton.Selected = false;
		ClearSelectNote();
	}

	private void OnClickPlay()
	{
		if (Time.time < _playButtonLockedUntil)
		{
			return;
		}
		if (Debug.isDebugBuild && Input.GetKey(KeyCode.LeftControl))
		{
			using (FileStream fileStream = AppData.OpenFile("Midi/Test/" + _music.Name + ".mid"))
			{
				if (fileStream != null)
				{
					fileStream.SetLength(0L);
					byte[] array = _music.ToBytes();
					fileStream.Write(array, 0, array.Length);
				}
				return;
			}
		}
		_playButtonLockedUntil = Time.time + 1f;
		if (IsMusicDirty)
		{
			SaveMusic();
		}
		else if (MusicPlayed != null)
		{
			MusicPlayed(_music);
		}
	}

	private void OnClickPreviewPlay()
	{
		PrivewPlayToggle();
	}

	private void OnShareMusic()
	{
		if (MusicShared != null)
		{
			MusicShared(_music);
		}
	}

	private void OnMusicPlay()
	{
		float num = _currentGuideLineTimer;
		float num2 = _music.TickToTimer(_music.GetLastTick());
		if (num < 0f || num >= num2)
		{
			num = 0f;
		}
		uint num3 = MusicManager.PlayMidi(_instrument, _music, num);
		if (num3 == 0)
		{
			OnMusicStop();
			return;
		}
		_playingMusic = new PlayingMusic
		{
			Id = num3,
			Since = Time.time - num,
			Until = Time.time - num + num2
		};
		_musicSheet.SetScrollEnable(on: false);
		_noteSelector.Hide();
		_keyboard.Disable = true;
		_previewButton.Selected = true;
		ClearSelectNote();
	}

	private void OnMusicStop()
	{
		if (_playingMusic.HasValue)
		{
			if (Time.time < _playingMusic.Value.Until)
			{
				MusicManager.StopMidi(_playingMusic.Value.Id);
			}
			_playingMusic = null;
		}
		_musicSheet.SetScrollEnable(on: true);
		_isEditPlay = false;
		_isSpacePress = false;
		_previewButton.Selected = false;
		ClearSelectNote();
		_keyboard.Disable = false;
	}

	private void PrivewPlayToggle()
	{
		if (IsPlaying)
		{
			OnMusicStop();
		}
		else
		{
			OnMusicPlay();
		}
	}

	private void SaveMusic()
	{
		int? slot = _music.Id.Slot;
		if (!slot.HasValue)
		{
			return;
		}
		if (_music.TickToTimer(_music.GetLastTick()) > 300f)
		{
			UIManager.SystemMsg(T._("악보로 만들 수 있는 악보 길이를 초과하였습니다. 수정 후 다시 시도 해주세요."));
			return;
		}
		if (_music.Notes.Count > 6000)
		{
			UIManager.SystemMsg(T._("악보로 만들 수 있는 용량을 초과하였습니다. 수정 후 다시 시도 해주세요."));
			return;
		}
		SetMusicDirty(dirty: false);
		Messages.Music msg = _music.ToMessage();
		MusicManager.SaveMusic(slot.Value, msg, delegate(bool success)
		{
			if (success)
			{
				if (MusicSaved != null)
				{
					MusicSaved(_music);
				}
			}
			else
			{
				SetMusicDirty(dirty: true);
			}
		});
	}

	private void OnKeyboardPress(int midi, bool press)
	{
		if (press)
		{
			if (!(_currentGuideLineTimer < 0f))
			{
				Note note = default(Note);
				note.Channel = 0;
				note.Midi = midi;
				note.Tick = _music.TimerToTick(_currentGuideLineTimer);
				note.Volume = 1f;
				note.On = true;
				Note note2 = note;
				bool flag = _makingNotes.Count > 0 || _isSpacePress;
				BeginMakeNote(note2, flag ? DuplicatedNoteProcess.Replace : DuplicatedNoteProcess.Select);
			}
		}
		else
		{
			Note note3 = default(Note);
			note3.Channel = 0;
			note3.Midi = midi;
			note3.Tick = _music.TimerToTick(_currentGuideLineTimer);
			note3.Volume = 0f;
			note3.On = false;
			Note note4 = note3;
			FinishMakeNote(note4);
		}
	}

	private void OnSpacePress(bool press)
	{
		_isSpacePress = press;
	}

	private void BeginMakeNote(Note note, DuplicatedNoteProcess duplicatedProcess)
	{
		note.Tick = Mathf.CeilToInt((float)note.Tick / (float)TickCountPerTerm) * TickCountPerTerm;
		int num = -1;
		int num2 = -1;
		int num3 = -1;
		for (int i = 0; i < _music.Notes.Count; i++)
		{
			Note note2 = _music.Notes[i];
			if (num == -1 && note2.Tick >= note.Tick)
			{
				if (note2.Tick == note.Tick)
				{
					for (int j = i; j < _music.Notes.Count; j++)
					{
						Note note3 = _music.Notes[j];
						if (note3.Tick > note.Tick)
						{
							num = j;
							break;
						}
						if (note3.Midi == note.Midi && !note3.On)
						{
							num = j + 1;
							break;
						}
					}
				}
				else
				{
					num = i;
				}
			}
			if (note2.Midi == note.Midi)
			{
				if (note2.Tick <= note.Tick)
				{
					num2 = i;
				}
				else if (note2.Tick > note.Tick)
				{
					num3 = i;
					break;
				}
			}
			if (note2.Tick > note.Tick + _music.Division)
			{
				break;
			}
		}
		if (num2 != -1 && _music.Notes[num2].On && _music.Notes[num2].Tick > note.Tick - _music.Division)
		{
			switch (duplicatedProcess)
			{
			case DuplicatedNoteProcess.Noting:
				return;
			case DuplicatedNoteProcess.Replace:
				if (num3 != -1 && !_music.Notes[num3].On)
				{
					RemoveAtNote(num3);
				}
				RemoveAtNote(num2);
				if (num != -1)
				{
					num--;
				}
				break;
			case DuplicatedNoteProcess.Select:
				SelectNote(_music.Notes[num2]);
				return;
			}
		}
		if (num == -1 || num >= _music.Notes.Count)
		{
			_music.Notes.Add(note);
			RefreshRunningTime();
		}
		else
		{
			_music.Notes.Insert(num, note);
		}
		uint soundId = MusicManager.PlayMidi(_instrument, (byte)note.Midi, 1f, 127);
		_makingNotes.Add(new MakingNote
		{
			SoundId = soundId,
			Note = note
		});
		_musicSheet.BeginMakeNote(note);
		ClearSelectNote();
		SetMusicDirty(dirty: true);
	}

	private void FinishMakeNote(Note note)
	{
		int num = -1;
		for (int i = 0; i < _makingNotes.Count; i++)
		{
			if (_makingNotes[i].Note.Midi == note.Midi)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			return;
		}
		MusicManager.StopMidi(_makingNotes[num].SoundId);
		int num2 = Mathf.RoundToInt((float)note.Tick / (float)TickCountPerTerm) * TickCountPerTerm;
		if (num2 == _makingNotes[num].Note.Tick)
		{
			num2 += TickCountPerTerm;
		}
		if (_makingNotes[num].Note.Tick <= num2 - _music.Division)
		{
			num2 = _makingNotes[num].Note.Tick + _music.Division;
		}
		note.Tick = num2;
		int num3 = -1;
		int num4 = -1;
		int num5 = -1;
		for (int j = 0; j < _music.Notes.Count; j++)
		{
			Note note2 = _music.Notes[j];
			if (num3 == -1 && note2.Tick >= note.Tick)
			{
				num3 = j;
			}
			if (num4 == -1)
			{
				MakingNote makingNote = _makingNotes[num];
				if (note2.Tick == makingNote.Note.Tick && note2.Midi == makingNote.Note.Midi && note2.On == makingNote.Note.On)
				{
					num4 = j;
				}
			}
			if (note2.Midi == note.Midi && note2.Tick > note.Tick)
			{
				num5 = j;
				break;
			}
			if (note2.Tick > note.Tick + _music.Division)
			{
				break;
			}
		}
		if (num5 != -1 && !_music.Notes[num5].On)
		{
			RemoveAtNote(num5);
		}
		if (num3 == -1)
		{
			_music.Notes.Add(note);
			RefreshRunningTime();
		}
		else
		{
			_music.Notes.Insert(num3, note);
		}
		_makingNotes.RemoveAt(num);
		_musicSheet.FinishMakeNote(note);
		for (int num6 = ((num3 != -1) ? num3 : (_music.Notes.Count - 1)) - 1; num6 > num4; num6--)
		{
			if (_music.Notes[num6].Midi == note.Midi)
			{
				RemoveAtNote(num6);
			}
		}
	}

	private void RemoveAtNote(int index)
	{
		if (index >= 0 || index < _music.Notes.Count)
		{
			Note note = _music.Notes[index];
			_music.Notes.RemoveAt(index);
			_musicSheet.RemoveNote(note);
			if (index == _music.Notes.Count)
			{
				RefreshRunningTime();
			}
		}
	}

	public void Set(Durango.Logic.Music.Music music)
	{
		_savedVersion = 0;
		ModifiedVersion = 0;
		_music = music;
		if (_music == null)
		{
			_music = new Durango.Logic.Music.Music();
		}
		TickCountPerTerm = _music.Division / 16;
		_musicSheet.Set(_music);
		RefreshMusicName();
		RefreshRunningTime();
		RefreshMusicTempo();
		ResetMusicSheet();
		SetMusicDirty(dirty: false);
		if (_music.Notes.Count == 0)
		{
			_playButton.Text = T._("연주");
			_shareButton.Disabled = true;
		}
		else
		{
			_playButton.Text = T._("연주");
			_shareButton.Disabled = GameManager.ClusterMode != Mode.Online;
		}
	}

	private void RefreshMusicName()
	{
		_nameLabel.text = _music.Name + " [icon=icon_chat_edit:1.5]";
	}

	private void RefreshRunningTime()
	{
		if (_music != null)
		{
			string text = ((_music.Notes != null && _music.Notes.Count != 0) ? TimedeltaFormatter.ColonFormat(Mathf.Ceil(_music.TickToTimer(_music.GetLastTick()))) : string.Empty);
			_timerLabel.text = text;
		}
	}

	private void RefreshMusicTempo()
	{
		if (_music != null && _music.Tempo > 0)
		{
			_tempoLabel.text = $"BPM {60000000 / _music.Tempo}";
		}
		else
		{
			_tempoLabel.text = string.Empty;
		}
	}

	private void OnTempoEdit()
	{
		StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
		int?[] items = new int?[4] { null, 120, 100, 80 };
		stringSelector.Set(items.Select((int? value) => value.HasValue ? value.Value.ToString() : T._("직접 입력")), delegate(int index)
		{
			int? num = items[index];
			if (num.HasValue)
			{
				SetBpm(num.Value);
			}
			else
			{
				UIManager.Popup.Tooltip<TextInputPopup>().Show(delegate(string value)
				{
					SetBpm(value.ToInt());
				}, T._("템포(곡의 빠르기)를 선택해주세요"), (_music == null || _music.Tempo <= 0) ? string.Empty : (60000000 / _music.Tempo).ToString());
			}
		});
		stringSelector.MinWidth = _tempoEditButton.width;
		stringSelector.AutoPosition = false;
		stringSelector.DragLock = true;
		stringSelector.Show();
		stringSelector.SetPosition(_tempoEditButton, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0f, 2f));
	}

	private void SetBpm(int bpm)
	{
		if (_music != null && bpm > 0)
		{
			bpm = Mathf.Clamp(bpm, 10, 1000);
			int num = 60000000 / bpm;
			if (_music.Tempo != num)
			{
				_music.Tempo = num;
				SetMusicDirty(dirty: true);
				RefreshMusicTempo();
				RefreshRunningTime();
			}
		}
	}

	private void OnSelectInstrument(string instrument)
	{
		_instrumentSelector.Set(instrument);
		_instrument = instrument;
	}

	private void OnChangeGuideline(int tick, bool stop)
	{
		if (!IsPlaying)
		{
			tick = Mathf.RoundToInt((float)tick / (float)TickCountPerTerm) * TickCountPerTerm;
			float timer = _music.TickToTimer(tick);
			if (stop)
			{
				SetGuideLine(timer, keyboardSync: true, scrollSync: false, selectNote: true);
			}
			else
			{
				SetGuideLine(timer, keyboardSync: true, scrollSync: false, selectNote: false);
				_noteSelector.Hide();
			}
			_isSpacePress = false;
		}
	}

	private void OnChangeNoteTick(Note begin, int beginTick, Note end, int endTick)
	{
		if (_music == null)
		{
			return;
		}
		int num = -1;
		int num2 = -1;
		for (int i = 0; i < _music.Notes.Count; i++)
		{
			Note note = _music.Notes[i];
			if (num == -1 && note.Tick == begin.Tick && note.Midi == begin.Midi && note.On == begin.On)
			{
				num = i;
			}
			if (num2 == -1 && note.Tick == end.Tick && note.Midi == end.Midi && note.On == end.On)
			{
				num2 = i;
			}
		}
		bool flag = false;
		if (num2 != -1)
		{
			_music.Notes.RemoveAt(num2);
			flag = true;
		}
		if (num != -1)
		{
			_music.Notes.RemoveAt(num);
			_musicSheet.RemoveNote(begin);
			flag = true;
		}
		if (beginTick != -1)
		{
			begin.Tick = beginTick;
			int num3 = -1;
			for (int j = 0; j < _music.Notes.Count; j++)
			{
				Note note2 = _music.Notes[j];
				if (num3 == -1 && note2.Tick >= begin.Tick)
				{
					if (note2.Tick == begin.Tick)
					{
						for (int k = j; k < _music.Notes.Count; k++)
						{
							Note note3 = _music.Notes[k];
							if (note3.Tick > begin.Tick)
							{
								num3 = k;
								break;
							}
							if (note3.Midi == begin.Midi && !note3.On)
							{
								num3 = k + 1;
								break;
							}
						}
					}
					else
					{
						num3 = j;
					}
				}
				if (note2.Tick > begin.Tick)
				{
					break;
				}
			}
			if (num3 == -1)
			{
				_music.Notes.Add(begin);
			}
			else
			{
				_music.Notes.Insert(num3, begin);
			}
			flag = true;
		}
		if (endTick != -1)
		{
			end.Tick = endTick;
			int num4 = -1;
			for (int l = 0; l < _music.Notes.Count; l++)
			{
				Note note4 = _music.Notes[l];
				if (num4 == -1 && note4.Tick >= end.Tick)
				{
					num4 = l;
				}
				if (note4.Tick >= end.Tick)
				{
					break;
				}
			}
			if (num4 == -1)
			{
				_music.Notes.Add(end);
			}
			else
			{
				_music.Notes.Insert(num4, end);
			}
			flag = true;
		}
		if (beginTick != -1)
		{
			_musicSheet.AddNote(begin, (endTick != -1) ? (endTick - beginTick) : _music.Division);
		}
		if (flag)
		{
			SetMusicDirty(dirty: true);
		}
	}

	private void SetGuideLine(float timer, bool keyboardSync, bool scrollSync, bool selectNote)
	{
		_currentGuideLineTimer = timer;
		_musicSheet.SetGuideLine(timer, scrollSync);
		if (selectNote)
		{
			ClearSelectNote();
		}
		if (keyboardSync || selectNote)
		{
			_currentTimerNotes.Clear();
			if (_music != null)
			{
				int num = _music.TimerToTick(timer);
				int i = 0;
				for (int count = _music.Notes.Count; i < count; i++)
				{
					Note value = _music.Notes[i];
					if (value.Tick > num)
					{
						break;
					}
					if (value.Tick >= num - _music.Division)
					{
						if (value.On)
						{
							_currentTimerNotes[value.Midi] = value;
						}
						else if (value.Tick < num)
						{
							_currentTimerNotes.Remove(value.Midi);
						}
					}
				}
			}
		}
		if (keyboardSync)
		{
			_keyboard.ResetKeyboard();
			foreach (KeyValuePair<int, Note> currentTimerNote in _currentTimerNotes)
			{
				_keyboard.SelectKey(currentTimerNote.Key, select: true);
			}
		}
		if (!selectNote)
		{
			return;
		}
		if (_currentTimerNotes.Count == 0)
		{
			_noteSelector.Hide();
			return;
		}
		if (_currentTimerNotes.Count > 1)
		{
			_noteSelector.Clear();
			foreach (KeyValuePair<int, Note> currentTimerNote2 in _currentTimerNotes)
			{
				_noteSelector.Add(currentTimerNote2.Value);
			}
			_noteSelector.Show();
			return;
		}
		_noteSelector.Hide();
		using Dictionary<int, Note>.Enumerator enumerator2 = _currentTimerNotes.GetEnumerator();
		if (enumerator2.MoveNext())
		{
			SelectNote(enumerator2.Current.Value);
		}
	}

	private void ClearSelectNote()
	{
		_musicSheet.ClearEditNote();
	}

	private void SelectNote(Note note)
	{
		_noteSelector.Hide();
		_musicSheet.EditNote(note);
		Note? note2 = null;
		int i = 0;
		for (int count = _music.Notes.Count; i < count; i++)
		{
			Note value = _music.Notes[i];
			if (value.Tick >= note.Tick && !value.On && value.Midi == note.Midi && value.Channel == note.Channel)
			{
				note2 = value;
				break;
			}
		}
		float length = ((!note2.HasValue) ? 1f : _music.TickToTimer(note2.Value.Tick - note.Tick));
		MusicManager.PlayMidi(_instrument, (byte)note.Midi, length, 127);
	}
}
