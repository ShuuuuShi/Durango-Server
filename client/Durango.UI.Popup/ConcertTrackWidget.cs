using System;
using Durango.Logic.Music;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI.Popup;

public class ConcertTrackWidget : UIWidget
{
	public Action<Concert.Track> InstrumentClicked;

	public Action<Concert.Track> MusicClicked;

	public Action<Concert.Track> SlotClicked;

	[SerializeField]
	private UIWidget _playerWidget;

	[SerializeField]
	private Selectable _instrumentButton;

	[SerializeField]
	private UISprite _instrumentSprite;

	[SerializeField]
	private Selectable _playerButton;

	[SerializeField]
	private UITexture _portraitTexture;

	[SerializeField]
	private Texture _portraitMask;

	[SerializeField]
	private GameObject[] _emptyPlayerObject;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _infoLabel;

	[SerializeField]
	private Selectable _musicButton;

	[SerializeField]
	private UILabel _musicNameLabel;

	[SerializeField]
	private GameObject[] _isMeObjects;

	[SerializeField]
	private GameObject[] _concertHostObjects;

	[SerializeField]
	private RectLayout _layout;

	private string _host;

	private Concert.Track _track;

	private float _playerInfoRefreshAt;

	private bool _isLayoutDirty;

	protected override void OnStart()
	{
		base.OnStart();
		if (!Application.isPlaying)
		{
			return;
		}
		Selectable musicButton = _musicButton;
		musicButton.Clicked = (Action)Delegate.Combine(musicButton.Clicked, (Action)delegate
		{
			if (MusicClicked != null)
			{
				MusicClicked(_track);
			}
		});
		Selectable playerButton = _playerButton;
		playerButton.Clicked = (Action)Delegate.Combine(playerButton.Clicked, (Action)delegate
		{
			if (SlotClicked != null)
			{
				SlotClicked(_track);
			}
		});
		Selectable instrumentButton = _instrumentButton;
		instrumentButton.Clicked = (Action)Delegate.Combine(instrumentButton.Clicked, (Action)delegate
		{
			if (InstrumentClicked != null)
			{
				InstrumentClicked(_track);
			}
		});
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (_isLayoutDirty)
		{
			_isLayoutDirty = false;
			_layout.UpdateLayout();
		}
		if (_playerInfoRefreshAt > 0f && _playerInfoRefreshAt < Time.time)
		{
			UpdatePlayerWidgetState();
		}
	}

	private void UpdatePlayerWidgetState()
	{
		if (_track == null)
		{
			_playerInfoRefreshAt = 0f;
		}
		else if (string.IsNullOrEmpty(_track.PlayerId))
		{
			_playerInfoRefreshAt = 0f;
			_playerWidget.alpha = 1f;
		}
		else
		{
			_playerInfoRefreshAt = Time.time + 5f;
			PlayerBehavior playerIncludeLocalPlayer = Singleton<PlayerManager>.Instance().GetPlayerIncludeLocalPlayer(_track.PlayerId);
			_playerWidget.alpha = ((!(playerIncludeLocalPlayer == null)) ? 1f : 0.5f);
		}
	}

	public void SetTrack([NotNull] Concert.Track track, string host)
	{
		_track = track;
		_host = host;
		RefershPlayerInfo();
		SetInstrument(_track.Timbre);
		SetMusicName(_track.MusicName);
		Observable<string> playerId = _track.PlayerId;
		playerId.Changed = (Action<string>)Delegate.Combine(playerId.Changed, (Action<string>)delegate
		{
			RefershPlayerInfo();
		});
		Observable<string> timbre = _track.Timbre;
		timbre.Changed = (Action<string>)Delegate.Combine(timbre.Changed, new Action<string>(SetInstrument));
		Observable<string> musicName = _track.MusicName;
		musicName.Changed = (Action<string>)Delegate.Combine(musicName.Changed, new Action<string>(SetMusicName));
	}

