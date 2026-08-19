using System;
using System.Collections;
using System.Collections.Generic;
using L10N;
using MusicData;
using UnityEngine;

public class MusicSheetContainer : MonoBehaviour
{
	public Action<Music, string> OnPlayMusic;

	public Action<Music> RequestSave;

	[SerializeField]
	private MusicMenuButton _playButton;

	[SerializeField]
	private MusicMenuButton _previewButton;

	[SerializeField]
	private MusicMenuButton _instrumentButton;

	[SerializeField]
	private MusicMenuButton _recordButton;

	[SerializeField]
	private UISpriteLabel _musicNameLabel;

	[SerializeField]
	private UISpriteLabel _runningTimeLabel;

	[SerializeField]
	private MusicKeyboard _keyboard;

	[SerializeField]
	private int _sheetWords = 4;

	private Music _music;

	private bool _isModified;

	private KScrollView _musicSheets;

	private float _currentGuideLineTimer;

	private int _currentGuideLineSheetIndex;

	private float _targetGuideLineTime;

	private bool _isRecording;

	private float _recordingTimer;

	private int _prevGuideLineSheet;

	private string _instrument;

	private readonly MusicController _musicController = new MusicController();

	private bool _isinit;

	public bool IsModified
	{
		get
		{
			return _isModified;
		}
		set
		{
			_isModified = value;
			if (value)
			{
				_playButton.Icon = "icon_musicsave";
				_playButton.Text = "#music_menu_button_save";
			}
			else
			{
				_playButton.Icon = "music_note";
				_playButton.Text = "#music_menu_button_play";
			}
		}
	}

	private void Init()
	{
		if (!_isinit)
		{
			_isinit = true;
			_musicSheets = ((Component)this).GetComponent<KScrollView>();
			_musicSheets.Nodes.Init(InitSheetNote);
			MusicMenuButton playButton = _playButton;
			playButton.Clicked = (Action)Delegate.Combine(playButton.Clicked, new Action(OnClickPlay));
			MusicMenuButton previewButton = _previewButton;
			previewButton.Clicked = (Action)Delegate.Combine(previewButton.Clicked, new Action(OnClickPreviewPlay));
			MusicMenuButton instrumentButton = _instrumentButton;
			instrumentButton.Clicked = (Action)Delegate.Combine(instrumentButton.Clicked, new Action(OnClickInstrument));
			MusicMenuButton recordButton = _recordButton;
			recordButton.Clicked = (Action)Delegate.Combine(recordButton.Clicked, new Action(OnClickRecord));
			_musicController.OnPlay = OnMusicPlay;
			_musicController.OnStop = OnMusicStop;
			_musicController.OnTick = OnMusicTick;
			_musicController.OnPlayNote = OnMusicPlayNote;
			SelectInstrument(0);
			_keyboard.Init(21, 108);
			_keyboard.KeyboardPressed = KeyboardPressed;
			OnMusicStop();
		}
	}

	private void OnEnable()
	{
		StopRecording();
		if (_music != null)
		{
			((MonoBehaviour)this).StartCoroutine(AsyncSet());
		}
	}

	private void OnDisable()
	{
		_music = null;
		_musicController.Reset();
		_keyboard.AllUnpress();
		if (KSingleton<MusicManager>.HasInstance() && (Object)(object)KSingleton<MusicManager>.Instance().PitchInput != (Object)null)
		{
			KSingleton<MusicManager>.Instance().PitchInput.Stop();
		}
	}

	private void Update()
	{
		if (_currentGuideLineTimer < _targetGuideLineTime)
		{
			float num = _currentGuideLineTimer + Time.deltaTime;
			if (num < _targetGuideLineTime)
			{
				SetGuideLine(num, keyboardSync: false);
				return;
			}
			num = _targetGuideLineTime;
			SetGuideLine(num);
		}
	}

	private void OnInitMusicSheet()
	{
		_musicSheets.MoveToNode(0, instant: true);
		_prevGuideLineSheet = 0;
		SetGuideLine(0f);
		_targetGuideLineTime = 0f;
	}

