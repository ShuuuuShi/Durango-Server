using System;
using Durango.Network;
using Durango.Player;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;

namespace Durango.Logic.Party;

public class Member
{
	private float _life;

	private float _stamina;

	private double _expiresAt;

	private PlayerBehavior _player;

	public string EntityId { get; private set; }

	public string RegionId { get; private set; }

	public bool IsLeader { get; private set; }

	public bool IsAccepted { get; private set; }

	public bool IsOffline
	{
		get
		{
			if (PlayerBehavior.LocalPlayer.EntityId == EntityId || _player != null)
			{
				return false;
			}
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			return predictedServerTime >= _expiresAt;
		}
	}

	public bool IsAlive => Life > 0f;

	public float Life
	{
		get
		{
			if (_player != null && _player.Life != null)
			{
				return GetGaugeRatio(_player.Life);
			}
			return _life;
		}
	}

	public float Stamina
	{
		get
		{
			if (_player != null)
			{
				Gauge stamina = _player.Stamina;
				if (stamina != null)
				{
					return GetGaugeRatio(stamina);
				}
			}
			return _stamina;
		}
	}

	public Point2 Tile { get; private set; }

	[CanBeNull]
	public Durango.Player.PlayerInfo PlayerInfo { get; private set; }

	public string RegionName { get; private set; }

	public event Action<Durango.Player.PlayerInfo> PlayerInfoUpdated;

	public Member(string entityId, bool isLeader, bool isAccepted)
	{
		EntityId = entityId;
		IsLeader = isLeader;
		IsAccepted = isAccepted;
		RegionName = string.Empty;
		Singleton<PlayerInfoManager>.Instance().RequestPlayerInfo(entityId, delegate(Durango.Player.PlayerInfo info)
		{
			PlayerInfo = info;
			if (string.IsNullOrEmpty(RegionName))
			{
				RegionName = info.RegionName;
			}
			if (this.PlayerInfoUpdated != null)
			{
				this.PlayerInfoUpdated(PlayerInfo);
			}
		});
	}

	public void SetPlayer(PlayerBehavior player)
	{
		_player = player;
	}

	public void SetStatus(PartierStatus status)
	{
		if (status.Health.y > 0f)
		{
			_life = status.Health.x / status.Health.y;
		}
		else
		{
			_life = 0f;
		}
		if (status.Energy.y > 0f)
		{
			_stamina = status.Energy.x / status.Energy.y;
		}
		else
		{
			_stamina = 0f;
		}
		_expiresAt = ((!status.IsOnline) ? 0.0 : status.ExpiresAt);
		Tile = status.Tile;
		if (RegionId != status.RegionId)
		{
			RegionId = status.RegionId;
			GameSystem<MapSystem>.Instance().GetRegion(RegionId, delegate(Region region)
			{
				RegionName = region.Name;
			});
		}
	}

	private static float GetGaugeRatio(Gauge gauge)
	{
		float num = gauge.RealMax();
		if (num > 0f)
		{
			return gauge.Get() / num;
		}
		return 0f;
	}
}
