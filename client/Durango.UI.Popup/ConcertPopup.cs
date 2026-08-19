using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.Music;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI.Popup;

public class ConcertPopup : TooltipBase
{
	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private Selectable _helpButton;

	[SerializeField]
	private Selectable _clearButton;

	[SerializeField]
	private UILabel _clearButtonLabel;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayout _layout;

	private Concert _concert;

	private List<KeyValuePair<MusicId, Messages.Music>> _musics;

	private string _lastSelectedInstrumentId;

	private string _reservedBandstand;

	private float _reserveValidAt;

	public override bool DragLock => true;

	protected override void OnAwake()
	{
		base.OnAwake();
		_scrollView.Nodes.Init(delegate(GameObject obj)
		{
			ConcertTrackWidget component = obj.GetComponent<ConcertTrackWidget>();
			component.SlotClicked = (Action<Concert.Track>)Delegate.Combine(component.SlotClicked, new Action<Concert.Track>(OnClickSlot));
			component.InstrumentClicked = (Action<Concert.Track>)Delegate.Combine(component.InstrumentClicked, new Action<Concert.Track>(OnClickInstrument));
			component.MusicClicked = (Action<Concert.Track>)Delegate.Combine(component.MusicClicked, new Action<Concert.Track>(OnClickMusic));
		});
		_cancelButton.Text = T._("닫기");
		_confirmButton.Text = T._("합주 시작");
		Selectable helpButton = _helpButton;
		helpButton.Clicked = (Action)Delegate.Combine(helpButton.Clicked, new Action(ShowHelpPopup));
		Selectable clearButton = _clearButton;
		clearButton.Clicked = (Action)Delegate.Combine(clearButton.Clicked, (Action)delegate
		{
			if (_concert == null)
			{
				Hide();
			}
			else if (IsHost())
			{
				UIManager.MessageBox.Show(T._("합주 모집을 포기하시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						MusicManager.FinishConcert(_concert.Bandstand);
						Hide();
					}
				});
			}
			else
			{
				UIManager.MessageBox.Show(T._("참가 중인 합주그룹을 떠나시겠습니까?"), delegate(bool ok)
				{
					if (ok)
					{
						MusicManager.UnregisterConcert(_concert.Bandstand);
						Hide();
					}
				});
			}
		});
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(Hide));
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, (Action)delegate
		{
			if (_concert != null && IsHost())
			{
				if (!_concert.IsPlayable(out var _))
				{
					UIManager.SystemMsg(T._("연주에 필요한 악기와 악보를 준비해주세요."));
				}
				else
				{
					MusicManager.PlayConcert(_concert.Bandstand);
				}
			}
		});
		Singleton<ArtifactManager>.Instance().StateChanged += UpdateBandstand;
	}

	protected override bool IsShowable()
	{
		return _concert != null;
	}

	protected override void OnShow()
	{
		base.OnShow();
		_musics = null;
	}

	protected override void OnHide()
	{
		base.OnHide();
		_concert = null;
		_lastSelectedInstrumentId = null;
		_reservedBandstand = null;
	}

	private void UpdateBandstand([NotNull] Artifact artifact)
	{
		if (!base.IsVisible)
		{
			if (!string.IsNullOrEmpty(_reservedBandstand) && Time.time < _reserveValidAt && artifact.EntityId == _reservedBandstand)
			{
				Set(artifact);
				Show();
			}
			return;
		}
		if (_concert == null)
		{
			Hide();
			return;
		}
		PropKey bandstand = _concert.Bandstand;
		if (!(bandstand.EntityId != artifact.EntityId))
		{
			Bandstand? bandstand2 = artifact.ArtifactState.Bandstand;
			if (!bandstand2.HasValue || string.IsNullOrEmpty(bandstand2.Value.Host))
			{
				Hide();
				return;
			}
			if (_concert.Host != bandstand2.Value.Host)
			{
				Hide();
				return;
			}
			_concert.Set(bandstand2.Value);
			OnBandstandUpdate();
		}
	}

	private void OnBandstandUpdate()
	{
		if (IsHost())
		{
			_clearButton.Disabled = false;
			return;
		}
		bool flag = false;
		if (_concert != null)
		{
			Concert.Track[] tracks = _concert.Tracks;
			foreach (Concert.Track track in tracks)
			{
				string value = track.PlayerId.Value;
				if (!string.IsNullOrEmpty(value) && value == GameManager.PlayerId)
				{
					flag = true;
					break;
				}
			}
		}
		_clearButton.Disabled = !flag;
	}

	public void SetReserve(string bandstandId)
	{
		_reservedBandstand = bandstandId;
		_reserveValidAt = Time.time + 3f;
	}

	public void Set(Artifact artifact)
	{
		_concert = null;
		_reservedBandstand = null;
		if (artifact == null)
		{
			return;
		}
		Bandstand? bandstand = artifact.ArtifactState.Bandstand;
		if (!bandstand.HasValue)
		{
			return;
		}
		_concert = new Concert(artifact.GetPropKey());
		_concert.Set(bandstand.Value);
		_scrollView.Nodes.BeginLoad();
		Concert.Track[] tracks = _concert.Tracks;
		foreach (Concert.Track track in tracks)
		{
			ConcertTrackWidget component = _scrollView.Nodes.GetNext().GetComponent<ConcertTrackWidget>();
			component.SetTrack(track, _concert.Host);
		}
		_scrollView.Nodes.EndLoad();
		_confirmButton.gameObject.SetActive(IsHost());
		_clearButtonLabel.text = string.Format("{0} [icon=icon_chat_exit:1.5]", (!IsHost()) ? T._("합주 탈퇴") : T._("합주 해산"));
		if (string.IsNullOrEmpty(_concert.Host))
		{
			_titleLabel.text = T._("합주 모집");
		}
		else
		{
			bool waiting = true;
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(_concert.Host, delegate(Durango.Player.PlayerInfo info)
			{
				if (info.Valid)
				{
					_titleLabel.text = T._("{0}님의 합주 모집", info.Name);
					waiting = false;
				}
			});
			if (waiting)
			{
				_titleLabel.text = T._("합주 모집");
			}
		}
		OnBandstandUpdate();
	}

	protected override void UpdateLayout()
	{
		base.UpdateLayout();
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		_scrollView.ResetPosition();
	}

	private bool IsHost()
	{
		if (_concert == null)
		{
			return false;
		}
		return _concert.IsHost();
	}

	private void OnClickSlot(Concert.Track track)
	{
		if (_concert == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(track.PlayerId))
		{
			if (track.PlayerId.Value == GameManager.PlayerId)
			{
				MusicManager.UnregisterConcert(_concert.Bandstand);
			}
			return;
		}
		Durango.Logic.Item.Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
		ItemData itemData = ((!string.IsNullOrEmpty(_lastSelectedInstrumentId)) ? playerInventory.Find(_lastSelectedInstrumentId) : null);
		if (itemData == null)
		{
			itemData = playerInventory.Items.Find((ItemData item) => item.GetPerformanceData("instrument").HasValue);
		}
		if (itemData == null)
		{
			UIManager.SystemMsg(T._("연주에 사용할 악기가 필요합니다."));
			return;
		}
		_lastSelectedInstrumentId = itemData.Id;
		MusicManager.RegisterConcert(_concert.Bandstand, track.Index, itemData.Id);
	}

	private void OnClickInstrument(Concert.Track track)
	{
		if (_concert == null || string.IsNullOrEmpty(track.PlayerId))
		{
			return;
		}
		if (track.PlayerId != GameManager.PlayerId)
		{
			string id = track.Timbre;
			MusicManager.Instrument instrument = Singleton<MusicManager>.Instance().GetInstrument(id);
			if (instrument != null)
			{
				WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
				widgetTooltipControl.Set(null, instrument.Name);
				widgetTooltipControl.Direction = TooltipDirection.Horizontal;
				widgetTooltipControl.Sign = -1;
				widgetTooltipControl.Show(5f);
			}
			return;
		}
		int order = track.Index;
		PopupItemSelector popupItemSelector = UIManager.Popup.Tooltip<PopupItemSelector>();
		popupItemSelector.MyInventory().Title(T._("연주 악기 선택")).AutoFillText(T._("확인"))
			.Filter((ItemData item) => item.GetPerformanceData("instrument").HasValue)
			.OnConfirmed(delegate(ItemData item)
			{
				if (_concert != null && item != null)
				{
					_lastSelectedInstrumentId = item.Id;
					MusicManager.RegisterConcert(_concert.Bandstand, order, item.Id);
				}
			})
			.Show();
	}

	private void OnClickMusic(Concert.Track track)
	{
		if (!IsHost())
		{
			return;
		}
		if (_musics == null)
		{
			Singleton<MusicManager>.Instance().GetMusics(delegate(List<KeyValuePair<MusicId, Messages.Music>> musics)
			{
				_musics = musics;
				SelectTrackMusic(track);
			});
		}
		else
		{
			SelectTrackMusic(track);
		}
	}

	private void SelectTrackMusic(Concert.Track track)
	{
		if (_concert == null)
		{
			return;
		}
		if (_musics == null || _musics.Count == 0)
		{
			UIManager.SystemMsg(T._("악보가 없습니다."));
			return;
		}
		GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
		genericSelector.SetTitle(T._("악보를 선택해주세요."));
		foreach (KeyValuePair<MusicId, Messages.Music> music in _musics)
		{
			genericSelector.AddItem(music.Value.Name);
		}
		KeyValuePair<MusicId, Messages.Music>[] array = _musics.ToArray();
		int order = track.Index;
		genericSelector.SetSelected(delegate(int index)
		{
			if (_concert != null && index >= 0 && index < array.Length)
			{
				MusicId key = array[index].Key;
				Messages.Music value = array[index].Value;
				MusicManager.SetConcertMusic(_concert.Bandstand, order, key, value.Name);
			}
		});
		genericSelector.Show();
	}

	private static void ShowHelpPopup()
	{
		SimpleTextListPopup simpleTextListPopup = UIManager.Popup.Tooltip<SimpleTextListPopup>();
		simpleTextListPopup.Set(T._("합주도움말"), new string[5]
		{
			T._("모닥불에서 합주버튼을 선택하여 합주를 이끌 수 있습니다.\n최초 합주버튼 누른 사람이 지휘자가 되며 악보 등록과 합주시작 권한을 갖습니다.\n단, 1시간이상 별도 행동을 하지 않는 경우 합주를 시도하는 다른 사람에게 권한이 이양될 수 있습니다."),
			T._("합주에 참가하려면 가방 내 악기를 보유해야 합니다.\n악보는 지휘자가 등록할 수 있으며, 악기는 캐릭터초상화 좌측에 악기 아이콘을 눌러 변경할 수 있습니다."),
			T._("악보와 악기가 준비되어 있는 참가자만 합주를 진행할 수 있습니다.\n준비하는 모닥불에서 일정거리 이상 떨어지면 합주시작 시 합주에서 제외됩니다."),
			T._("닫기버튼을 누르면 악기, 참가여부를 모두 유지한 상태에서 합주모집 화면을 종료합니다.\n종료한 상태에서 자유롭게 채팅 및 이동 등을 할 수 있으며 합주를 진행했던 모닥불을 선택하여 합주모집으로 돌아올 수 있습니다."),
			T._("합주해산은 지휘자만 할 수 있으며 참가자를 포함하여 진행 중인 합주 모집을 취소합니다.\n합주탈퇴는 참가자만 할 수 있으며 본인의 악기, 참가여부를 취소하고 합주 모집에서 이탈합니다.")
		});
		simpleTextListPopup.Show();
	}
}