	private void OnClickPlay()
	{
		if (IsModified)
		{
			SaveMusic();
		}
		else if (OnPlayMusic != null)
		{
			OnPlayMusic(_music, _instrument);
		}
	}

	private void OnClickPreviewPlay()
	{
		PrivewPlayToggle();
	}

	private void OnClickInstrument()
	{
		ShowInstrumentSelector();
	}

	private void OnClickRecord()
	{
		RecordingToggle();
	}

	private void OnMusicPlay()
	{
		_keyboard.Disable = true;
		_previewButton.Text = "#music_menu_button_preview_stop";
		_previewButton.Select = true;
	}

	private void OnMusicStop()
	{
		OnInitMusicSheet();
		_keyboard.Disable = false;
		_previewButton.Text = "#music_menu_button_preview_play";
		_previewButton.Select = false;
	}

	private void OnMusicTick()
	{
		MusicController current = MusicController.Current;
		if (current.IsPlay)
		{
			SetGuideLine(current.Timer, keyboardSync: false);
		}
	}

	private void OnMusicPlayNote()
	{
		MusicController current = MusicController.Current;
		Note lastNote = current.LastNote;
		if (lastNote.Volume > 0f)
		{
			_keyboard.PressKey(lastNote.Midi);
		}
		else
		{
			_keyboard.UnpressKey(lastNote.Midi);
		}
	}

	private void InitSheetNote(GameObject obj)
	{
		MusicSheet component = obj.GetComponent<MusicSheet>();
		component.NoteTouched = NoteTouched;
	}

	private void PrivewPlayToggle()
	{
		if (_musicController.IsPlay)
		{
			_musicController.IsPlay = false;
		}
		else
		{
			MusicManager.Play(_musicController, _music, _instrument, loop: false);
		}
	}

	private void RecordingToggle()
	{
		if (_isRecording)
		{
			StopRecording();
		}
		else
		{
			StartRecording();
		}
	}

	private void SaveMusic()
	{
		if (RequestSave != null)
		{
			RequestSave(_music);
		}
		IsModified = false;
	}

	private void KeyboardPressed(int midi, bool press)
	{
		if (_isRecording)
		{
			if (press)
			{
				RecordingMidi(midi);
			}
		}
		else if (press)
		{
			if (_currentGuideLineTimer < 0f)
			{
				return;
			}
			int num = _music.TimerToTick(_currentGuideLineTimer);
			int num2 = -1;
			bool flag = true;
			for (int i = 0; i < _music.Notes.Count; i++)
			{
				Note note = _music.Notes[i];
				if (note.Tick > num)
				{
					num2 = i;
					break;
				}
				if (note.Tick == num && note.Midi == midi)
				{
					flag = false;
					if (_targetGuideLineTime <= _currentGuideLineTimer)
					{
						_music.Notes.RemoveAt(i);
					}
					break;
				}
			}
			if (flag)
			{
				if (_targetGuideLineTime <= _currentGuideLineTimer)
				{
					_keyboard.AllUnpress();
					_keyboard.PressKey(midi, -1f);
				}
				_targetGuideLineTime = float.MaxValue;
				Note note2 = default(Note);
				note2.Channel = 0;
				note2.Midi = midi;
				note2.Tick = num;
				note2.Volume = 1f;
				Note item = note2;
				if (num2 == -1)
				{
					_music.Notes.Add(item);
					RefreshRunningTime();
				}
				else
				{
					_music.Notes.Insert(num2, item);
				}
			}
			else
			{
				_targetGuideLineTime = 0f;
			}
			MusicSheet component = _musicSheets.Nodes[_currentGuideLineSheetIndex].GetComponent<MusicSheet>();
			component.Refresh(_music);
			CheckNeedNewSheet();
			IsModified = true;
		}
		else
		{
			_targetGuideLineTime = ((!(_targetGuideLineTime > 0f)) ? 0f : (_currentGuideLineTimer + 0.5f));
		}
	}

