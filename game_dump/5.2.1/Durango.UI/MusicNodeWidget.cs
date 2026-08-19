using System;
using Durango.Logic.Clusters;
using Durango.Player;
using Durango.UI.Control;
using Durango.Utils;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class MusicNodeWidget : UIWidget
{
	[SerializeField]
	private SelectableWidget _playButton;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private SelectableButton _removeButton;

	[SerializeField]
	private SelectableButton _shareButton;

	[SerializeField]
	private SelectableButton _editButton;

	private MusicId _musicId;

	public event Action<MusicId> MusicRemoved;

	public event Action<MusicId> MusicShared;

	public event Action<MusicId> MusicEdited;

	public event Action<MusicId> MusicPlayed;

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			SelectableWidget playButton = _playButton;
			playButton.Clicked = (Action)Delegate.Combine(playButton.Clicked, new Action(OnMusicPlay));
			SelectableButton removeButton = _removeButton;
			removeButton.Clicked = (Action)Delegate.Combine(removeButton.Clicked, new Action(OnRemoveMusic));
			SelectableButton shareButton = _shareButton;
			shareButton.Clicked = (Action)Delegate.Combine(shareButton.Clicked, new Action(OnShareMusic));
			SelectableButton editButton = _editButton;
			editButton.Clicked = (Action)Delegate.Combine(editButton.Clicked, new Action(OnEditMusic));
		}
	}

	public void Set(MusicId id, Music music)
	{
		_musicId = id;
		_nameLabel.text = music.Name;
		if (KUtility.GetSize(music.Data) == 0)
		{
			_shareButton.Disabled = true;
			_editButton.Disabled = true;
			_playButton.Disabled = true;
		}
		else if (id.Slot.HasValue)
		{
			_shareButton.Disabled = GameManager.ClusterMode != Mode.Online;
			_editButton.Disabled = false;
			_playButton.Disabled = false;
		}
		else
		{
			_shareButton.Disabled = GameManager.ClusterMode != Mode.Online;
			_editButton.Disabled = true;
			_playButton.Disabled = false;
		}
		if (string.IsNullOrEmpty(music.Publisher))
		{
			return;
		}
		if (music.Publisher == GameManager.PlayerId)
		{
			Singleton<MusicManager>.Instance().GetSharedMusic(id.SharedId, delegate(SharedMusic m)
			{
				_nameLabel.text = $"{music.Name}  <bar/>  [icon=icon_share] {m.RefCount}";
			});
			return;
		}
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(music.Publisher, delegate(Durango.Player.PlayerInfo info)
		{
			if (info.Valid)
			{
				_nameLabel.text = music.Name + "  <bar/>  <em>" + info.Name + "</em> [b3b2af]" + info.GetFreq() + "[-]";
			}
		});
	}

	private void OnRemoveMusic()
	{
		if (this.MusicRemoved != null)
		{
			this.MusicRemoved(_musicId);
		}
	}

	private void OnShareMusic()
	{
		if (this.MusicShared != null)
		{
			this.MusicShared(_musicId);
		}
	}

	private void OnEditMusic()
	{
		if (this.MusicEdited != null)
		{
			this.MusicEdited(_musicId);
		}
	}

	private void OnMusicPlay()
	{
		if (this.MusicPlayed != null)
		{
			this.MusicPlayed(_musicId);
		}
	}
}
