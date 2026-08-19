using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Durango.Logic.Item;
using Durango.Logic.Music;
using Durango.Logic.Social;
using Durango.Network;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using InteractionData;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Chat;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Music")]
public class MusicEditorGroup : UIBase
{
	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass28_0
	{
		public MusicManager.Instrument[] instruments;

		public List<KeyValuePair<string, Messages.Music>> list;
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass28_1
	{
		public KeyValuePair<MusicId, Messages.Music> i;

		public _003C_003Ec__DisplayClass28_0 CS_0024_003C_003E8__locals1;

		internal void _003CCoConcertTest_003Eb__1(int index)
		{
			MusicManager.Instrument instrument = CS_0024_003C_003E8__locals1.instruments[index];
			CS_0024_003C_003E8__locals1.list.Add(new KeyValuePair<string, Messages.Music>(instrument.Id, i.Value));
		}
	}

	[CompilerGenerated]
	private sealed class _003CCoConcertTest_003Ed__28 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public List<KeyValuePair<MusicId, Messages.Music>> musics;

		private _003C_003Ec__DisplayClass28_0 _003C_003E8__1;

		private GenericSelector _003Cselector_003E5__2;

		private string[] _003Ctexts_003E5__3;

		private List<KeyValuePair<MusicId, Messages.Music>>.Enumerator _003C_003E7__wrap3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoConcertTest_003Ed__28(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			int num = _003C_003E1__state;
			if (num == -3 || num == 1)
			{
				try
				{
				}
				finally
				{
					_003C_003Em__Finally1();
				}
			}
			_003C_003E8__1 = null;
			_003Cselector_003E5__2 = null;
			_003Ctexts_003E5__3 = null;
			_003C_003E7__wrap3 = default(List<KeyValuePair<MusicId, Messages.Music>>.Enumerator);
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			try
			{
				int num = _003C_003E1__state;
				if (num != 0)
				{
					if (num != 1)
					{
						return false;
					}
					_003C_003E1__state = -3;
					goto IL_016d;
				}
				_003C_003E1__state = -1;
				_003C_003E8__1 = new _003C_003Ec__DisplayClass28_0();
				_003C_003E8__1.list = new List<KeyValuePair<string, Messages.Music>>();
				_003Cselector_003E5__2 = UIManager.Popup.Tooltip<GenericSelector>();
				_003C_003E8__1.instruments = Durango.Utils.Singleton<MusicManager>.Instance().GetInstruments();
				_003Ctexts_003E5__3 = _003C_003E8__1.instruments.Select((MusicManager.Instrument item) => item.Name).ToArray();
				_003C_003E7__wrap3 = musics.GetEnumerator();
				_003C_003E1__state = -3;
				goto IL_017a;
				IL_016d:
				if (_003Cselector_003E5__2.IsVisible)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_017a;
				IL_017a:
				if (_003C_003E7__wrap3.MoveNext())
				{
					_003C_003Ec__DisplayClass28_1 CS_0024_003C_003E8__locals0 = new _003C_003Ec__DisplayClass28_1
					{
						CS_0024_003C_003E8__locals1 = _003C_003E8__1,
						i = _003C_003E7__wrap3.Current
					};
					_003Cselector_003E5__2.ResetArguments();
					_003Cselector_003E5__2.SetTitle(CS_0024_003C_003E8__locals0.i.Value.Name);
					string[] array = _003Ctexts_003E5__3;
					foreach (string text in array)
					{
						_003Cselector_003E5__2.AddItem(text);
					}
					_003Cselector_003E5__2.SetSelected(delegate(int index)
					{
						MusicManager.Instrument instrument = CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.instruments[index];
						CS_0024_003C_003E8__locals0.CS_0024_003C_003E8__locals1.list.Add(new KeyValuePair<string, Messages.Music>(instrument.Id, CS_0024_003C_003E8__locals0.i.Value));
					});
					_003Cselector_003E5__2.Show();
					goto IL_016d;
				}
				_003C_003Em__Finally1();
				_003C_003E7__wrap3 = default(List<KeyValuePair<MusicId, Messages.Music>>.Enumerator);
				foreach (KeyValuePair<string, Messages.Music> item in _003C_003E8__1.list)
				{
					if (!string.IsNullOrEmpty(item.Key))
					{
						Durango.Logic.Music.Music music = Durango.Logic.Music.Music.Create(item.Value);
						if (music != null)
						{
							MusicManager.PlayMidi(item.Key, music);
						}
					}
				}
				return false;
			}
			catch
			{
				//try-fault
				((IDisposable)this).Dispose();
				throw;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		private void _003C_003Em__Finally1()
		{
			_003C_003E1__state = -1;
			((IDisposable)_003C_003E7__wrap3).Dispose();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private MusicList _musicList;

	[SerializeField]
	private MusicSheetEditor _musicSheetEditor;

	private int _musicEditCloseConfirmVersion;

	private List<KeyValuePair<MusicId, Messages.Music>> _musics;

	private void Awake()
	{
		_musicList.Init();
		_musicSheetEditor.Init();
	}

	private void Start()
	{
		Durango.Utils.Singleton<PlayerController>.Instance().TryMoveWhenLocked += delegate
		{
			if (PlayerBehavior.LocalPlayer.IsPlayingMusic() && !UIBase.HasOpenedUI)
			{
				UIManager.MessageBox.Show(T._("연주를 종료하겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						MusicManager.StopMusic();
					}
				});
			}
		};
		Durango.Utils.Singleton<ArtifactManager>.Instance().StateChangedWithPrev += delegate(Artifact artifact, ArtifactState prev)
		{
			bool num = IsValidBandstand(prev.Bandstand);
			ArtifactState artifactState = artifact.ArtifactState;
			bool flag = IsValidBandstand(artifactState.Bandstand);
			if (num || flag)
			{
				ChatBubbleGroup chatBubbleGroup = UIManager.FindScript<ChatBubbleGroup>();
				if (flag)
				{
					double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
					Bandstand? bandstand = artifactState.Bandstand;
					double num2 = (bandstand.HasValue ? artifactState.Bandstand.Value.ExpiresAt.GetValueOrDefault() : 0.0);
					if (predictedServerTime < num2)
					{
						chatBubbleGroup.Show(artifact.ChatableBase, null, null, "icon_hormony", Color.black, ChatBubble.TargetPivot.Up, Vector3.zero, alwaysInScreen: false, (float)(num2 - predictedServerTime));
						return;
					}
				}
				chatBubbleGroup.Hide(artifact.EntityId);
			}
		};
		Durango.Utils.Singleton<ArtifactManager>.Instance().Removed += delegate(Artifact artifact)
		{
			UIManager.FindScript<ChatBubbleGroup>().Hide(artifact.EntityId);
		};
		base.OnOpenSucceed += delegate
		{
			MusicManager.SetMusicEditMode(editMode: true);
		};
		base.OnCloseSucceed += delegate
		{
			MusicManager.SetMusicEditMode(editMode: false);
		};
		_musicList.MusicEdited = OnMusicEdit;
		_musicList.MusicCreated = CreateNewMusic;
		_musicList.MusicRemoved = OnRemoveMusic;
		_musicList.MusicPlayed = PlayMusic;
		_musicList.MusicShared = ShareMusic;
		_musicSheetEditor.MusicPlayed = delegate(Durango.Logic.Music.Music music)
		{
			PlayMusic(music.Id);
		};
		_musicSheetEditor.MusicShared = delegate(Durango.Logic.Music.Music music)
		{
			ShareMusic(music.Id);
		};
		_musicSheetEditor.MusicSaved = OnMusicSave;
		_titleWidget.Object.SetTitle(string.Format("{0} <help>uri=ui://Music/Help</help>", T._("연주")));
		_titleWidget.Object.OnClose += delegate
		{
			CheckMusicEditCloseWithoutSave(UIBase.CloseAllUI);
		};
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.HostConcert, delegate(InteractionObject obj)
		{
			Artifact targetComponent2 = obj.GetTargetComponent<Artifact>();
			if (!(targetComponent2 == null))
			{
				MusicManager.HostConcert(targetComponent2.GetPropKey());
				UIManager.Popup.Tooltip<ConcertPopup>().SetReserve(targetComponent2.EntityId);
			}
		});
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.RegisterConcert, delegate(InteractionObject obj)
		{
			Artifact targetComponent = obj.GetTargetComponent<Artifact>();
			if (!(targetComponent == null))
			{
				ConcertPopup concertPopup = UIManager.Popup.Tooltip<ConcertPopup>();
				concertPopup.Set(targetComponent);
				concertPopup.Show();
			}
		});
		SetChildrenActive(activated: false);
	}

	protected override bool TryOpen()
	{
		_musicList.gameObject.SetActive(value: true);
		_musicSheetEditor.gameObject.SetActive(value: false);
		_musicList.Set(_musics);
		Refresh();
		return base.TryOpen();
	}

	protected override bool TryClose()
	{
		if (_musicSheetEditor.gameObject.activeSelf)
		{
			CheckMusicEditCloseWithoutSave(delegate
			{
				_musicSheetEditor.gameObject.SetActive(value: false);
				_musicList.gameObject.SetActive(value: true);
			});
			return false;
		}
		return base.TryClose();
	}

	private void CheckMusicEditCloseWithoutSave([NotNull] Action func)
	{
		if (_musicSheetEditor.IsMusicDirty && _musicEditCloseConfirmVersion != _musicSheetEditor.ModifiedVersion)
		{
			UIManager.MessageBox.Show(T._("종료하시겠습니까?"), T._("저장 되지 않은 악보는 사라집니다."), delegate(bool ok)
			{
				if (ok)
				{
					_musicEditCloseConfirmVersion = _musicSheetEditor.ModifiedVersion;
					func();
				}
			}, T._("닫기"), T._("취소"));
		}
		else
		{
			func();
		}
	}

	private int MusicIndexOf(MusicId musicId)
	{
		if (_musics == null)
		{
			return -1;
		}
		for (int i = 0; i < _musics.Count; i++)
		{
			if (musicId.IsEqual(_musics[i].Key))
			{
				return i;
			}
		}
		return -1;
	}

	private bool TryGetMusic(MusicId musicId, out Messages.Music music)
	{
		int num = MusicIndexOf(musicId);
		if (num == -1)
		{
			music = default(Messages.Music);
			return false;
		}
		music = _musics[num].Value;
		return true;
	}

	private void PlayMusic(MusicId musicId)
	{
		if (!TryGetMusic(musicId, out var music))
		{
			return;
		}
		UIManager.Popup.Tooltip<PopupItemSelector>().MyInventory().Title(T._("연주 악기 선택"))
			.HelpText(music.Name)
			.AutoFillText(T._("확인"))
			.Filter((ItemData item) => item.GetPerformanceData("instrument").HasValue)
			.OnConfirmed(delegate(ItemData item)
			{
				if (item != null)
				{
					UIBase.CloseAllUI();
					MusicManager.PlayMusic(musicId, music, item);
				}
			})
			.Show();
	}

	private void OnMusicSave(Durango.Logic.Music.Music music)
	{
		if (_musics == null)
		{
			return;
		}
		MusicId id = music.Id;
		int? slot = id.Slot;
		if (slot.HasValue)
		{
			Messages.Music value = music.ToMessage();
			int num = MusicIndexOf(id);
			KeyValuePair<MusicId, Messages.Music> keyValuePair = new KeyValuePair<MusicId, Messages.Music>(id, value);
			if (num == -1)
			{
				_musics.Add(keyValuePair);
				_musics.Sort(Durango.Logic.Music.Music.CompareMusic);
			}
			else
			{
				_musics[num] = keyValuePair;
			}
			_musicList.Set(_musics);
		}
	}

	private void OnMusicEdit(MusicId id)
	{
		if (TryGetMusic(id, out var music))
		{
			Durango.Logic.Music.Music music2 = Durango.Logic.Music.Music.Create(music);
			if (music2 == null)
			{
				UIManager.SystemMsg(T._("편집할 수 없는 상태의 악보입니다."));
				return;
			}
			music2.Id = id;
			OpenMusicSheet(music2);
		}
	}

	private void Refresh()
	{
		Durango.Utils.Singleton<MusicManager>.Instance().GetMusics(delegate(List<KeyValuePair<MusicId, Messages.Music>> musics)
		{
			_musics = musics;
			_musicList.Set(_musics);
		}, disableCached: true);
	}

	private void OnRemoveMusic(MusicId id)
	{
		UIManager.MessageBox.Show(T._("선택한 악보를 삭제하시겠습니까?"), delegate(bool ok)
		{
			if (ok)
			{
				RemoveMusic(id);
			}
		});
	}

	private void RemoveMusic(MusicId id)
	{
		int num = MusicIndexOf(id);
		if (num == -1)
		{
			return;
		}
		_musics.RemoveAt(num);
		_musicList.Set(_musics);
		if (id.Slot.HasValue)
		{
			MusicManager.RemoveMusic(id.Slot.Value, delegate(bool success)
			{
				if (!success)
				{
					Refresh();
				}
			});
		}
		else
		{
			if (string.IsNullOrEmpty(id.SharedId))
			{
				return;
			}
			MusicManager.ChangeFollowMusic(id.SharedId, follow: false, delegate(bool success)
			{
				if (!success)
				{
					Refresh();
				}
			});
		}
	}

	private void ShareMusic(MusicId id)
	{
		if (!TryGetMusic(id, out var music))
		{
			return;
		}
		string musicName = music.Name;
		GetOrMakeSharedMusicSheetId(id, delegate(string sharedMusicSheetId)
		{
			if (!string.IsNullOrEmpty(sharedMusicSheetId))
			{
				_musicSheetEditor.gameObject.SetActive(value: false);
				_musicList.gameObject.SetActive(value: true);
				if (id.SharedId != sharedMusicSheetId)
				{
					int num = MusicIndexOf(id);
					KeyValuePair<MusicId, Messages.Music> keyValuePair = new KeyValuePair<MusicId, Messages.Music>(sharedMusicSheetId, music);
					if (num == -1)
					{
						_musics.Add(keyValuePair);
						_musics.Sort(Durango.Logic.Music.Music.CompareMusic);
					}
					else
					{
						_musics[num] = keyValuePair;
					}
					_musicList.Set(_musics);
				}
				RadioLink radioLink = MakeMusicExport(sharedMusicSheetId, musicName);
				GameSystem<SocialSystem>.Instance().SystemSay(radioLink, ChannelType.Region);
				ConfirmPopup confirmPopup = UIManager.Popup.Tooltip<ConfirmPopup>();
				confirmPopup.AddButton(new MessageBox.Button(T._("지역 채팅 이동")), delegate
				{
					UIManager.FindScript<ChattingGroupBase>().Open(ChatFilterType.Region, string.Empty);
				});
				confirmPopup.Show(T._("악보가 공유 되었습니다."), 10f);
			}
		});
	}

	public static RadioLink MakeMusicExport(string sharedSheetId, string musicName)
	{
		RadioLink result = default(RadioLink);
		result.Text = T._("<em>{0}</em> 악보를 공유합니다.", musicName);
		result.Link = "icon=icon_music_small, color=FF7E29, link=ui://Music/Import/" + sharedSheetId;
		return result;
	}

	public static void GetOrMakeSharedMusicSheetId(MusicId id, [NotNull] Action<string> result)
	{
		if (!string.IsNullOrEmpty(id.SharedId))
		{
			result(id.SharedId);
			return;
		}
		int? slot = id.Slot;
		if (!slot.HasValue)
		{
			return;
		}
		int slotId = id.Slot.Value;
		UIManager.MessageBox.Show(T._("악보를 공유하시겠습니까?"), string.Format("<alert_icon/> {0}", T._("공유 시 악보를 더 이상 수정할 수 없습니다.")), delegate(bool ok)
		{
			if (ok)
			{
				MusicManager.PublishMusic(slotId, delegate(SharedSheet? sheet)
				{
					if (sheet.HasValue)
					{
						result(sheet.Value.SheetId);
					}
				});
			}
		});
	}

	private void CreateNewMusic(Durango.Logic.Music.Music m)
	{
		string defaultName = m?.Name;
		if (GetNextNewMusic(defaultName, out var id, out var title))
		{
			if (m == null)
			{
				m = new Durango.Logic.Music.Music();
			}
			m.Id = id;
			m.Name = title;
			OpenMusicSheet(m);
			_musicSheetEditor.SetMusicDirty(KUtility.GetSize(m.Notes) > 0);
		}
	}

	private void OpenMusicSheet(Durango.Logic.Music.Music music)
	{
		if (!base.IsOpened)
		{
			Open();
		}
		_musicSheetEditor.Set(music);
		_musicList.gameObject.SetActive(value: false);
		_musicSheetEditor.gameObject.SetActive(value: true);
		_musicEditCloseConfirmVersion = -1;
	}

	private bool GetNextNewMusic(string defaultName, out int id, out string title)
	{
		if (_musics == null)
		{
			id = -1;
			title = null;
			return false;
		}
		if (string.IsNullOrEmpty(defaultName))
		{
			defaultName = T._("새 악보");
		}
		int num = 0;
		int num2 = -1;
		for (int i = 0; i < defaultName.Length; i++)
		{
			if (defaultName[i] == ' ')
			{
				if (num2 == -1)
				{
					num2 = i;
				}
				num++;
			}
		}
		if (num > 1)
		{
			StringBuilder stringBuilder = new StringBuilder(defaultName);
			int num3 = num2 + 1;
			stringBuilder.Replace(" ", string.Empty, num3, stringBuilder.Length - num3);
			defaultName = stringBuilder.ToString();
		}
		string text = defaultName;
		int num4 = -1;
		int j = 0;
		for (int slotCount = Yaml.Util.Singleton<Constants>.Instance.Musician.SlotCount; j < slotCount; j++)
		{
			if (MusicIndexOf(j) == -1)
			{
				num4 = j;
				break;
			}
		}
		if (num4 == -1)
		{
			id = -1;
			title = null;
			return false;
		}
		id = num4;
		int num5 = 0;
		for (int k = 0; k < 100; k++)
		{
			bool flag = true;
			foreach (KeyValuePair<MusicId, Messages.Music> music in _musics)
			{
				if (music.Key.Slot.HasValue && music.Value.Name.Equals(text))
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				title = text;
				return true;
			}
			num5++;
			text = $"{defaultName}{num5}";
		}
		id = -1;
		title = null;
		return false;
	}

	private static bool IsValidBandstand(Bandstand? bandstand)
	{
		if (!bandstand.HasValue)
		{
			return false;
		}
		Bandstand value = bandstand.Value;
		if (string.IsNullOrEmpty(value.Host))
		{
			return false;
		}
		if (value.ExpiresAt.HasValue && value.ExpiresAt.Value < Connections.Frontend.GetPredictedServerTime())
		{
			return false;
		}
		return true;
	}

	[Uri("Import")]
	private void ImportMusic(string sharedSheetId)
	{
		MusicManager.ChangeFollowMusic(sharedSheetId, follow: true, delegate(bool ok)
		{
			if (ok)
			{
				UIManager.SystemMsg(T._("공유받은 악보가 저장되었습니다."));
			}
		});
	}

	[Uri("Help")]
	private void HelpMusicTooltip()
	{
		SimpleTextListPopup simpleTextListPopup = UIManager.Popup.Tooltip<SimpleTextListPopup>();
		if (_musicSheetEditor.gameObject.activeSelf)
		{
			simpleTextListPopup.Set(T._("작곡하기"), new string[2]
			{
				T._("건반을 눌러 음계 노트를 입력합니다.입력한 노트는 터치하여 <em>편집</em>할 수 있습니다."),
				T._("<em>BPM</em>을 입력하여 곡의 빠르기를 조절할 수 있습니다.")
			});
		}
		else
		{
			simpleTextListPopup.Set(T._("악기연주와 합주"), new string[5]
			{
				T._("제작한 악기로 보유하고 있는 악보를 연주할 수 있고, 악보는 연주 화면에서 새 악보 추가 및 불러오기를 통해 제작할 수 있습니다."),
				T._("함께 연주할 사람들과 모닥불에서 <em>합주</em>를 할 수 있습니다."),
				T._("<em>사용하는 기기의 특정 경로</em>에 위치하는 <em>미디파일</em>을 불러오거나, <em>미디파일 링크주소</em>를 입력하여 손 쉽게 악보로 옮길 수 있습니다. 불러온 파일에 담긴 채널 중 원하는 미디 채널를 선택하여 악보를 제작할 수 있습니다."),
				T._("연주화면의 공유버튼, 채팅탭에 악보공유를 눌러서 자신이 만든 악보를 <em>공유</em>할 수 있습니다. 단,공유한 악보는 <em>편집</em>이 불가능해집니다."),
				T._("다른 사람에게 공유 받은 악보로 연주할 수 있습니다. 하지만 원작자의 파일이 삭제, 사용 정지되면 공유받은 악보들도 사용이 불가능해집니다.")
			});
		}
		simpleTextListPopup.Show();
	}

	[Uri("ConcertTest")]
	private void ConcertTest()
	{
		Durango.Utils.Singleton<MusicManager>.Instance().GetMusics(delegate(List<KeyValuePair<MusicId, Messages.Music>> musics)
		{
			StartCoroutine(CoConcertTest(musics));
		});
	}

	private IEnumerator CoConcertTest(List<KeyValuePair<MusicId, Messages.Music>> musics)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoConcertTest_003Ed__28(0)
		{
			musics = musics
		};
	}
}
