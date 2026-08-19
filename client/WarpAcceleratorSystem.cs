using System;
using System.Collections.Generic;
using Durango.Network;
using Messages;
using Shared.Accelerator;

public class WarpAcceleratorSystem : GameSystem<WarpAcceleratorSystem>
{
	private readonly List<WarpAcceleratorInfo> _warpAccelerators = new List<WarpAcceleratorInfo>();

	private WarpAcceleratorAcquisition _warpAcceleratorAcquisition;

	public List<WarpAcceleratorInfo> WarpAccelerators => _warpAccelerators;

	public event Action WarpAcceleratorsUpdated;

	private void Awake()
	{
		Connections.Frontend.On<WarpAcceleratorsInRegion>(OnWarpAcceleratorsInRegion);
		Connections.Frontend.On<WarpAcceleratorInfo>(OnWarpAcceleratorInfo);
		Connections.Frontend.On<WarpAcceleratorAcquisition>(OnWarpAcceleratorAcquisition);
		Connections.Radiotower.On<WarpAcceleratorsInRegion>(OnWarpAcceleratorsInRegion);
		Connections.Radiotower.On<WarpAcceleratorInfo>(OnWarpAcceleratorInfo);
		Artifact.ArtifactStateChanged += OnChangeArtifactState;
		GameSystem<StatisticsSystem>.Instance().Rewarded += delegate(Rewarded msg)
		{
			if (msg.Effect is WarpAccelerationRewardsEffect)
			{
				_warpAcceleratorAcquisition = ((WarpAccelerationRewardsEffect)msg.Effect).Acquisition;
			}
		};
	}

	public WarpAcceleratorInfo? GetMyWarpAcceleratorInfo()
	{
		string playerId = GameManager.PlayerId;
		WarpAcceleratorInfo? result = null;
		foreach (WarpAcceleratorInfo warpAccelerator in _warpAccelerators)
		{
			Messages.WarpAccelerator warpaccelerator = warpAccelerator.Warpaccelerator;
			if (warpaccelerator.Participants == null || Array.IndexOf(warpaccelerator.Participants, playerId) == -1)
			{
				continue;
			}
			if (!result.HasValue)
			{
				result = warpAccelerator;
				continue;
			}
			AcceleratorStatus status = warpAccelerator.Warpaccelerator.Status;
			if (status == AcceleratorStatus.Waiting || status == AcceleratorStatus.Processing || status == AcceleratorStatus.Intermission)
			{
				result = warpAccelerator;
			}
		}
		return result;
	}

	public Pair<int, int> GetWarpMatterAcquisition()
	{
		Pair<int, int> acquired = _warpAcceleratorAcquisition.WarpMatter.Acquired;
		if (Connections.Frontend.GetPredictedServerTime() < _warpAcceleratorAcquisition.WarpMatter.RefreshAt)
		{
			return new Pair<int, int>(Math.Max(0, acquired.Item2 - acquired.Item1), acquired.Item2);
		}
		return new Pair<int, int>(acquired.Item2, acquired.Item2);
	}

	private void OnWarpAcceleratorsInRegion(WarpAcceleratorsInRegion msg, PacketHeader header)
	{
		_warpAccelerators.Clear();
		_warpAccelerators.AddRange(msg.Warpaccelerators);
		if (this.WarpAcceleratorsUpdated != null)
		{
			this.WarpAcceleratorsUpdated();
		}
	}

	private void OnWarpAcceleratorInfo(WarpAcceleratorInfo msg, PacketHeader header)
	{
		int num = -1;
		for (int i = 0; i < _warpAccelerators.Count; i++)
		{
			if (_warpAccelerators[i].EntityId == msg.EntityId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			_warpAccelerators.Add(msg);
		}
		else
		{
			_warpAccelerators[num] = msg;
		}
		if (this.WarpAcceleratorsUpdated != null)
		{
			this.WarpAcceleratorsUpdated();
		}
	}

	private void OnWarpAcceleratorAcquisition(WarpAcceleratorAcquisition msg, PacketHeader header)
	{
		_warpAcceleratorAcquisition = msg;
	}

	private void OnChangeArtifactState(Artifact artifact)
	{
		string entityId = artifact.EntityId;
		ArtifactState artifactState = artifact.ArtifactState;
		int num = -1;
		for (int i = 0; i < _warpAccelerators.Count; i++)
		{
			if (_warpAccelerators[i].EntityId == entityId)
			{
				num = i;
				break;
			}
		}
		Messages.WarpAccelerator? warpaccelerator = artifactState.Warpaccelerator;
		if (!warpaccelerator.HasValue)
		{
			if (num == -1)
			{
				return;
			}
			_warpAccelerators.RemoveAt(num);
		}
		else
		{
			WarpAcceleratorInfo warpAcceleratorInfo = default(WarpAcceleratorInfo);
			warpAcceleratorInfo.EntityId = artifact.EntityId;
			warpAcceleratorInfo.Tile = artifact.WorldTile;
			warpAcceleratorInfo.Warpaccelerator = artifactState.Warpaccelerator.Value;
			WarpAcceleratorInfo warpAcceleratorInfo2 = warpAcceleratorInfo;
			if (num == -1)
			{
				_warpAccelerators.Add(warpAcceleratorInfo2);
			}
			else
			{
				_warpAccelerators[num] = warpAcceleratorInfo2;
			}
		}
		if (this.WarpAcceleratorsUpdated != null)
		{
			this.WarpAcceleratorsUpdated();
		}
	}
}
