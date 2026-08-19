using System;
using Durango.Network;
using L10N;
using Messages;
using Yaml;
using Yaml.Util;

namespace Durango.Logic;

public class PvpIslandSystem : GameSystem<PvpIslandSystem>
{
	public S02PVPStart TimeInfo { get; private set; }

	public int TotalPlayerCount { get; private set; }

	public event Action GameStarted;

	public event Action BattleStarted;

	public event Action<int> PlayerCountUpdated;

	public event Action<S02PVPDead> PlayerDied;

	public event Action<S02PVPKill> Kill;

	public event Action<S02PVPFinish> Win;

	private void Awake()
	{
		Connections.Frontend.On(delegate(S02PVPStart msg, PacketHeader header)
		{
			TimeInfo = msg;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			if (this.BattleStarted != null)
			{
				double num = TimeInfo.GameStartAt - predictedServerTime;
				KUtility.DelayedCall(this, this.BattleStarted, (float)num);
			}
			double num2 = TimeInfo.FirstPlayerEnteredAt + (double)Singleton<Constants>.Instance.Season2.StormSignTime - predictedServerTime;
			KUtility.DelayedCall(this, delegate
			{
				UIManager.SystemMsg(T._("곧 폭풍우가 몰아칠 것 같다."));
			}, (float)num2);
			if (this.GameStarted != null)
			{
				this.GameStarted();
			}
		});
		Connections.Radiotower.On<S02PVPAnnounceLeave>(delegate
		{
			Connections.Frontend.Send(default(S02PVPRefresh));
		});
		TotalPlayerCount = -1;
		Connections.Frontend.On(delegate(S02PVPStatus msg, PacketHeader header)
		{
			if (TotalPlayerCount < 0)
			{
				TotalPlayerCount = msg.RemainSurvivorCount;
			}
			if (this.PlayerCountUpdated != null)
			{
				this.PlayerCountUpdated(msg.RemainSurvivorCount);
			}
		});
		Connections.Frontend.On(delegate(S02PVPDead msg, PacketHeader header)
		{
			this.BattleStarted = null;
			ExitWithDelay();
			if (this.PlayerDied != null)
			{
				this.PlayerDied(msg);
			}
		});
		Connections.Frontend.On(delegate(S02PVPKill msg, PacketHeader header)
		{
			if (this.Kill != null)
			{
				this.Kill(msg);
			}
		});
		Connections.Frontend.On(delegate(S02PVPFinish msg, PacketHeader header)
		{
			this.BattleStarted = null;
			ExitWithDelay();
			if (this.Win != null)
			{
				this.Win(msg);
			}
		});
	}

	private void ExitWithDelay()
	{
		KUtility.DelayedCall(this, delegate
		{
			Connections.Frontend.Send(default(S02Leave));
		}, 25f);
	}
}