	private bool IsHostPlayer()
	{
		if (string.IsNullOrEmpty(_host))
		{
			return false;
		}
		if (string.IsNullOrEmpty(_track.PlayerId))
		{
			return false;
		}
		return _host == _track.PlayerId.Value;
	}

	private void RefershPlayerInfo()
	{
		bool waiting = true;
		_track.GetPlayerInfo(delegate(PlayerInfo info)
		{
			waiting = false;
			SetPlayer(info);
		});
		if (waiting)
		{
			SetPlayer(null);
		}
	}

	private void SetPlayer(PlayerInfo info)
	{
		_isLayoutDirty = true;
		bool active = GameManager.PlayerId == _track.PlayerId;
		GameObject[] isMeObjects = _isMeObjects;
		foreach (GameObject gameObject in isMeObjects)
		{
			gameObject.SetActive(active);
		}
		UpdatePlayerWidgetState();
		if (info == null || !info.Valid)
		{
			if (_track != null && !string.IsNullOrEmpty(_track.PlayerId))
			{
				_portraitTexture.gameObject.SetActive(value: false);
				_nameLabel.text = string.Empty;
				_infoLabel.text = string.Empty;
				GameObject[] emptyPlayerObject = _emptyPlayerObject;
				foreach (GameObject gameObject2 in emptyPlayerObject)
				{
					gameObject2.gameObject.SetActive(value: false);
				}
			}
			else
			{
				_portraitTexture.gameObject.SetActive(value: false);
				_nameLabel.text = string.Empty;
				_infoLabel.text = string.Empty;
				GameObject[] emptyPlayerObject2 = _emptyPlayerObject;
				foreach (GameObject gameObject3 in emptyPlayerObject2)
				{
					gameObject3.gameObject.SetActive(value: true);
				}
			}
		}
		else
		{
			_portraitTexture.gameObject.SetActive(value: true);
			GameObject[] emptyPlayerObject3 = _emptyPlayerObject;
			foreach (GameObject gameObject4 in emptyPlayerObject3)
			{
				gameObject4.gameObject.SetActive(value: false);
			}
			PortraitBuilder.Argument portraitArgument = info.GetPortraitArgument();
			portraitArgument.Mask = _portraitMask;
			PortraitBuilder.Set(portraitArgument, _portraitTexture);
			_nameLabel.text = ((!IsHostPlayer()) ? info.Name : $"[icon=crown] {info.Name}");
			_infoLabel.text = $"{LocalizeUtil.FormatLevel(info.Level)} <bar/> {info.GetFreq()}";
		}
	}

	private void SetInstrument(string instrument)
	{
		_isLayoutDirty = true;
		if (string.IsNullOrEmpty(_track.PlayerId))
		{
			_instrumentSprite.gameObject.SetActive(value: false);
			return;
		}
		if (string.IsNullOrEmpty(instrument))
		{
			_instrumentSprite.gameObject.SetActive(value: false);
			return;
		}
		_instrumentSprite.gameObject.SetActive(value: true);
		MusicManager.Instrument instrument2 = Singleton<MusicManager>.Instance().GetInstrument(instrument);
		if (instrument2 == null)
		{
			_instrumentSprite.spriteName = string.Empty;
		}
		else
		{
			_instrumentSprite.spriteName = instrument2.Icon.sprite;
		}
	}

	private void SetMusicName(string musicName)
	{
		_isLayoutDirty = true;
		bool flag = _host == GameManager.PlayerId;
		GameObject[] concertHostObjects = _concertHostObjects;
		foreach (GameObject gameObject in concertHostObjects)
		{
			gameObject.SetActive(flag);
		}
		if (string.IsNullOrEmpty(musicName))
		{
			if (flag)
			{
				_musicNameLabel.text = string.Format("[858480]{0}[-]  [89857A][icon=chat_plus_icon:1.6][-]", T._("악보 선택"));
			}
			else
			{
				_musicNameLabel.text = string.Empty;
			}
		}
		else
		{
			_musicNameLabel.text = musicName;
		}
	}
}
