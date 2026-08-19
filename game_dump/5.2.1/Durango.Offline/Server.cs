using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using Durango.Logic.Clusters;
using Durango.Utils;
using UnityEngine;

namespace Durango.Offline;

public class Server
{
	private static Gateway _gateway;

	private static GameServer _gameServer;

	public static PlayerContext _localPlayer;

	[CompilerGenerated]
	private static Func<string, WorldContext> cache0;

	[CompilerGenerated]
	private static Func<string, PlayerContext> cache1;

	public static bool _isConnectingKylloxServer;

	public static bool _isOpenedKylloxServer;

	public static bool _autoConnected;

	public string Key { get; private set; }

	public List<Context> Contexts { get; private set; }

	public Cluster Cluster { get; private set; }

	public static double CurrentVersion => 1.2;

	public Server(string key, Dictionary<string, string> names)
	{
		Server server = this;
		Key = key;
		Contexts = new List<Context>();
		Cluster = new Cluster();
		Cluster.Mode = ((key == "free") ? Mode.Editable : ((key == "solo") ? Mode.SingleMode : ((key == "multi") ? Mode.MultiMode : ((!(key == "online")) ? Mode.Offline : Mode.Online))));
		Cluster.Names = names;
		int islandPort = GetIslandPort();
		Cluster.GatewayUrlRoot = "http://127.0.0.1:" + islandPort;
		Cluster.OnRequestAccount = delegate(Action<Account> action)
		{
			Account account = new Account
			{
				Players = new List<PlayerInfo>()
			};
			foreach (Context context3 in server.Contexts)
			{
				PlayerContext player = context3.Player;
				PlayerInfo playerInfo = player.PlayerInfo;
				playerInfo.OfflineFunc = () => new Pair<PortraitBuilder.Argument, int>(player.AppearPlayer.Display.GetPortraitArgument(player.AppearPlayer.EntityId, player.AppearPlayer.IsMale()), player.AppearPlayer.Freq);
				account.Players.Add(playerInfo);
			}
			account.MaxPlayerSlotCount = Mathf.Max(2, (server.Cluster.Mode == Mode.Editable) ? 7 : ((server.Cluster.Mode == Mode.SingleMode) ? 3 : ((server.Cluster.Mode == Mode.MultiMode) ? 3 : account.Players.Count)));
			account.PlayerSlotCount = ((server.Cluster.Mode == Mode.Editable) ? 7 : ((server.Cluster.Mode == Mode.SingleMode) ? 3 : ((server.Cluster.Mode == Mode.MultiMode) ? 3 : account.Players.Count)));
			action?.Invoke(account);
		};
		Cluster.OnDeletePlayer = delegate(string entityId)
		{
			for (int i = 0; i < server.Contexts.Count; i++)
			{
				Context context2 = server.Contexts[i];
				if (context2.EntityId == entityId)
				{
					try
					{
						File.Delete(context2.Player.Path);
						File.Delete(context2.World.Path);
						File.Delete(Path.GetDirectoryName(context2.World.Path) + "\\" + context2.PlayerSlot + ".gen");
						File.Delete(Path.GetDirectoryName(context2.World.Path) + "\\" + context2.PlayerSlot + ".ua60vol");
					}
					catch (Exception)
					{
					}
					server.Contexts.RemoveAt(i);
					break;
				}
			}
		};
		Cluster.OnConfirm = delegate(string entityId)
		{
			Context context = server.Contexts.Find((Context x) => x.EntityId == entityId);
			if (context == null)
			{
				int num = 0;
				if (server.Contexts.Count > 0)
				{
					num = server.Contexts[server.Contexts.Count - 1].PlayerSlot + 1;
				}
				WorldContext worldContext = new WorldContext();
				worldContext.Initialize(WorldContext.MakePath(num, key));
				worldContext.PlayerSlot = num;
				PlayerContext playerContext2 = new PlayerContext();
				playerContext2.Initialize(PlayerContext.MakePath(num, key));
				playerContext2.PlayerSlot = num;
				context = new Context(worldContext, playerContext2);
				server.Contexts.Add(context);
			}
			BeginServer(context.World, context.Player);
		};
		string[] files = AppData.GetFiles(WorldContext.GetBasePath(Key), "*.world", SearchOption.TopDirectoryOnly);
		if (files == null)
		{
			return;
		}
		IEnumerable<WorldContext> enumerable = from x in files.Select(WorldContext.Load)
			where x != null
			select x;
		string[] files2 = AppData.GetFiles(WorldContext.GetBasePath(Key), "*.player", SearchOption.TopDirectoryOnly);
		Dictionary<int, PlayerContext> dictionary = new Dictionary<int, PlayerContext>();
		if (files2 != null)
		{
			foreach (PlayerContext item in from x in files2.Select(PlayerContext.Load)
				where x != null
				select x)
			{
				dictionary[item.PlayerSlot] = item;
			}
		}
		Contexts = new List<Context>();
		foreach (WorldContext item2 in enumerable)
		{
			PlayerContext playerContext = dictionary.Get(item2.PlayerSlot);
			if (playerContext == null)
			{
				playerContext = new PlayerContext();
				playerContext.Initialize(PlayerContext.MakePath(item2.PlayerSlot, Key));
			}
			Contexts.Add(new Context(item2, playerContext));
		}
		Contexts = Contexts.OrderBy((Context x) => x.PlayerSlot).ToList();
	}

