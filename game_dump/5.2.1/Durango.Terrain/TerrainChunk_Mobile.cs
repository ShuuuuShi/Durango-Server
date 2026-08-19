using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Render.Particle;
using Durango.Render.Sprite;
using Durango.Render.Water;
using Durango.Utils;
using JetBrains.Annotations;
using Shared.Region;
using UnityEngine;

namespace Durango.Terrain;

public class TerrainChunk_Mobile : TerrainChunkBase
{
	[CompilerGenerated]
	private sealed class _003CLoadGrass_003Ed__12 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public TerrainChunk_Mobile _003C_003E4__this;

		public byte[] tileBiomeData;

		private bool _003ChasGrass_003E5__2;

		private int _003Cy_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CLoadGrass_003Ed__12(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TerrainChunk_Mobile terrainChunk_Mobile = _003C_003E4__this;
			switch (num)
			{
			default:
				return false;
			case 0:
				_003C_003E1__state = -1;
				terrainChunk_Mobile._grassSpritePool.ResetSprites();
				_003ChasGrass_003E5__2 = false;
				_003Cy_003E5__3 = 0;
				goto IL_00a8;
			case 1:
				_003C_003E1__state = -1;
				_003Cy_003E5__3++;
				goto IL_00a8;
			case 2:
				{
					_003C_003E1__state = -1;
					return false;
				}
				IL_00a8:
				if (_003Cy_003E5__3 < 16)
				{
					for (int i = 0; i < 16; i++)
					{
						byte rawBiome = tileBiomeData[i + _003Cy_003E5__3 * 18];
						_003ChasGrass_003E5__2 |= terrainChunk_Mobile.AddGrass(rawBiome, i, _003Cy_003E5__3);
					}
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				terrainChunk_Mobile._grassSpritePool.gameObject.SetActive(_003ChasGrass_003E5__2);
				_003C_003E2__current = null;
				_003C_003E1__state = 2;
				return true;
			}
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	[CompilerGenerated]
	private sealed class _003CLoadNaturals_003Ed__15 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public IList<NaturalInfo> naturalData;

		public TerrainChunk_Mobile _003C_003E4__this;

		private bool _003ChasSpriteObject_003E5__2;

		private int _003Ci_003E5__3;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CLoadNaturals_003Ed__15(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			TerrainChunk_Mobile terrainChunk_Mobile = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				goto IL_008f;
			}
			_003C_003E1__state = -1;
			if (naturalData == null)
			{
				return false;
			}
			terrainChunk_Mobile._worldSpritePool.ResetSprites();
			_003ChasSpriteObject_003E5__2 = false;
			_003Ci_003E5__3 = 0;
			goto IL_00f6;
			IL_008f:
			Point2 point = terrainChunk_Mobile.FromWorldTile(new Point2(naturalData[_003Ci_003E5__3].X, naturalData[_003Ci_003E5__3].Y));
			TileObject tileObject = terrainChunk_Mobile.StaticObjectChunk.GetTileObject(point);
			if (tileObject != null)
			{
				terrainChunk_Mobile.FillShrubCollisionToGrid(point, tileObject.NaturalObject);
			}
			_003Ci_003E5__3++;
			goto IL_00f6;
			IL_00f6:
			if (_003Ci_003E5__3 < naturalData.Count)
			{
				_003ChasSpriteObject_003E5__2 |= terrainChunk_Mobile.AddNaturalEntity(naturalData[_003Ci_003E5__3]);
				if ((_003Ci_003E5__3 + 1) % 10 == 0)
				{
					_003C_003E2__current = null;
					_003C_003E1__state = 1;
					return true;
				}
				goto IL_008f;
			}
			terrainChunk_Mobile._worldSpritePool.gameObject.SetActive(_003ChasSpriteObject_003E5__2);
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	private SpritePool _grassSpritePool;

	private SpritePool _worldSpritePool;

	private static readonly Point2[] GridPatterns = new Point2[5]
	{
		new Point2(0, 0),
		new Point2(0, 1),
		new Point2(1, 0),
		new Point2(-1, 1),
		new Point2(1, -1)
	};

	private bool _oldWorldSpritePoolVisibility;

	public override void Init(WaterChunk oceanChunk, WaterChunk lakeChunk, RiverChunk riverChunk)
	{
		base.Init(oceanChunk, lakeChunk, riverChunk);
		Singleton<SpriteManager>.Instance().SpriteCollectionLoaded += SpriteManager_SpriteCollectionLoaded;
	}

	protected override void InitGameObjects()
	{
		base.InitGameObjects();
		_grassSpritePool = InitSpritePool("Grass", selectable: false);
		_worldSpritePool = InitSpritePool("Natural", selectable: true);
	}

	protected override void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY)
	{
		TerrainMaterial = GetComponent<Renderer>().material;
		TerrainMesh = GetComponent<MeshFilter>().mesh;
		Vector3[] array = new Vector3[numTilesInChunkX * numTilesInChunkY * 4];
		Vector2[] array2 = new Vector2[numTilesInChunkX * numTilesInChunkY * 4];
		int[] array3 = new int[numTilesInChunkX * numTilesInChunkY * 6];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < numTilesInChunkY; i++)
		{
			for (int j = 0; j < numTilesInChunkX; j++)
			{
				int num3 = num;
				Vector3 vector = new Vector3(((float)j - 8f) * 200f, 0f, ((float)i - 8f) * 200f);
				Vector2 vector2 = new Vector2((float)j % 2f * 0.5f * 0.5f, (float)i % 2f * 0.5f * 1f);
				array[num] = vector;
				array2[num] = vector2;
				num++;
				ref Vector3 reference = ref array[num];
				reference = vector + new Vector3(0f, 0f, 201.5635f);
				ref Vector2 reference2 = ref array2[num];
				reference2 = vector2 + new Vector2(0f, 0.499f);
				num++;
				ref Vector3 reference3 = ref array[num];
				reference3 = vector + new Vector3(201.5635f, 0f, 0f);
				ref Vector2 reference4 = ref array2[num];
				reference4 = vector2 + new Vector2(0.249f, 0f);
				num++;
				ref Vector3 reference5 = ref array[num];
				reference5 = vector + new Vector3(201.5635f, 0f, 201.5635f);
				ref Vector2 reference6 = ref array2[num];
				reference6 = vector2 + new Vector2(0.249f, 0.499f);
				num++;
				array3[num2++] = num3;
				array3[num2++] = num3 + 3;
				array3[num2++] = num3 + 2;
				array3[num2++] = num3 + 1;
				array3[num2++] = num3 + 3;
				array3[num2++] = num3;
			}
		}
		TerrainMesh.vertices = array;
		TerrainMesh.normals = null;
		TerrainMesh.uv = array2;
		TerrainMesh.triangles = array3;
		TerrainMesh.tangents = null;
	}

