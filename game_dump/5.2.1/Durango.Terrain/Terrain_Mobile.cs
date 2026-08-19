using System;
using Durango.Render.Sprite;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;
using UnityEngine;

namespace Durango.Terrain;

public class Terrain_Mobile : TerrainBase
{
	[Serializable]
	private class TileSet
	{
		[SerializeField]
		public string Name;

		[SerializeField]
		public Texture2D MaskTexture;

		[SerializeField]
		public Texture2D DamagedPropTexture;

		[SerializeField]
		[EnumList(typeof(Biome), true, 0, -1)]
		public Texture2D[] BiomeTextures;
	}

	[SerializeField]
	private TileSet _defaultTileSet;

	[SerializeField]
	private TileSet[] _overrideTileSets;

	private TileSet _currentTileSet;

	public new static Terrain_Mobile Instance()
	{
		return Singleton<TerrainBase>.Instance() as Terrain_Mobile;
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

	public Texture2D GetDamagedPropTexture()
	{
		TileSet currentTileSet = _currentTileSet;
		TileSet defaultTileSet = _defaultTileSet;
		if (currentTileSet.DamagedPropTexture != null)
		{
			return currentTileSet.DamagedPropTexture;
		}
		return defaultTileSet.DamagedPropTexture;
	}

	public Texture2D GetTileTextureFromBiome(Biome biome)
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
		NaturalSpriteObject naturalSpriteObject = moveAffectingObject as NaturalSpriteObject;
		if (naturalSpriteObject == null)
		{
			return TileMoveType.None;
		}
		if (naturalSpriteObject.Sprite == null)
		{
			return TileMoveType.None;
		}
		if (!IsBushWhackableSize(naturalSpriteObject.Sprite))
		{
			return TileMoveType.None;
		}
		return naturalSpriteObject.Sprite.SpriteColliderSize switch
		{
			tk2dSpriteDefinition.ColliderSizeType.Tiny => TileMoveType.Tiny, 
			tk2dSpriteDefinition.ColliderSizeType.Small => TileMoveType.Small, 
			tk2dSpriteDefinition.ColliderSizeType.Medium => TileMoveType.Medium, 
			tk2dSpriteDefinition.ColliderSizeType.Large => TileMoveType.Large, 
			_ => TileMoveType.None, 
		};
	}

	public override bool IsBushWhackableSize(ImmovableBase immovable)
	{
		NaturalSpriteObject naturalSpriteObject = immovable as NaturalSpriteObject;
		if (naturalSpriteObject == null)
		{
			return false;
		}
		if (naturalSpriteObject.Sprite == null)
		{
			return false;
		}
		return IsBushWhackableSize(naturalSpriteObject.Sprite);
	}

	public bool IsBushWhackableSize([NotNull] Durango.Render.Sprite.Sprite sprite)
	{
		return sprite.SpriteColliderSize >= tk2dSpriteDefinition.ColliderSizeType.Small;
	}

	public override bool IsShakable(ImmovableBase immovableObject)
	{
		NaturalSpriteObject naturalSpriteObject = immovableObject as NaturalSpriteObject;
		if (naturalSpriteObject == null)
		{
			return false;
		}
		if (naturalSpriteObject.Sprite == null)
		{
			return false;
		}
		return naturalSpriteObject.Sprite.IsShakable;
	}
}
