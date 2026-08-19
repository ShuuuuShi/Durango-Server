using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Durango.Terrain;
using Durango.Utils;
using JetBrains.Annotations;
using Messages;
using UnityEngine;

namespace Durango.Offline;

public class World
{
	public enum ChunkVisit
	{
		None,
		Visit,
		Sent
	}

	private class ChunkData
	{
		public byte[] Landmarks;

		public byte[] Garden;

		public byte[] herds;
	}

	private const int BiomesPerChunk = 324;

	public readonly ArtifactManager ArtifactManager;

	public readonly MarketManager MarketManager;

	private readonly TerrainData _terrainData;

	private readonly ChunkData[,] _chunkData;

	public readonly WorldContext _context;

	private readonly List<NaturalInfo> _addedNatural;

	private readonly List<Point2> _removedNatural;

	private readonly List<Player> _players;

	private readonly PlayerContext PlayerContext;

	public int NumChunksX { get; private set; }

	public int NumChunksY { get; private set; }

	public int NumTilesX => _terrainData.Width;

	public int NumTilesY => _terrainData.Height;

	public Point2 EntryPoint
	{
		get
		{
			if (KUtility.GetSize(_terrainData.Info.entry_points) >= 1 && KUtility.GetSize(_terrainData.Info.entry_points[0]) >= 2)
			{
				return new Point2(_terrainData.Info.entry_points[0][0], _terrainData.Info.entry_points[0][1]);
			}
			return new Point2(NumTilesX / 2, NumTilesY / 2);
		}
	}

	public TerrainInfoJson TerrainInfo => _terrainData.Info;

	public byte[] Biomes => _terrainData.Biomes;

	public string Weather { get; private set; }

	public event Action<AppearArtifact> ArtifactAppeared;

	public event Action<AppearArtifact> ArtifactDisappeared;

	public event Action<Player> PlayerAppeared;

	public event Action<Player> PlayerDisappeared;

	public event Action<Point2, byte[]> NaturalAdded;

	public event Action<Point2> NaturalDestroyed;

	public World(WorldContext context)
	{
		_players = new List<Player>();
		_context = context;
		ArtifactManager = new ArtifactManager(_context.Artifacts, _context.ArtifactAddOns, _context.ArtifactMannequins, _context.BoxInventories, _context.SailingRoutes);
		ArtifactManager.ArtifactStateUpdated += ArtifactManager_ArtifactStateUpdated;
		ArtifactManager.ArtifactDisplayUpdated += ArtifactManager_ArtifactDisplayUpdated;
		_addedNatural = _context.AddedNatural;
		_removedNatural = _context.RemovedNatural;
		_terrainData = TerrainLoader.Load(context.TerrainId);
		MarketManager = new MarketManager();
		_terrainData.Info.global_landmarks = null;
		NumChunksX = _terrainData.Width / 16;
		NumChunksY = _terrainData.Height / 16;
		_chunkData = new ChunkData[NumChunksX, NumChunksY];
		AssignChunkData();
	}

	public void Process()
	{
		for (int num = _players.Count - 1; num >= 0; num--)
		{
			_players[num].Process();
		}
	}

	public void Stop()
	{
		for (int num = _players.Count - 1; num >= 0; num--)
		{
			_players[num].Stop();
		}
		_players.Clear();
	}

	public void AddPlayer([NotNull] Player player)
	{
		player.Closed += delegate
		{
			_players.Remove(player);
			if (this.PlayerDisappeared != null)
			{
				this.PlayerDisappeared(player);
			}
		};
		foreach (Player player2 in _players)
		{
			player.SendAppear(player2);
		}
		if (!string.IsNullOrEmpty(Weather))
		{
			player.Send(new Weather
			{
				_Weather = Weather
			});
		}
		_players.Add(player);
		if (this.PlayerAppeared != null)
		{
			this.PlayerAppeared(player);
		}
	}

	public void BroadCast<T>(T msg)
	{
		foreach (Player player in _players)
		{
			player.Send(msg);
		}
	}

	public void Save()
	{
		_context.Save();
	}

	private void ArtifactManager_ArtifactDisplayUpdated(ArtifactDisplay obj)
	{
		Save();
	}

	private void ArtifactManager_ArtifactStateUpdated(ArtifactState state)
	{
		Save();
	}