	private void NoteTouched(int tick, int midi)
	{
		float timer = _music.TickToTimer(tick);
		SetGuideLine(timer, keyboardSync: true, scrollSync: false);
		_targetGuideLineTime = 0f;
	}

	public void Set(Music music)
	{
		Init();
		IsModified = false;
		_music = music;
		if (_music == null)
		{
			_music = new Music();
		}
		RefreshMusicName();
		RefreshRunningTime();
		if (((Component)this).gameObject.activeInHierarchy)
		{
			((MonoBehaviour)this).StartCoroutine(AsyncSet());
		}
	}

	private IEnumerator AsyncSet()
	{
		if (_music == null)
		{
			yield break;
		}
		int maxTick = ((_music.Notes.Count > 0) ? _music.Notes[_music.Notes.Count - 1].Tick : 0);
		int term = _music.Division * _sheetWords;
		int sheetCount = Mathf.CeilToInt((float)maxTick / (float)term);
		_currentGuideLineTimer = 0f;
		_musicSheets.Nodes.Set(sheetCount + 1);
		_musicSheets.Reposition(resetPosition: true);
		((Component)_musicSheets.ScrollView).GetComponent<UIPanel>().alpha = 0f;
		UIManager.ShowLoadingIcon(show: true);
		float deltaTime = 0f;
		float start = 0f;
		float timer = 0f;
		int i = 0;
		for (int count = _musicSheets.Nodes.Count; i < count; i++)
		{
			if (timer >= deltaTime)
			{
				yield return null;
				deltaTime = Time.deltaTime * 0.5f;
				start = Time.realtimeSinceStartup;
			}
			MusicSheet sheet = _musicSheets.Nodes[i].GetComponent<MusicSheet>();
			sheet.Set(_music, term * i, term * (i + 1));
			timer = Time.realtimeSinceStartup - start;
		}
		OnInitMusicSheet();
		TweenAlpha.Begin(((Component)_musicSheets.ScrollView).gameObject, 0.3f, 1f);
		UIManager.ShowLoadingIcon(show: false);
	}

	private void RefreshMusicName()
	{
		if (_music != null)
		{
			string arg = ((!string.IsNullOrEmpty(_music.Name)) ? _music.Name : T._("새 악보"));
			_musicNameLabel.text = $"{arg} ({LocalizeSystem.Get($"#music_instrument_{_instrument}")})";
		}
	}

	private void RefreshRunningTime()
	{
		if (_music != null)
		{
			if (_music.Notes == null || _music.Notes.Count == 0)
			{
				_runningTimeLabel.text = TimeSpan.FromSeconds(0.0).ToString();
				return;
			}
			Note note = _music.Notes[_music.Notes.Count - 1];
			_runningTimeLabel.text = TimeSpan.FromSeconds(Mathf.FloorToInt(_music.TickToTimer(note.Tick))).ToString();
		}
	}

	private void CheckNeedNewSheet()
	{
		if (_music.Notes == null || _music.Notes.Count == 0)
		{
			return;
		}
		int tick = _music.Notes[_music.Notes.Count - 1].Tick;
		int num = _music.Division * _sheetWords;
		int num2 = Mathf.CeilToInt((float)tick / (float)num);
		if (num2 == _musicSheets.Nodes.Count)
		{
			MusicSheet musicSheet = ((ListObjectPoolBase<GameObject>)_musicSheets.Nodes).Add<MusicSheet>();
			musicSheet.Set(_music, num * num2, num * (num2 + 1));
		}
		else
		{
			if (num2 + 1 >= _musicSheets.Nodes.Count)
			{
				return;
			}
			_musicSheets.Nodes.Set(num2 + 1);
		}
		_musicSheets.Reposition();
	}

