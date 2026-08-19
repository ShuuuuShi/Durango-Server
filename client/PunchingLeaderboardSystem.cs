using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using L10N;
using Messages;

public class PunchingLeaderboardSystem : GameSystem<PunchingLeaderboardSystem>
{
	public enum Category
	{
		[T.EnumName("최근")]
		Recently,
		[T.EnumName("섬")]
		Region,
		[T.EnumName("전체")]
		Global
	}

	private class PlayerInfoCollector
	{
		private readonly List<string> _ids = new List<string>();

		public void Request(Action response, params Leaderboard[] leaderboards)
		{
			_ids.Clear();
			for (int i = 0; i < leaderboards.Length; i++)
			{
				Leaderboard leaderboard = leaderboards[i];
				LeaderboardContent[] contents = leaderboard.Contents;
				for (int j = 0; j < contents.Length; j++)
				{
					LeaderboardContent leaderboardContent = contents[j];
					_ids.Add(leaderboardContent.UserId);
				}
			}
			_ids.Add(GameManager.PlayerId);
			Singleton<PlayerInfoManager>.Instance().RequestPlayerInfos(_ids, delegate
			{
				response();
			});
		}
	}

	private static readonly LeaderboardContent[] Empty = new LeaderboardContent[0];

	private readonly PlayerInfoCollector _playerInfoCollector = new PlayerInfoCollector();

	private readonly Dictionary<int, LeaderboardContent[]> _leaderboards = new Dictionary<int, LeaderboardContent[]>();

	public LeaderboardContent? MyScore { get; private set; }

	public event Action LeaderboardsUpdated;

	private void Awake()
	{
		_leaderboards[0] = Empty;
		_leaderboards[1] = Empty;
		_leaderboards[2] = Empty;
		Connections.Frontend.On<PunchMachineLeaderboards>(OnPunchMachineLeaderboards);
	}

	[NotNull]
	public LeaderboardContent[] GetLeaderboard(Category category)
	{
		return _leaderboards[(int)category];
	}

	public void UpdateLeaderboards([NotNull] Artifact artifact)
	{
		Connections.Frontend.Send(new GetPunchMachineLeaderboard
		{
			EntityId = artifact.EntityId,
			Tile = artifact.WorldTile
		});
	}

	private void OnPunchMachineLeaderboards(PunchMachineLeaderboards msg, PacketHeader header)
	{
		_playerInfoCollector.Request(delegate
		{
			_leaderboards[0] = msg.RegionRecentLeaderboard.Contents;
			_leaderboards[1] = msg.RegionTotalLeaderboard.Contents;
			_leaderboards[2] = msg.GlobalLeaderboard.Contents;
			MyScore = msg.MyScore;
			if (this.LeaderboardsUpdated != null)
			{
				this.LeaderboardsUpdated();
			}
		}, msg.RegionRecentLeaderboard, msg.RegionTotalLeaderboard, msg.GlobalLeaderboard);
	}
}