	private SpritePool InitSpritePool(string spritePoolName, bool selectable)
	{
		SpritePool spritePool = CreateLocalObject(spritePoolName).AddComponent<SpritePool>();
		spritePool.Init(selectable);
		return spritePool;
	}

	[NotNull]
	private Durango.Render.Sprite.Sprite AllocSpriteFromPool(bool isGrassOrPuddle)
	{
		SpritePool spritePool = ((!isGrassOrPuddle) ? _worldSpritePool : _grassSpritePool);
		GameObject gameObject = spritePool.gameObject;
		if (!gameObject.activeSelf)
		{
			gameObject.SetActive(value: true);
			spritePool.ResetSprites();
		}
		return spritePool.Alloc();
	}

	public void ReleaseSpriteToPool(Durango.Render.Sprite.Sprite sprite)
	{
		if (sprite.SpriteObjectType == SpriteObjectType.Grass || sprite.SpriteObjectType == SpriteObjectType.Puddle)
		{
			_grassSpritePool.Release(sprite);
		}
		else
		{
			_worldSpritePool.Release(sprite);
		}
	}

	private void SpriteManager_SpriteCollectionLoaded()
	{
		if (_grassSpritePool != null)
		{
			_grassSpritePool.CheckLoaded();
		}
		if (_worldSpritePool != null)
		{
			_worldSpritePool.CheckLoaded();
		}
	}