	private void AssignChunkData()
	{
		for (int i = 0; i < NumChunksX; i++)
		{
			for (int j = 0; j < NumChunksY; j++)
			{
				ChunkData chunkData = new ChunkData();
				_chunkData[i, j] = chunkData;
			}
		}
		if (_terrainData.Landmarks != null)
		{
			List<byte[]>[,] array = CreateByteMap(_terrainData.Landmarks, 16);
			if (array != null)
			{
				AggregateByteMap(array, 16, delegate(ChunkData chunk, byte[] bytes)
				{
					chunk.Landmarks = bytes;
				});
			}
		}
		byte[] garden = _terrainData.Garden;
		if (_context.Garden != null)
		{
			garden = _context.Garden;
		}
		if (garden == null)
		{
			return;
		}
		List<NaturalInfo> list = (from g in NaturalInfo.FromBytes(garden)
			where _removedNatural.All((Point2 t) => t.x != g.X || t.y != g.Y)
			select g).ToList();
		foreach (NaturalInfo natural in _addedNatural)
		{
			NaturalInfo naturalInfo = list.Find((NaturalInfo t) => t.X == natural.X && t.Y == natural.Y);
			if (naturalInfo != null)
			{
				naturalInfo.EntityType = natural.EntityType;
			}
			else
			{
				list.Add(natural);
			}
		}
		List<byte[]>[,] array2 = CreateByteMap(NaturalInfo.ToBytes(list), 6);
		if (array2 != null)
		{
			AggregateByteMap(array2, 6, delegate(ChunkData chunk, byte[] bytes)
			{
				chunk.Garden = bytes;
			});
		}
	}

	private List<byte[]>[,] CreateByteMap(byte[] bytes, int stride)
	{
		if (bytes.Length % stride != 0)
		{
			Debug.LogError("Invalid byte size: " + bytes.Length);
			return null;
		}
		int num = bytes.Length / stride;
		List<byte[]>[,] array = new List<byte[]>[NumChunksX, NumChunksY];
		for (int i = 0; i < num; i++)
		{
			int num2 = i * stride;
			ushort x = BitConverter.ToUInt16(bytes, num2);
			int y = BitConverter.ToUInt16(bytes, num2 + 2);
			Point2 point = Util.TilePositionToChunkCoords(new Point2(x, y));
			List<byte[]> list = array[point.x, point.y];
			if (list == null)
			{
				list = new List<byte[]>();
				array[point.x, point.y] = list;
			}
			byte[] array2 = new byte[stride];
			Array.Copy(bytes, num2, array2, 0, stride);
			list.Add(array2);
		}
		return array;
	}

	private void AggregateByteMap(List<byte[]>[,] byteMap, int stride, Action<ChunkData, byte[]> onFill)
	{
		for (int i = 0; i < NumChunksX; i++)
		{
			for (int j = 0; j < NumChunksY; j++)
			{
				ChunkData arg = _chunkData[i, j];
				List<byte[]> list = byteMap[i, j];
				if (list != null)
				{
					byte[] array = new byte[list.Count * stride];
					for (int k = 0; k < list.Count; k++)
					{
						Array.Copy(list[k], 0, array, k * stride, stride);
					}
					onFill(arg, array);
				}
			}
		}
	}

	public List<Chunk> CreateChunkMessages(int centerX, int centerY, ChunkVisit[,] chunkVisited)
	{
		List<Chunk> list = new List<Chunk>();
		for (int i = centerX - 1; i <= centerX + 1; i++)
		{
			for (int j = centerY - 1; j <= centerY + 1; j++)
			{
				if (i >= 0 && i < NumChunksX && j >= 0 && j < NumChunksY && chunkVisited[i, j] == ChunkVisit.Visit)
				{
					Chunk item = CreateChunk(i, j);
					list.Add(item);
					chunkVisited[i, j] = ChunkVisit.Sent;
				}
			}
		}
		return list;
	}

	public void ConstructArtifact(AppearArtifact artifact, AddOns? addon)
	{
		ArtifactManager.AddArtifact(artifact);
		if (addon.HasValue)
		{
			AppearArtifact? appearArtifact = ArtifactManager.PlaceAddOns(artifact.EntityId, addon.Value._AddOns);
			if (appearArtifact.HasValue)
			{
				artifact = appearArtifact.Value;
			}
		}
		OnArtifactAppeared(artifact);
		Save();
	}

	private void OnArtifactAppeared(AppearArtifact aa)
	{
		if (this.ArtifactAppeared != null)
		{
			this.ArtifactAppeared(aa);
		}
	}

	public void DestructArtifact(string entityId)
	{
		AppearArtifact? appearArtifact = ArtifactManager.RemoveArtifact(entityId);
		if (appearArtifact.HasValue)
		{
			OnArtifactDisappeared(appearArtifact.Value);
			Save();
		}
	}

	private void OnArtifactDisappeared(AppearArtifact aa)
	{
		if (this.ArtifactDisappeared != null)
		{
			this.ArtifactDisappeared(aa);
		}
	}

	public void ExtendFloor(string entityId, bool withRoof)
	{
		AppearArtifact? appearArtifact = ArtifactManager.ExtendFloor(entityId, withRoof);
		if (appearArtifact.HasValue)
		{
			OnArtifactAppeared(appearArtifact.Value);
			Save();
		}
	}

