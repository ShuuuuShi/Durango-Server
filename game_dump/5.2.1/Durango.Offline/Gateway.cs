using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Durango.Prologue;
using Durango.Utils;
using Durango.Utils.Extensions;
using Messages;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Offline;

public class Gateway
{
	public const int DefaultPort = 8390;

	private readonly WebServer _webServer;

	private readonly GameServer _gameServer;

	private readonly WorldContext _worldCtx;

	private readonly PlayerContext _playerCtx;

	public int Port { get; private set; }

	public Gateway(GameServer gameServer, WorldContext worldCtx, PlayerContext playerCtx)
	{
		Port = Server.GetIslandPort();
		_webServer = new WebServer(Port);
		_gameServer = gameServer;
		_worldCtx = worldCtx;
		_playerCtx = playerCtx;
		_webServer.GetRoute["/knock"] = delegate(HttpListenerRequest request, Dictionary<string, string> postData)
		{
			JObject jObject = new JObject
			{
				["server_version"] = CurrentBundleVersion.GetClientVersion(),
				["compatible"] = true
			};
			string text3 = ((!Debug.isDebugBuild) ? "live" : "release");
			string text4 = request.QueryString.Get("platform");
			text4 = ((text4 != null && text4.Equals("iPhonePlayer", StringComparison.OrdinalIgnoreCase)) ? "ios" : ((text4 == null || !text4.Equals("Android", StringComparison.OrdinalIgnoreCase)) ? "windows" : "android"));
			jObject["assetbundle_index_url"] = "http://assetbundles.k.nexon.com/" + text3 + "/" + text4 + "/Info.5.2.1.json";
			jObject["assetbundle_url_root"] = "http://durango-assetbundles.akamaized.net/" + text3 + "/" + text4 + "/";
			return new WebServer.JsonResponse(jObject.ToString());
		};
		_webServer.GetRoute["/notice"] = (HttpListenerRequest request, Dictionary<string, string> _) => new WebServer.JsonResponse("{}");
		_webServer.PostRoute["/sessions"] = delegate(HttpListenerRequest request, Dictionary<string, string> postData)
		{
			PlayerContext playerContext = null;
			string text2 = postData.Get("player");
			if (!string.IsNullOrEmpty(text2))
			{
				playerContext = Json.Read<PlayerContext>(text2);
				if (!_gameServer.Register(playerContext))
				{
					playerContext = null;
				}
			}
			if (playerContext == null)
			{
				playerContext = _playerCtx;
			}
			return new WebServer.JsonResponse(new JObject
			{
				["user_id"] = playerContext.EntityId,
				["session_token"] = playerContext.EntityId
			}.ToString());
		};
		_webServer.GetRoute["/admission"] = (HttpListenerRequest request, Dictionary<string, string> _) => new WebServer.JsonResponse(new JObject { ["admitted"] = true }.ToString());
		_webServer.GetRoute["/entry"] = (HttpListenerRequest request, Dictionary<string, string> _) => new WebServer.JsonResponse(new JObject
		{
			["frontend_addresses"] = new JArray(new object[1] { "127.0.0.1:" + _gameServer.Port }),
			["cluster_mode"] = GameManager.ClusterMode.ToString()
		}.ToString());
		_webServer.PostRoute["/players"] = delegate(HttpListenerRequest request, Dictionary<string, string> postData)
		{
			_playerCtx.PlayerInfo.PlayerName = postData.Get("name");
			List<string> regionTemplateIds = Yaml.Util.Singleton<Constants>.Instance.PersonalRegion.RegionTemplateIds;
			int index = UnityEngine.Random.Range(0, regionTemplateIds.Count);
			string text = postData.Get("region");
			_worldCtx.TerrainId = ((!regionTemplateIds.Contains(text)) ? regionTemplateIds[index] : text);
			UpdateAppearPlayer(_playerCtx, postData);
			string[] bodyColor = _playerCtx.AppearPlayer.Display.BodyColor;
			string[] array = new string[8] { "clothes_engineer", "clothes_officeworker", "clothes_student", "clothes_farmer", "clothes_waiter", "clothes_soldier", "clothes_homeworker", "clothes_jobless" };
			int value = postData.Get("job").ToInt();
			value = Mathf.Clamp(value, 0, array.Length - 1);
			Item? item = Cheats.MakeItem(array[value], 1);
			if (item.HasValue)
			{
				Item value2 = item.Value;
				if (bodyColor.Length >= 3)
				{
					value2.ColorR = bodyColor[0];
					value2.ColorG = bodyColor[1];
					value2.ColorB = bodyColor[2];
				}
				_playerCtx.InventoryItems.Add(value2);
				_playerCtx.EquippedItems.Add("body", value2.Id);
			}
			_worldCtx.Save();
			_playerCtx.Save();
			return new WebServer.JsonResponse(new JObject { ["entity_id"] = _playerCtx.EntityId }.ToString());
		};
		_webServer.GetRoute["/terrains/1"] = (HttpListenerRequest _003Cp0_003E, Dictionary<string, string> _003Cp1_003E) => new WebServer.JsonResponse(Json.Write(_gameServer.World.TerrainInfo));
		_webServer.GetRoute["/terrains/1/whole_biomes"] = (HttpListenerRequest request, Dictionary<string, string> _) => new WebServer.BinaryReponse
		{
			Content = _gameServer.World.Biomes
		};
		_webServer.UnhandledUrl += UnhandledUrl;
	}

