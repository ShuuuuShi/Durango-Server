using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Terrain;

public static class Util
{
	public const int SingleTileSize = 200;

	public const int NumTilesInChunkX = 16;

	public const int NumTilesInChunkY = 16;

	public const int ChunkWidth = 3200;

	public const int ChunkHeight = 3200;

	public const byte CollidableMask = 128;

	public const byte NotPlantableMask = 64;

	public static readonly Vector3 TileCenterOffset = new Vector3(100f, 0f, 100f);

	public static bool IsWater(Biome biome)
	{
		switch (biome)
		{
		case Biome.ColdOcean:
		case Biome.WarmOcean:
		case Biome.River:
		case Biome.Lake:
			return true;
		default:
			return false;
		}
	}

	public static bool IsDrinkable(Biome biome)
	{
		if (biome == Biome.River || biome == Biome.Lake)
		{
			return true;
		}
		return false;
	}

	public static bool IsCollidableMasked(byte maskedBiome)
	{
		return (maskedBiome & 0x80) != 0;
	}

	public static bool IsNotPlantableMasked(byte maskedBiome)
	{
		return (maskedBiome & 0x40) != 0;
	}

	public static Biome GetUnmaskedBiome(byte maskedBiome)
	{
		Biome biome = (Biome)((int)maskedBiome & -193);
		if (biome < Biome.Invalid || biome > Biome.Lava)
		{
			return Biome.Invalid;
		}
		return biome;
	}

	public static byte MaskBiome(Biome biome, bool isCollidable, bool isExcludePlant)
	{
		byte b = (byte)biome;
		if (isCollidable)
		{
			b = (byte)(b | 0x80u);
		}
		if (isExcludePlant)
		{
			b = (byte)(b | 0x40u);
		}
		return b;
	}

	public static Vector2 WorldPositionToTilePosition(Vector3 position)
	{
		return new Vector2(position.x / 200f, position.z / 200f);
	}

	public static Vector2 ClientPositionToTilePosition(Vector3 position)
	{
		return WorldPositionToTilePosition(ClientPositionToWorldPosition(position));
	}

	public static Vector3 TilePositionToWorldPosition(Point2 tilePosition, bool tileCenter = false)
	{
		Vector3 vector = ((!tileCenter) ? Vector3.zero : TileCenterOffset);
		return new Vector3(tilePosition.x * 200, 0f, tilePosition.y * 200) + vector;
	}

	public static Vector3 TilePositionToWorldPosition(Vector2 tilePosition, bool tileCenter = false)
	{
		Vector3 vector = ((!tileCenter) ? Vector3.zero : TileCenterOffset);
		return new Vector3(tilePosition.x * 200f, 0f, tilePosition.y * 200f) + vector;
	}

	public static Vector3 TilePositionToClientPosition(Vector2 tilePosition, bool tileCenter = false)
	{
		return WorldPositionToClientPosition(TilePositionToWorldPosition(tilePosition, tileCenter));
	}

	public static Vector3 TilePositionToClientPosition(Point2 tilePosition, bool tileCenter = false)
	{
		return WorldPositionToClientPosition(TilePositionToWorldPosition(tilePosition, tileCenter));
	}

	public static Point2 WorldPositionToChunkCoords(Vector3 worldPosition)
	{
		return new Point2((int)(worldPosition.x / 3200f), (int)(worldPosition.z / 3200f));
	}

	public static Vector3 ChunkCoordsToWorldPosition(Point2 chunkCoords)
	{
		return new Vector3(chunkCoords.x * 3200, 0f, chunkCoords.y * 3200);
	}

	public static Point2 TilePositionToChunkCoords(Point2 worldTile)
	{
		Vector3 worldPosition = new Vector3(worldTile.x * 200, 0f, worldTile.y * 200);
		return WorldPositionToChunkCoords(worldPosition);
	}

	public static Point2 ClientPositionToChunkCoords(Vector3 clientPosition)
	{
		Vector3 worldPosition = ClientPositionToWorldPosition(clientPosition);
		return WorldPositionToChunkCoords(worldPosition);
	}

	public static Vector3 ChunkCoordsToClientPosition(Vector2 chunkCoords, float height)
	{
		return WorldPositionToClientPosition(new Vector3(chunkCoords.x * 3200f, height, chunkCoords.y * 3200f));
	}

	public static Vector3 WorldPositionToClientPosition(Vector2 worldPosition)
	{
		Vector3 vector = new Vector3(worldPosition.x, 0f, worldPosition.y);
		if (Singleton<TerrainBase>.HasInstance() && TerrainBase.IsPlayerInitialized)
		{
			return vector - Singleton<TerrainBase>.Instance().CorrectionPosition;
		}
		return vector;
	}

	public static Vector3 WorldPositionToClientPosition(Vector3 worldPosition)
	{
		if (Singleton<TerrainBase>.HasInstance() && TerrainBase.IsPlayerInitialized)
		{
			return worldPosition - Singleton<TerrainBase>.Instance().CorrectionPosition;
		}
		return worldPosition;
	}

	public static Vector3 ClientPositionToWorldPosition(Vector3 clientPosition)
	{
		if (Singleton<TerrainBase>.HasInstance() && TerrainBase.IsPlayerInitialized)
		{
			return clientPosition + Singleton<TerrainBase>.Instance().CorrectionPosition;
		}
		return clientPosition;
	}
}