	public void SetSpriteTransformParams(Durango.Render.Sprite.Sprite sprite, int tileX, int tileY, int entityType)
	{
		float num = ChunkHash.Value(tileX, tileY, ChunkHash.Category.SpriteX, entityType);
		float num2 = ChunkHash.Value(tileX, tileY, ChunkHash.Category.SpriteY, entityType);
		Vector2 vector = new Vector2(100f, 100f);
		vector.x += (num - 0.5f) * 200f * 0.4f;
		vector.y += (num2 - 0.5f) * 200f * 0.4f;
		float num3 = (float)tileX * 200f + vector.x;
		float num4 = (float)tileY * 200f + vector.y;
		float num5 = num4 / 16f * 0.01f;
		float scaleRatio = ChunkHash.Value(tileX, tileY, ChunkHash.Category.NaturalScale, entityType);
		float yawRatio = ChunkHash.Value(tileX, tileY, ChunkHash.Category.NaturalYaw, entityType);
		float brightnessRatio = ChunkHash.Value(tileX, tileY, ChunkHash.Category.NaturalBrightness, entityType);
		sprite.SetTransformParams(scaleRatio, yawRatio, new Vector2(num3 + num5, num4 + num5), brightnessRatio);
	}

	protected override IEnumerator LoadGrass(byte[] tileBiomeData)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLoadGrass_003Ed__12(0)
		{
			_003C_003E4__this = this,
			tileBiomeData = tileBiomeData
		};
	}

	protected override bool AddGrass(byte rawBiome, int x, int y)
	{
		if (Util.IsNotPlantableMasked(rawBiome))
		{
			return false;
		}
		Point2 tile = new Point2(x, y);
		TileObject tileObject = base.StaticObjectChunk.GetTileObject(tile);
		if (tileObject == null || tileObject.IsGrassRemoved)
		{
			return false;
		}
		Biome unmaskedBiome = Util.GetUnmaskedBiome(rawBiome);
		float tileMinDepth = GetTileMinDepth(x, y);
		if (!GrassHelper.HasRandomGrass(unmaskedBiome, tileMinDepth, x, y, ChunkHash))
		{
			return false;
		}
		Color color;
		BiomeSpriteInfo randomGrass = GrassHelper.GetRandomGrass(unmaskedBiome, x, y, ChunkHash, out color);
		if (randomGrass == null)
		{
			return false;
		}
		string text = RandomNaturaleName(randomGrass, tile);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		Durango.Render.Sprite.Sprite sprite = AddNaturalSprite(string.Empty, text, randomGrass, tile);
		if (sprite != null && randomGrass.SpriteObjectType == SpriteObjectType.Grass)
		{
			sprite.SetColor(color);
			return true;
		}
		return false;
	}

	public override void RemoveGrass(Point2 tile)
	{
		TileObject tileObject = base.StaticObjectChunk.GetTileObject(tile);
		if (tileObject != null)
		{
			tileObject.IsGrassRemoved = true;
			Durango.Render.Sprite.Sprite grassSprite = tileObject.GrassSprite;
			if (grassSprite != null)
			{
				tileObject.SetGrassSprite(null);
				ReleaseSpriteToPool(grassSprite);
			}
		}
	}

	protected override IEnumerator LoadNaturals(IList<NaturalInfo> naturalData)
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CLoadNaturals_003Ed__15(0)
		{
			_003C_003E4__this = this,
			naturalData = naturalData
		};
	}

	protected override void FillShrubCollisionToGrid(Point2 tilePos, NaturalObject naturalObject)
	{
		NaturalSpriteObject naturalSpriteObject = naturalObject as NaturalSpriteObject;
		if (!(naturalSpriteObject == null) && naturalSpriteObject.Sprite != null && naturalSpriteObject.Sprite.SpriteObjectType == SpriteObjectType.Shrub)
		{
			int num = -1;
			switch (naturalSpriteObject.Sprite.SpriteColliderSize)
			{
			case tk2dSpriteDefinition.ColliderSizeType.Tiny:
			case tk2dSpriteDefinition.ColliderSizeType.Small:
				num = 0;
				break;
			case tk2dSpriteDefinition.ColliderSizeType.Medium:
				num = 2;
				break;
			case tk2dSpriteDefinition.ColliderSizeType.Large:
				num = 4;
				break;
			}
			for (int i = 0; i <= num; i++)
			{
				Point2 point = tilePos + GridPatterns[i];
				point.x = Mathf.Clamp(point.x, 0, 15);
				point.y = Mathf.Clamp(point.y, 0, 15);
				int num2 = point.x + point.y * 16;
				MoveAffectingCollisionGrid[num2] = naturalObject;
			}
		}
	}

	public override bool AddNaturalEntity(NaturalInfo natural, Action<GameObject> callback = null)
	{
		if (!DataHelper.IsNaturalObject(natural.EntityType))
		{
			return false;
		}
		Point2 tile = FromWorldTile(new Point2(natural.X, natural.Y));
		BiomeSpriteInfo biomeSpriteInfo = DataHelper.GetBiomeSpriteInfo(natural.EntityType);
		if (biomeSpriteInfo == null)
		{
			return false;
		}
		string text = RandomNaturaleName(biomeSpriteInfo, tile);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
		{
			AddNaturalPrefab(string.Empty, natural.EntityType, text, tile);
			return false;
		}
		Durango.Render.Sprite.Sprite sprite = AddNaturalSprite(string.Empty, text, biomeSpriteInfo, tile);
		callback?.Invoke(sprite?.GameObject);
		return sprite != null;
	}

	[CanBeNull]
	private Durango.Render.Sprite.Sprite AddNaturalSprite(string entityId, [NotNull] string spriteName, [NotNull] BiomeSpriteInfo spriteInfo, Point2 tile)
	{
		TileObject tileObject = base.StaticObjectChunk.GetTileObject(tile);
		if (tileObject == null)
		{
			Point2 point = tile;
			Debug.LogError("GetTileObject failed - " + point.ToString());
			return null;
		}
		bool flag = spriteInfo.SpriteObjectType == SpriteObjectType.Grass || spriteInfo.SpriteObjectType == SpriteObjectType.Puddle;
		Durango.Render.Sprite.Sprite sprite = AllocSpriteFromPool(flag);
		int entityType = spriteInfo.EntityType;
		SetSpriteTransformParams(sprite, tile.x, tile.y, entityType);
		if (!sprite.SetSpriteByName(spriteInfo.SpriteObjectType, spriteName, spriteInfo.Additive, spriteInfo.Particle))
		{
			ReleaseSpriteToPool(sprite);
			return null;
		}
		RemoveGrass(tile);
		GameObject spriteObject = sprite.GameObject;
		if (flag)
		{
			SetFirefly(spriteObject, tile.x, tile.y);
			tileObject.SetGrassSprite(sprite);
		}
		else
		{
			NaturalObject naturalObject = sprite.NaturalObject;
			if ((bool)naturalObject)
			{
				Point2 worldTile = ToWorldTile(tile);
				naturalObject.SetEntity(entityId, (ushort)entityType, worldTile);
				tileObject.SetNaturalObject(naturalObject);
			}
			else
			{
				Debug.LogError("Sprite doesn't have NaturalObject component - " + spriteInfo.EntityType);
			}
		}
		return sprite;
	}

	private void SetFirefly(GameObject spriteObject, int tileX, int tileY)
	{
		Firefly component = spriteObject.GetComponent<Firefly>();
		if ((double)ChunkHash.Value(tileX, tileY, ChunkHash.Category.FireFly) < 0.05)
		{
			if (component == null)
			{
				spriteObject.AddComponent<Firefly>();
			}
			else
			{
				component.enabled = true;
			}
		}
		else if ((bool)component)
		{
			component.enabled = false;
		}
	}

	protected override void AssignMaterial(int[] tileToBiome)
	{
		Terrain_Mobile terrain_Mobile = Terrain_Mobile.Instance();
		string[] array = new string[6] { "TILE1", "TILE2", "TILE3", "TILE4", "TILE5", "TILE6" };
		for (int i = 0; i < 6; i++)
		{
			TerrainMaterial.DisableKeyword(array[i]);
		}
		int num = Mathf.Clamp(tileToBiome.Length - 1, 0, array.Length - 1);
		TerrainMaterial.EnableKeyword(array[num]);
		TerrainMaterial.SetTexture("_MaskTex", terrain_Mobile.GetMaskTexture());
		for (int j = 0; j < tileToBiome.Length; j++)
		{
			string text = "_TileTex" + (j + 1);
			TerrainMaterial.SetTexture(text, terrain_Mobile.GetTileTextureFromBiome((Biome)tileToBiome[j]));
		}
	}

	public override void HideWorldSpritePool()
	{
		_oldWorldSpritePoolVisibility = _worldSpritePool.gameObject.activeSelf;
		_worldSpritePool.gameObject.SetActive(value: false);
	}

	public override void RestoreWorldSpritePoolVisibility()
	{
		_worldSpritePool.gameObject.SetActive(_oldWorldSpritePoolVisibility);
	}
}