	private static void UpdateAppearPlayer(PlayerContext player, Dictionary<string, string> postData)
	{
		bool flag = postData.Get("gender") == "male";
		player.AppearPlayer.EntityType = (ushort)((!flag) ? 1001u : 1000u);
		PrologueManager.PlayerDisplay playerDisplay = Json.Read<PrologueManager.PlayerDisplay>(postData.Get("model_info"));
		PlayerDisplay display = player.AppearPlayer.Display;
		display.Hair = playerDisplay.hair;
		display.BodyColor = playerDisplay.body_color;
		display.HeadColor = playerDisplay.head_color;
		display.SkinColor = playerDisplay.skin_color;
		display.HairColor = playerDisplay.hair_color;
		display.LipColor = playerDisplay.lip_color;
		display.EyeColor = playerDisplay.eye_color;
		display.Portrait = playerDisplay.portrait;
		display.PortraitBg = playerDisplay.portrait_bg;
		display.PortraitBgColor = playerDisplay.portrait_bg_color;
		display.Beard = playerDisplay.beard;
		display.VoiceType = playerDisplay.voice_type;
		display.BodySize = playerDisplay.body_size;
		player.AppearPlayer.Display = display;
		player.AppearPlayer.Name = player.PlayerInfo.PlayerName;
		player.AppearPlayer.Level = player.PlayerInfo.PlayerLevel;
	}

	public void Process()
	{
		_webServer.Process();
	}

	public void Close()
	{
		_webServer.Close();
	}

	private WebServer.RouteFunction UnhandledUrl(string url)
	{
		if (url.StartsWith("/terrains/1/"))
		{
			return delegate
			{
				Point2 point2FromUrl3 = GetPoint2FromUrl(url);
				WebServer.BinaryReponse binaryReponse = new WebServer.BinaryReponse();
				byte[] chunkBiomes = _gameServer.World.GetChunkBiomes(point2FromUrl3);
				byte[] chunkOcean = _gameServer.World.GetChunkOcean(point2FromUrl3);
				byte[] chunkRiver = _gameServer.World.GetChunkRiver(point2FromUrl3);
				byte[] chunkLandmark = _gameServer.World.GetChunkLandmark(point2FromUrl3);
				MemoryStream memoryStream = new MemoryStream();
				memoryStream.Write(chunkBiomes, 0, chunkBiomes.Length);
				memoryStream.Write(chunkOcean, 0, chunkOcean.Length);
				memoryStream.Write(chunkRiver, 0, chunkRiver.Length);
				if (chunkLandmark != null)
				{
					memoryStream.Write(chunkLandmark, 0, chunkLandmark.Length);
				}
				binaryReponse.Content = memoryStream.ToArray();
				return binaryReponse;
			};
		}
		if (url.StartsWith("/terrains/1/ocean"))
		{
			return delegate
			{
				Point2 point2FromUrl2 = GetPoint2FromUrl(url);
				return new WebServer.BinaryReponse
				{
					Content = _gameServer.World.GetChunkOcean(point2FromUrl2)
				};
			};
		}
		if (url.StartsWith("/terrains/1/rivers"))
		{
			return delegate
			{
				Point2 point2FromUrl = GetPoint2FromUrl(url);
				return new WebServer.BinaryReponse
				{
					Content = _gameServer.World.GetChunkRiver(point2FromUrl)
				};
			};
		}
		return (HttpListenerRequest request, Dictionary<string, string> _) => new WebServer.BadRequestResponse();
	}

	private static Point2 GetPoint2FromUrl(string url)
	{
		int num = url.LastIndexOf("/", StringComparison.Ordinal) + 1;
		string[] array = url.Substring(num, url.Length - num).Split(',');
		return new Point2(array[0].ToInt(), array[1].ToInt());
	}
}
