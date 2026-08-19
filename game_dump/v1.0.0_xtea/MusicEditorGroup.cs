using System;
using System.IO;
using MusicData;
using Sanford.Multimedia.Midi;
using UnityEngine;

public class MusicEditorGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private MusicList _musicList;

	[SerializeField]
	private MusicSheetContainer _musicSheetContainer;

	private string _filePath;

	private void Start()
	{
		_musicList.MidiFileClicked = MidiFileClicked;
		_musicList.CreatedNewMusic = CreateNewMusic;
		_musicSheetContainer.OnPlayMusic = PlayMusic;
		_musicSheetContainer.RequestSave = RequestSave;
		_titleWidget.OnClose += base.ForceClose;
		_titleWidget.OnBack += Close;
		base.OnClose();
	}

	protected override bool OnOpen()
	{
		((Component)_musicList).gameObject.SetActive(true);
		((Component)_musicSheetContainer).gameObject.SetActive(false);
		_titleWidget.ShowBackButton(isShow: false, instant: true);
		return base.OnOpen();
	}

	protected override bool OnClose()
	{
		if (((Component)_musicSheetContainer).gameObject.activeSelf)
		{
			((Component)_musicSheetContainer).gameObject.SetActive(false);
			((Component)_musicList).gameObject.SetActive(true);
			_titleWidget.ShowBackButton(isShow: false);
			return false;
		}
		return base.OnClose();
	}

	private void PlayMusic(Music music, string instrument)
	{
		ForceClose();
		KSingleton<PlayerController>.Instance().PlayMusic(music, instrument);
	}

	private void RequestSave(Music music)
	{
		string text = _filePath;
		if (string.IsNullOrEmpty(text))
		{
			string text2 = music.Name;
			if (string.IsNullOrEmpty(text2))
			{
				text2 = DateTime.Now.ToString("yyyyMMddHHmmss");
			}
			text = $"Players/Music/{text2}.mid";
			KFileUtil.GetFileStream(text).Close();
		}
		music.Save(text);
	}

	private void MidiFileClicked(string file)
	{
		_filePath = file;
		if (!string.IsNullOrEmpty(file))
		{
			FileStream stream = new FileStream(file, FileMode.OpenOrCreate);
			Sequence sequence = new Sequence();
			try
			{
				sequence.Load(stream);
				Music music = Music.Create(sequence);
				music.Name = KFileUtil.GetFileName(file);
				OpenMusicSheet(music);
				sequence.Dispose();
				return;
			}
			catch (MidiFileException)
			{
				Music music2 = new Music();
				music2.Name = KFileUtil.GetFileName(file);
				OpenMusicSheet(music2);
				return;
			}
		}
		OpenMusicSheet(null);
	}

	private void CreateNewMusic(string pathOrUrl)
	{
		if (pathOrUrl.ToLower().Contains("http://"))
		{
			_filePath = null;
			KSingleton<MusicManager>.Instance().RequestMidi(pathOrUrl, OpenMusicSheet);
		}
		else if (!string.IsNullOrEmpty(pathOrUrl))
		{
			ulong num = 0uL;
			if (KSingleton<PlayerController>.HasInstance())
			{
				num = GameManager.PlayerId;
			}
			string text = $"Players/{num}/music/{pathOrUrl}.mid";
			FileStream fileStream = KFileUtil.GetFileStream(text);
			fileStream.Dispose();
			text = Path.Combine(KFileUtil.GetAppPath(), text);
			MidiFileClicked(text);
			_musicSheetContainer.IsModified = true;
		}
	}

	public void OpenMusicSheet(Music music)
	{
		if (!base.IsOpen)
		{
			Open();
		}
		_musicSheetContainer.Set(music);
		((Component)_musicList).gameObject.SetActive(false);
		((Component)_musicSheetContainer).gameObject.SetActive(true);
		_titleWidget.ShowBackButton(isShow: true);
	}
}
