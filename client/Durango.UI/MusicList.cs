using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BestHTTP;
using Durango.Logic.Music;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using Sanford.Multimedia.Midi;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class MusicList : MonoBehaviour
{
	public Action<MusicId> MusicPlayed;

	public Action<MusicId> MusicEdited;

	public Action<MusicId> MusicRemoved;

	public Action<MusicId> MusicShared;

	public Action<Durango.Logic.Music.Music> MusicCreated;

	[SerializeField]
	private KScrollView _scrollList;

	[SerializeField]
	private GameObject _makeMusicButton;

	[SerializeField]
	private UILabel _musicSlotLabel;

	[SerializeField]
	private SelectableButton _importButton;

	private List<KeyValuePair<MusicId, Messages.Music>> _musics;

	private bool _isInit;

	public void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_scrollList.Nodes.Init(NodeInit);
		_scrollList.Nodes.Clear();
		_scrollList.ResetPosition();
		UIEventListener uIEventListener = UIEventListener.Get(_makeMusicButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (!HasRemainMusicSlot())
			{
				UIManager.SystemMsg(T._("더 이상 악보를 만들 수 없습니다."));
			}
			else if (MusicCreated != null)
			{
				MusicCreated(null);
			}
		});
		SelectableButton importButton = _importButton;
		importButton.Clicked = (Action)Delegate.Combine(importButton.Clicked, new Action(OnImportMusic));
	}

	private void NodeInit(GameObject obj)
	{
		MusicNodeWidget component = obj.GetComponent<MusicNodeWidget>();
		component.MusicPlayed += OnPlayMusic;
		component.MusicRemoved += OnRemoveMusic;
		component.MusicEdited += OnEditMusic;
		component.MusicShared += OnShareMusic;
	}

	private void OnPlayMusic(MusicId musicId)
	{
		if (MusicPlayed != null)
		{
			MusicPlayed(musicId);
		}
	}

	private void OnRemoveMusic(MusicId musicId)
	{
		if (MusicRemoved != null)
		{
			MusicRemoved(musicId);
		}
	}

	private void OnShareMusic(MusicId musicId)
	{
		if (MusicShared != null)
		{
			MusicShared(musicId);
		}
	}

	private void OnEditMusic(MusicId musicId)
	{
		if (MusicEdited != null)
		{
			MusicEdited(musicId);
		}
	}

	private bool HasRemainMusicSlot()
	{
		int size = KUtility.GetSize(_musics);
		return size < Yaml.Util.Singleton<Constants>.Instance.Musician.SlotCount;
	}

	private void OnImportMusic()
	{
		if (!HasRemainMusicSlot())
		{
			UIManager.SystemMsg(T._("더 이상 악보를 만들 수 없습니다."));
			return;
		}
		MessageBox messageBox = UIManager.MessageBox;
		messageBox.Show(T._("미디 파일 불러오기"), $"<alert_icon/> {MusicManager.GetCopyrightWarningText()}", delegate(int index)
		{
			switch (index)
			{
			case 0:
				ImportFromFile();
				break;
			case 1:
				ImportFromLink();
				break;
			}
		}, new MessageBox.Button(T._("내 미디 파일 불러오기")), new MessageBox.Button(T._("링크로 불러오기")), T._("취소"));
	}

	private void ImportFromFile()
	{
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.ResetArguments();
		genericSelector.SetTitle(T._("불러올 악보를 선택해주세요."));
		if (Application.isEditor || Application.platform == RuntimePlatform.Android)
		{
			genericSelector.SetInfo(AppData.CombinePath("Midi").Replace('/', '\\'));
		}
		SearchOption option = ((GameManager.ClusterMode != 0) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
		string[] files = AppData.GetFiles("Midi", "*.mid", option);
		string[] array = files;
		foreach (string path in array)
		{
			genericSelector.AddItem(Path.GetFileName(path));
		}
		TextAsset[] imported = Resources.LoadAll<TextAsset>("Midi");
		TextAsset[] array2 = imported;
		foreach (TextAsset textAsset in array2)
		{
			genericSelector.AddItem(textAsset.name);
		}
		genericSelector.SetSelected(delegate(int index)
		{
			KUtility.DelayedCall(this, delegate
			{
				int size = KUtility.GetSize(files);
				if (index < size)
				{
					string path2 = files[index];
					using FileStream stream = new FileStream(path2, FileMode.Open);
					ImportMidi(stream);
					return;
				}
				TextAsset textAsset2 = imported[index - size];
				using MemoryStream stream2 = new MemoryStream(textAsset2.bytes);
				ImportMidi(stream2);
			}, 0.1f);
		});
		genericSelector.Show();
	}

	private void ImportFromLink()
	{
		string systemCopyBuffer = GUIUtility.systemCopyBuffer;
		if (!string.IsNullOrEmpty(systemCopyBuffer))
		{
			if (UIUtility.IsUrl(systemCopyBuffer) || systemCopyBuffer.EndsWith(".mid", StringComparison.OrdinalIgnoreCase))
			{
				ImportFromLink(systemCopyBuffer);
				return;
			}
			if (TryImportMML(systemCopyBuffer))
			{
				return;
			}
		}
		TextInputPopup textInputPopup = UIManager.Popup.Tooltip<TextInputPopup>();
		textInputPopup.Show(ImportFromLink, T._("불러올 악보 링크를 적어주세요."), null, isMultiline: true, null, 0);
	}

	private bool TryImportMML(string mml)
	{
		Durango.Logic.Music.Music music = Durango.Logic.Music.Music.CreateFromMabinogiMML(mml);
		if (music == null)
		{
			music = Durango.Logic.Music.Music.CreateFromMs2MML(mml);
		}
		if (music == null)
		{
			return false;
		}
		OnMusicCreate(music);
		return true;
	}

	private void ImportFromLink(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return;
		}
		if (UIUtility.IsUrl(value))
		{
			UIManager.Popup.IsLoading = true;
			Http.Request(value, delegate(byte[] bytes, HTTPResponse responce)
			{
				UIManager.Popup.IsLoading = false;
				try
				{
					if (responce.IsSuccess && KUtility.GetSize(bytes) > 0)
					{
						using (MemoryStream stream = new MemoryStream(bytes))
						{
							ImportMidi(stream);
							return;
						}
					}
				}
				catch
				{
				}
				UIManager.SystemMsg(T._("불러올 수 없는 링크 주소입니다. 올바른 주소를 입력해주세요."));
			});
		}
		else
		{
			TryImportMML(value);
		}
	}

	public void Set(List<KeyValuePair<MusicId, Messages.Music>> musics)
	{
		_musics = musics;
		_scrollList.Nodes.BeginLoad();
		if (_musics != null)
		{
			foreach (KeyValuePair<MusicId, Messages.Music> music in _musics)
			{
				MusicNodeWidget component = _scrollList.Nodes.GetNext().GetComponent<MusicNodeWidget>();
				component.Set(music.Key, music.Value);
			}
		}
		_scrollList.Nodes.EndLoad();
		_scrollList.Reposition();
		_musicSlotLabel.text = $"{KUtility.GetSize(musics)}<weak> / {Yaml.Util.Singleton<Constants>.Instance.Musician.SlotCount}</weak>";
	}

	private void OnMusicCreate([NotNull] Durango.Logic.Music.Music music)
	{
		bool flag = false;
		bool flag2 = false;
		float num = 300f - music.TickToTimer(music.Division) * 2f;
		List<Note> list = new List<Note>();
		for (int i = 0; i < music.Notes.Count; i++)
		{
			Note item = music.Notes[i];
			float num2 = music.TickToTimer(item.Tick);
			if (num2 >= num)
			{
				flag2 = true;
				break;
			}
			item.Channel = 0;
			list.Add(item);
			if (list.Count == 6000)
			{
				flag = true;
				break;
			}
		}
		if (flag2)
		{
			UIManager.SystemMsg(T._("악보로 제작할 수 있는 길이를 초과하여 일부분만 제작되었습니다."));
		}
		else if (flag)
		{
			UIManager.SystemMsg(T._("악보로 제작할 수 있는 용량이 초과되어 일부분만 제작되었습니다."));
		}
		music.Notes = list;
		if (MusicCreated != null)
		{
			MusicCreated(music);
		}
	}

	private void ImportMidi(Stream stream)
	{
		using Sequence sequence = new Sequence();
		sequence.Load(stream);
		Dictionary<int, int?> timbre = new Dictionary<int, int?>();
		Durango.Logic.Music.Music music = Durango.Logic.Music.Music.Create(sequence, ref timbre);
		Action<HashSet<int>> import = delegate(HashSet<int> selectedChannel)
		{
			List<Note> list = new List<Note>();
			for (int k = 0; k < music.Notes.Count; k++)
			{
				Note item = music.Notes[k];
				if (selectedChannel == null || selectedChannel.Contains(item.Channel))
				{
					list.Add(item);
				}
			}
			music.Notes = list;
			OnMusicCreate(music);
		};
		if (timbre.Count > 1)
		{
			GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
			genericSelector.ResetArguments();
			genericSelector.SetTitle(T._("가져올 채널을 선택해주세요."));
			KeyValuePair<int, int?>[] array = timbre.ToArray();
			Array.Sort(array, (KeyValuePair<int, int?> a1, KeyValuePair<int, int?> a2) => a1.Key - a2.Key);
			KeyValuePair<int, int?>[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				KeyValuePair<int, int?> keyValuePair = array2[i];
				if (keyValuePair.Value.HasValue)
				{
					genericSelector.AddItem(MusicManager.GetTimbreName(keyValuePair.Value.Value));
					continue;
				}
				if (keyValuePair.Key == 9)
				{
					genericSelector.AddItem(T._("드럼"));
					continue;
				}
				genericSelector.AddItem(T._("채널 {0}", keyValuePair.Key));
			}
			genericSelector.SetSelectableCount(timbre.Count);
			genericSelector.SetSelected(delegate(int[] selected)
			{
				if (KUtility.GetSize(selected) != 0)
				{
					HashSet<int> hashSet = new HashSet<int>();
					foreach (int num in selected)
					{
						hashSet.Add(array[num].Key);
					}
					import(hashSet);
				}
			});
			genericSelector.Show();
		}
		else
		{
			import(null);
		}
	}
}