	private void ShowInstrumentSelector()
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		IList<string> instruments = MusicManager.GetInstruments();
		string[] array = new string[instruments.Count];
		int i = 0;
		for (int num = array.Length; i < num; i++)
		{
			array[i] = LocalizeSystem.Get($"#music_instrument_{instruments[i]}");
		}
		MenuTooltip menuTooltip = UIManager.Popup.Tooltip<MenuTooltip>();
		menuTooltip.Set(T._("연주 악기"), array, SelectInstrument);
		menuTooltip.Show(UICamera.selectedObject.GetComponent<UIWidget>(), Vector2.down * 30f, 3600f);
	}

	private void SelectInstrument(int index)
	{
		IList<string> instruments = MusicManager.GetInstruments();
		if (index >= 0 && index < instruments.Count)
		{
			string instrument = (_instrument = instruments[index]);
			_keyboard.Instrument = instrument;
			string text = $"#music_instrument_{_instrument}";
			_instrumentButton.Text = LocalizeSystem.Get(text);
			_instrumentButton.Icon = IconMap.Get(text);
			RefreshMusicName();
		}
	}

	private void StartRecording()
	{
		_isRecording = true;
		((MonoBehaviour)this).StartCoroutine(CoRecording(_currentGuideLineTimer));
		_recordButton.Text = "#music_menu_button_record_stop";
		_recordButton.Select = true;
	}

	private void StopRecording()
	{
		_isRecording = false;
		_recordButton.Text = "#music_menu_button_record";
		_recordButton.Select = false;
	}

	private IEnumerator CoRecording(float start)
	{
		yield return null;
		_recordingTimer = start;
		KSingleton<MusicManager>.Instance().PitchInput.Listen(RecordingMidi);
		while (_isRecording)
		{
			int index = SetGuideLine(_recordingTimer, keyboardSync: false);
			if (index == _musicSheets.Nodes.Count - 1)
			{
				int term = _music.Division * _sheetWords;
				int c = _musicSheets.Nodes.Count;
				MusicSheet sheet = ((ListObjectPoolBase<GameObject>)_musicSheets.Nodes).Add<MusicSheet>();
				sheet.Set(_music, term * c, term * (c + 1));
				_musicSheets.Reposition();
			}
			yield return null;
			_recordingTimer += Time.deltaTime;
		}
		KSingleton<MusicManager>.Instance().PitchInput.Stop();
		Set(_music);
		OnInitMusicSheet();
		IsModified = true;
	}

	private void RecordingMidi(int midi)
	{
		int num = _music.TimerToTick(_recordingTimer);
		int num2 = -1;
		for (int i = 0; i < _music.Notes.Count; i++)
		{
			if (_music.Notes[i].Tick > num)
			{
				num2 = i;
				break;
			}
			if (_music.Notes[i].Tick == num && _music.Notes[i].Midi == midi)
			{
				return;
			}
		}
		Note note = default(Note);
		note.Tick = num;
		note.Midi = midi;
		note.Volume = 1f;
		Note item = note;
		if (num2 == -1)
		{
			_music.Notes.Add(item);
			RefreshRunningTime();
		}
		else
		{
			_music.Notes.Insert(num2, item);
		}
		_musicSheets.Nodes[_currentGuideLineSheetIndex].GetComponent<MusicSheet>().Refresh(_music);
	}

	public int SetGuideLine(float timer, bool keyboardSync = true, bool scrollSync = true)
	{
		_currentGuideLineTimer = timer;
		int num = -1;
		int i = 0;
		for (int count = _musicSheets.Nodes.Count; i < count; i++)
		{
			MusicSheet component = _musicSheets.Nodes[i].GetComponent<MusicSheet>();
			if (component.SetGuideLine(timer))
			{
				num = i;
			}
		}
		if (keyboardSync)
		{
			_keyboard.AllUnpress();
			if (_music != null)
			{
				int num2 = _music.TimerToTick(timer);
				int j = 0;
				for (int count2 = _music.Notes.Count; j < count2 && _music.Notes[j].Tick <= num2; j++)
				{
					if (_music.Notes[j].Tick == num2)
					{
						_keyboard.PressKey(_music.Notes[j].Midi, -1f);
					}
				}
			}
		}
		_currentGuideLineSheetIndex = num;
		if (num != -1 && _prevGuideLineSheet != num)
		{
			_prevGuideLineSheet = num;
			if (scrollSync)
			{
				_musicSheets.MoveToNode(num, instant: false);
			}
		}
		return num;
	}
}