	public void AddNatural(Point2 tile, ushort entityType)
	{
		if (AddNaturalToGarden(tile, entityType))
		{
			_removedNatural.Remove(tile);
			NaturalInfo naturalInfo = _addedNatural.Find((NaturalInfo t) => t.X == tile.x && t.Y == tile.y);
			if (naturalInfo != null)
			{
				naturalInfo.EntityType = entityType;
			}
			else
			{
				_addedNatural.Add(new NaturalInfo
				{
					X = (ushort)tile.x,
					Y = (ushort)tile.y,
					EntityType = entityType
				});
			}
			if (this.NaturalAdded != null)
			{
				Point2 arg = Util.TilePositionToChunkCoords(tile);
				byte[] garden = _chunkData[arg.x, arg.y].Garden;
				this.NaturalAdded(arg, garden);
			}
			Save();
		}
	}

	private bool AddNaturalToGarden(Point2 tile, ushort entityType)
	{
		Point2 point = Util.TilePositionToChunkCoords(tile);
		if (point.x < 0 || point.x >= NumChunksX || point.y < 0 || point.y >= NumChunksY)
		{
			return false;
		}
		if (!DataHelper.IsNaturalObject(entityType))
		{
			return false;
		}
		bool flag = false;
		byte[] garden = _chunkData[point.x, point.y].Garden;
		List<NaturalInfo> list = ((garden != null) ? NaturalInfo.FromBytes(garden).ToList() : new List<NaturalInfo>());
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].X == tile.x && list[i].Y == tile.y)
			{
				list[i].EntityType = entityType;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			list.Add(new NaturalInfo
			{
				X = (ushort)tile.x,
				Y = (ushort)tile.y,
				EntityType = entityType
			});
		}
		_chunkData[point.x, point.y].Garden = NaturalInfo.ToBytes(list);
		return true;
	}

	public void DestroyNatural(Point2 tile)
	{
		if (RemoveNaturalFromGarden(tile))
		{
			if (!_removedNatural.Contains(tile))
			{
				_removedNatural.Add(tile);
			}
			int num = _addedNatural.FindIndex((NaturalInfo t) => t.X == tile.x && t.Y == tile.y);
			if (num != -1)
			{
				_addedNatural.RemoveAt(num);
			}
			if (this.NaturalDestroyed != null)
			{
				this.NaturalDestroyed(tile);
			}
			Save();
		}
	}

	private bool RemoveNaturalFromGarden(Point2 tile)
	{
		Point2 point = Util.TilePositionToChunkCoords(tile);
		if (point.x < 0 || point.x >= NumChunksX || point.y < 0 || point.y >= NumChunksY)
		{
			return false;
		}
		if (_chunkData[point.x, point.y].Garden == null)
		{
			return false;
		}
		List<NaturalInfo> list = NaturalInfo.FromBytes(_chunkData[point.x, point.y].Garden).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].X == tile.x && list[i].Y == tile.y)
			{
				list.RemoveAt(i);
				break;
			}
		}
		_chunkData[point.x, point.y].Garden = NaturalInfo.ToBytes(list);
		return true;
	}

	public DefoggedChunks CreateDefoggedChunks()
	{
		DefoggedChunks result = default(DefoggedChunks);
		int num = NumChunksX * NumChunksY;
		result.Chunks = new Point2[num];
		for (int i = 0; i < NumChunksX; i++)
		{
			for (int j = 0; j < NumChunksY; j++)
			{
				int num2 = j * NumChunksX + i;
				Point2 point = new Point2(i, j);
				result.Chunks[num2] = point;
			}
		}
		return result;
	}

	public byte[] GetChunkBiomes(Point2 pos)
	{
		byte[] array = new byte[324];
		CopyChunk(pos.x, pos.y, _terrainData.Biomes, array, 1, 1, 1);
		return array;
	}

	public byte[] GetChunkOcean(Point2 pos)
	{
		byte[] array = new byte[289];
		CopyChunk(pos.x, pos.y, _terrainData.Ocean, array, 1, 0, 1);
		return array;
	}

	public byte[] GetChunkRiver(Point2 pos)
	{
		byte[] array = new byte[867];
		CopyChunk(pos.x, pos.y, _terrainData.Rivers, array, 3, 0, 1);
		return array;
	}

	public byte[] GetChunkLandmark(Point2 pos)
	{
		return _chunkData[pos.x, pos.y].Landmarks;
	}

	private Chunk CreateChunk(int chunkX, int chunkY)
	{
		Chunk result = default(Chunk);
		result._Chunk.x = chunkX;
		result._Chunk.y = chunkY;
		result.Garden = _chunkData[chunkX, chunkY].Garden;
		if (result.Garden == null)
		{
			result.Garden = new byte[0];
		}
		return result;
	}

	public void ChangeWeather(string weather)
	{
		if (!(Weather == weather))
		{
			Weather = weather;
			BroadCast(new Weather
			{
				_Weather = weather
			});
		}
	}

	public List<Pet> GetGrazedPets()
	{
		return _context.GrazedPetList;
	}

	private static void CopyChunk(int chunkX, int chunkY, byte[] src, byte[] dst, int count, int prevOffset, int postOffset)
	{
		int num = chunkX * 16;
		int num2 = chunkY * 16;
		int num3 = (int)Mathf.Sqrt((float)src.Length / (float)count);
		for (int i = -prevOffset; i < 16 + postOffset; i++)
		{
			for (int j = -prevOffset; j < 16 + postOffset; j++)
			{
				int num4 = Mathf.Clamp(num + i, 0, num3 - 1);
				int num5 = Mathf.Clamp(num2 + j, 0, num3 - 1);
				int num6 = num4 + num5 * num3;
				num6 *= count;
				int num7 = i + prevOffset;
				int num8 = j + prevOffset;
				int num9 = 16 + postOffset + prevOffset;
				int num10 = num7 + num8 * num9;
				num10 *= count;
				for (int k = 0; k < count; k++)
				{
					dst[num10 + k] = src[num6 + k];
				}
			}
		}
	}

	public List<Pet> GetPets()
	{
		return Player.Instance._context.PetList;
	}

	public void ActionTimer()
	{
		DateTime now = DateTime.Now;
		foreach (string key in _context.ActionTimer.Keys)
		{
			_context.ActionTimer.TryGetValue(key, out var value);
			if (DateTime.Compare(value, now) > 0 && _context.CollectedFrom.Keys.Contains(key))
			{
				_context.CollectedFrom.Remove(key);
				_context.ActionTimer.Remove(key);
			}
		}
	}

	public void SpawnWildAnimal(ushort type)
	{
		WorldPosition position = default(WorldPosition);
		position.SetFromClientPosition(PlayerBehavior.LocalPlayer.CurrentPosition);
		string entityId = Guid.NewGuid().ToString();
		int num = UnityEngine.Random.Range(10, 90);
		float num2 = UnityEngine.Random.Range(100f, 2000f) * (float)num;
		AppearAnimal appearAnimal = default(AppearAnimal);
		appearAnimal.EntityId = entityId;
		appearAnimal.EntityType = type;
		appearAnimal.IsAlive = true;
		appearAnimal.Level = num;
		appearAnimal.Move = new Move
		{
			EntityId = entityId,
			Movements = new Movement[1]
			{
				new Movement
				{
					Path = new Location[1]
					{
						new Location
						{
							Position = position
						}
					}
				}
			}
		};
		appearAnimal.Survival = new Survival
		{
			EntityId = entityId,
			Life = new Gauge(num2, 0f, new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = num2
				}
			})
		};
		appearAnimal.Display = new AnimalDisplay
		{
			EntityId = entityId,
			BaseScale = 1f
		};
		AppearAnimal appearAnimal2 = appearAnimal;
		Singleton<AnimalManager>.Instance().MakeAnimalObject(appearAnimal2, PlayerBehavior.LocalPlayer.CurrentPosition);
		_context.WildAnimalList.Add(appearAnimal2);
		BroadCast(appearAnimal2);
		Save();
	}

	static World()
	{
	}

	public List<AppearAnimal> GetWildAnimals()
	{
		return _context.WildAnimalList;
	}

	public void RemoveAppearAnimal(string id)
	{
		List<AppearAnimal> wildAnimals = GetWildAnimals();
		int index = wildAnimals.FindIndex((AppearAnimal x) => x.EntityId == id);
		wildAnimals.RemoveAt(index);
		Save();
	}

	public void AddArchitect(string artifactId, string entityId)
	{
		ArtifactManager.AddArchitect(artifactId, entityId);
		Save();
	}

	public List<string> GetMultiUserInfo(string info)
	{
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		try
		{
			UIManager.ShowLoadingIcon(show: true);
			using (StreamReader streamReader = new StreamReader(new WebClient().OpenRead("http://kyllox.pe.kr/user_infos/player/infos.txt"), Encoding.Default))
			{
				string text;
				while ((text = streamReader.ReadLine()) != null)
				{
					string[] array = text.Split(new string[1] { "::" }, StringSplitOptions.None);
					list.Add(array[0]);
					list2.Add(array[1]);
				}
			}
			UIManager.ShowLoadingIcon(show: false);
		}
		catch (Exception ex)
		{
			UIManager.SystemMsg(ex.ToString());
		}
		if (info == "name")
		{
			return list;
		}
		if (info == "id")
		{
			return list2;
		}
		return null;
	}
}