	public static void BeginServer(WorldContext worldCtx, PlayerContext playerCtx)
	{
		EndServer();
		_gameServer = new GameServer(worldCtx, playerCtx);
		_gateway = new Gateway(_gameServer, worldCtx, playerCtx);
		_localPlayer = playerCtx;
		if (!_autoConnected)
		{
			string environmentVariable = global::System.Environment.GetEnvironmentVariable("DURANGO_AUTOCONNECT");
			if (!string.IsNullOrEmpty(environmentVariable))
			{
				_autoConnected = true;
				ConnectTo(environmentVariable);
			}
		}
	}

	public static void EndServer()
	{
		if (_gateway != null)
		{
			_gateway.Close();
			_gateway = null;
		}
		if (_gameServer != null)
		{
			_gameServer.Close();
			_gameServer = null;
		}
	}

	public static void Process()
	{
		if (_gateway != null)
		{
			_gateway.Process();
		}
		if (_gameServer != null)
		{
			_gameServer.Process();
		}
	}

	public static void ConnectTo(string ip)
	{
		Cluster cluster = new Cluster();
		if (ip.StartsWith("http://"))
		{
			ip = ip.Substring(7);
		}
		string gatewayUrlRoot = ((ip.IndexOf(':') >= 0) ? ("http://" + ip) : ("http://" + ip + ":" + 8190));
		cluster.OnRequestAccount = delegate(Action<Account> action)
		{
			Account account = new Account();
			account.MaxPlayerSlotCount = 7;
			account.PlayerSlotCount = 1;
			account.Players = new List<PlayerInfo>();
			account.Players.Add(_localPlayer.PlayerInfo);
			action?.Invoke(account);
		};
		if (_isConnectingKylloxServer)
		{
			_isOpenedKylloxServer = true;
		}
		cluster.LocalPlayer = Json.Write(_localPlayer);
		cluster.GatewayUrlRoot = gatewayUrlRoot;
		cluster.Mode = Mode.Offline;
		GameManager.ConnectCluster = cluster;
		GameManager.Emigrated = GameManager.EmigratedType.Explore;
		Singleton<GameManager>.Instance().MoveToTitle();
	}

	public static void SendLogs(string log)
	{
		if (_localPlayer.IsConnectedKylloxServer)
		{
			WebClient webClient = new WebClient();
			if (new StreamReader(webClient.OpenRead("http://durangomanager.000webhostapp.com/server/log.txt")).ReadToEnd().IndexOf(log) == -1)
			{
				NameValueCollection nameValueCollection = new NameValueCollection();
				nameValueCollection["data"] = log;
				webClient.UploadValues("http://durangomanager.000webhostapp.com/server/upload_log.php", nameValueCollection);
			}
		}
	}

	public static int GetIslandPort()
	{
		string environmentVariable = global::System.Environment.GetEnvironmentVariable("DURANGO_ISLAND_PORT");
		if (!string.IsNullOrEmpty(environmentVariable) && int.TryParse(environmentVariable, out var result) && result > 0)
		{
			return result;
		}
		return 8390;
	}
}
