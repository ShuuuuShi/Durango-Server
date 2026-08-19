using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Durango.Logic.Clusters;
using Durango.Network;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using Shared.Region;

namespace Durango.Offline;

public class GameServer
{
	public const int DefaultPort = 8391;

	private readonly Listener _listener;

	private readonly PlayerContext _playerCtx;

	private readonly Dictionary<string, PlayerContext> _playerContexts = new Dictionary<string, PlayerContext>();

	private readonly List<Connection> _connections = new List<Connection>();

	private readonly Dictionary<Connection, string> _connectionDict = new Dictionary<Connection, string>();

	public World World { get; private set; }

	public int Port { get; private set; }

	public GameServer(WorldContext worldCtx, PlayerContext playerCtx)
	{
		_listener = new Listener();
		Port = Server.GetIslandPort() + 1;
		_listener.Start(Port);
		_listener.ClientAccepted += Listener_ClientAccepted;
		World = new World(worldCtx);
		_playerCtx = playerCtx;
	}

	public void Close()
	{
		try
		{
			_listener.Close();
			for (int num = _connections.Count - 1; num >= 0; num--)
			{
				_connections[num].Close();
			}
			_connections.Clear();
			World.Stop();
		}
		catch (Exception)
		{
		}
	}

	public void Process()
	{
		_listener.Process();
		for (int num = _connections.Count - 1; num >= 0; num--)
		{
			_connections[num].Process();
		}
		World.Process();
	}

	public bool Register(PlayerContext context)
	{
		if (context != null && !string.IsNullOrEmpty(context.EntityId))
		{
			_playerContexts[context.EntityId] = context;
			return true;
		}
		return false;
	}

	private void Listener_ClientAccepted(Socket socket)
	{
		Connection connection = new Connection(socket);
		connection.Recv(delegate(GetClock getClock, PacketHeader header)
		{
			Clock msg = default(Clock);
			msg.ClientTime = getClock.Time;
			msg.ServerTime = Times.UnixTimeNow();
			connection.Send(msg, header.Seq);
		});
		connection.Recv(delegate(Auth auth, PacketHeader header)
		{
			string entityId = auth.EntityId;
			PlayerContext playerContext2 = GetPlayerContext(entityId);
			_connectionDict[connection] = entityId;
			SendWelcome(connection, entityId, playerContext2.PlayerInfo.PlayerName, header.Seq);
		});
		connection.Recv(delegate(Ready ready, PacketHeader readyHeader)
		{
			string text = _connectionDict.Get(connection);
			if (string.IsNullOrEmpty(text))
			{
				connection.Close();
			}
			else
			{
				connection.Send(default(OK), readyHeader.Seq);
				PlayerContext playerContext = GetPlayerContext(text);
				bool flag = playerContext.EntityId == text;
				Player player = new Player(text, connection, World, playerContext, flag);
				if (flag)
				{
					player.ContextChanged += delegate
					{
						_playerCtx.Save();
					};
				}
				World.AddPlayer(player);
			}
			_connections.Remove(connection);
			_connectionDict.Remove(connection);
		});
		connection.ConnetionClosed += delegate
		{
			_connections.Remove(connection);
			_connectionDict.Remove(connection);
		};
		connection.StartReceive();
		_connections.Add(connection);
	}

	[NotNull]
	private PlayerContext GetPlayerContext(string entityId)
	{
		PlayerContext playerContext = _playerContexts.Get(entityId);
		if (playerContext == null)
		{
			playerContext = _playerCtx;
		}
		return playerContext;
	}

	private void SendWelcome(Connection connection, string entityId, string name, uint seq)
	{
		Welcome msg = default(Welcome);
		if (World._context.IsSailingUnstable)
		{
			if (World._context.TerrainId.IndexOf("sa") != -1)
			{
				msg.Region.Name = "불안정 사바나 지대";
			}
			else if (World._context.TerrainId.IndexOf("vol") != -1)
			{
				msg.Region.Name = "불안정 화산 지대";
			}
			msg.Region.Role = Role.Risky;
		}
		else
		{
			msg.Region.Name = ((GameManager.ClusterMode == Mode.Editable) ? "창작섬" : ((GameManager.ClusterMode == Mode.SingleMode) ? "개인섬" : ((GameManager.ClusterMode == Mode.Offline) ? "기록섬" : ((GameManager.ClusterMode == Mode.MultiMode) ? "멀티 플레이 모드" : "온라인 서버"))));
			msg.Region.Role = ((GameManager.ClusterMode != Mode.Editable) ? ((GameManager.ClusterMode == Mode.SingleMode) ? Role.Personal : ((GameManager.ClusterMode == Mode.Offline) ? Role.Invalid : ((GameManager.ClusterMode == Mode.MultiMode) ? Role.Rural : Role.Urban))) : Role.Sandbox);
		}
		msg.UserId = entityId;
		msg.Name = name;
		PlayerContext playerContext = GetPlayerContext(entityId);
		msg.Storage.Data = playerContext.Storage;
		msg.Region.CreatedAt = 0.0;
		msg.Region.Id = "1";
		msg.Region.TemplateId = World.TerrainInfo.region_template;
		msg.Region.TerrainId = "1";
		BoolOption[] @bool = new BoolOption[1]
		{
			new BoolOption
			{
				Key = "market.ui_enabled",
				Value = true
			}
		};
		IntegerOption[] @int = new IntegerOption[1]
		{
			new IntegerOption
			{
				Key = "market.search.limit",
				Value = 200L
			}
		};
		msg.Options.Bool = @bool;
		msg.Options.Int = @int;
		connection.Send(msg, seq);
	}
}
