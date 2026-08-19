using System;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Terrain;

public class Terrain_PC : TerrainBase
{
	[Serializable]
	public class TextureSet
	{
		[SerializeField]
		public Texture2D Base;

		[SerializeField]
		public Texture2D Pbr;
	}

	[Serializable]
	private class TileSet
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public Texture2D MaskTexture;

		[SerializeField]
		public TextureSet DamagedPropTexture;

		[SerializeField]
		[EnumList(typeof(Biome), true, 0, -1)]
		public TextureSet[] BiomeTextures;
	}

	[SerializeField]
	private TileSet _defaultTileSet;

	[SerializeField]
	private TileSet[] _overrideTileSets;

	private TileSet _currentTileSet;

	public new static Terrain_PC Instance()
	{
		return Singleton<TerrainBase>.Instance() as Terrain_PC;
	}

	protected override void ApplyTileSet()
	{
		_currentTileSet = _defaultTileSet;
		for (int i = 0; i < _overrideTileSets.Length; i++)
		{
			if (_overrideTileSets[i].Name == TerrainMeta.TileSet)
			{
				_currentTileSet = _overrideTileSets[i];
				break;
			}
		}
	}

	public Texture2D GetMaskTexture()
	{
		TileSet currentTileSet = _currentTileSet;
		TileSet defaultTileSet = _defaultTileSet;
		if (currentTileSet.MaskTexture != null)
		{
			return currentTileSet.MaskTexture;
		}
		return defaultTileSet.MaskTexture;
	}

	public TextureSet GetDamagedPropTextureSet()
	{
		TileSet currentTileSet = _currentTileSet;
		TileSet defaultTileSet = _defaultTileSet;
		if (currentTileSet.DamagedPropTexture != null)
		{
			return currentTileSet.DamagedPropTexture;
		}
		return defaultTileSet.DamagedPropTexture;
	}

	public TextureSet GetTileTextureSetFromBiome(Biome biome)
	{
		TileSet currentTileSet = _currentTileSet;
		TileSet defaultTileSet = _defaultTileSet;
		if (currentTileSet.BiomeTextures[(int)biome] != null)
		{
			return currentTileSet.BiomeTextures[(int)biome];
		}
		return defaultTileSet.BiomeTextures[(int)biome];
	}

	public override TileMoveType GetTileMoveType(Vector3 worldPosition)
	{
		ImmovableBase moveAffectingObject = GetMoveAffectingObject(worldPosition);
		if (moveAffectingObject == null)
		{
			return TileMoveType.None;
		}
		if (IsRoad(moveAffectingObject))
		{
			return TileMoveType.Road;
		}
		return TileMoveType.None;
	}

	public override bool IsBushWhackableSize(ImmovableBase immovable)
	{
		return false;
	}

	public override bool IsShakable(ImmovableBase immovableObject)
	{
		return false;
	}
}
