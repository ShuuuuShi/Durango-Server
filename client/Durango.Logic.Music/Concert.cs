using System;
using System.Collections.Generic;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;

namespace Durango.Logic.Music;

public class Concert
{
	public class Track
	{
		public readonly int Index;

		public readonly Observable<string> PlayerId;

		public readonly Observable<string> Timbre;

		public readonly Observable<string> MusicName;

		private Durango.Player.PlayerInfo _playerInfo;

		public Track(int index)
		{
			Index = index;
			PlayerId = new Observable<string>();
			Observable<string> playerId = PlayerId;
			playerId.Changed = (Action<string>)Delegate.Combine(playerId.Changed, (Action<string>)delegate
			{
				_playerInfo = null;
			});
			Timbre = new Observable<string>();
			MusicName = new Observable<string>();
		}

		public bool IsPlayable(out bool notReady)
		{
			bool flag = !string.IsNullOrEmpty(MusicName);
			bool flag2 = !string.IsNullOrEmpty(PlayerId);
			if (flag && flag2)
			{
				notReady = string.IsNullOrEmpty(Timbre);
				return true;
			}
			notReady = false;
			return false;
		}

		public void SetPlayer(string id)
		{
			PlayerId.Value = id;
		}

		public void SetTimbre(string timbre)
		{
			Timbre.Value = timbre;
		}

		public void SetMusicName(string musicName)
		{
			MusicName.Value = musicName;
		}

		public void GetPlayerInfo([NotNull] Action<Durango.Player.PlayerInfo> callback)
		{
			if (string.IsNullOrEmpty(PlayerId.Value))
			{
				_playerInfo = null;
				callback(null);
				return;
			}
			if (_playerInfo != null && _playerInfo.Valid && _playerInfo.EntityId == PlayerId.Value)
			{
				callback(_playerInfo);
				return;
			}
			_playerInfo = null;
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(PlayerId, delegate(Durango.Player.PlayerInfo info)
			{
				if (info.Valid && !(info.EntityId != PlayerId))
				{
					_playerInfo = info;
					callback(info);
				}
			});
		}
	}

	public const int MaxTrackCount = 6;

	public readonly PropKey Bandstand;

	public string Host { get; private set; }

	[NotNull]
	public Track[] Tracks { get; private set; }

	public Concert(PropKey bandstand)
	{
		Bandstand = bandstand;
		Tracks = new Track[6];
		for (int i = 0; i < Tracks.Length; i++)
		{
			Tracks[i] = new Track(i);
		}
	}

	public bool IsPlayable(out int playableCount)
	{
		playableCount = 0;
		Track[] tracks = Tracks;
		foreach (Track track in tracks)
		{
			if (track.IsPlayable(out var notReady))
			{
				playableCount++;
			}
			else if (notReady)
			{
				return false;
			}
		}
		return playableCount > 0;
	}

	public void Set(Bandstand bandstand)
	{
		Host = bandstand.Host;
		Dictionary<int, Band> bands = bandstand.Bands;
		for (int i = 0; i < Tracks.Length; i++)
		{
			Track track = Tracks[i];
			if (bands != null && bands.TryGetValue(i, out var value))
			{
				track.SetPlayer(value.Musician);
				track.SetTimbre(value.Timbre);
				track.SetMusicName(value.MusicName);
			}
			else
			{
				track.SetPlayer(null);
				track.SetTimbre(null);
				track.SetMusicName(null);
			}
		}
	}

	public bool IsHost()
	{
		if (string.IsNullOrEmpty(Host))
		{
			return false;
		}
		return Host == GameManager.PlayerId;
	}
}
