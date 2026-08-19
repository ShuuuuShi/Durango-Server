using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Durango.Render.Sprite;
using Durango.Utils;
using Shared.Region;
using UnityEngine;

namespace Durango.Terrain;

public class TerrainChunk_PC : TerrainChunkBase
{
	protected override void InitTerrainMesh(int numTilesInChunkX, int numTilesInChunkY)
	{
		TerrainMaterial = GetComponent<Renderer>().material;
		TerrainMesh = GetComponent<MeshFilter>().mesh;
		Vector3[] array = new Vector3[numTilesInChunkX * numTilesInChunkY * 4];
		Vector3[] array2 = new Vector3[numTilesInChunkX * numTilesInChunkY * 4];
		Vector2[] array3 = new Vector2[numTilesInChunkX * numTilesInChunkY * 4];
		int[] array4 = new int[numTilesInChunkX * numTilesInChunkY * 6];
		int num = 0;
		int num2 = 0;
		for (int i = 0; i < numTilesInChunkY; i++)
		{
			for (int j = 0; j < numTilesInChunkX; j++)
			{
				int num3 = num;
				Vector3 vector = new Vector3(((float)j - 8f) * 200f, 0f, ((float)i - 8f) * 200f);
				Vector2 vector2 = new Vector2((float)j % 2f * 0.5f, (float)i % 2f * 0.5f);
				array[num] = vector;
				ref Vector3 reference = ref array2[num];
				reference = Vector3.up;
				array3[num] = vector2;
				num++;
				ref Vector3 reference2 = ref array[num];
				reference2 = vector + new Vector3(0f, 0f, 201.5635f);
				ref Vector3 reference3 = ref array2[num];
				reference3 = Vector3.up;
				ref Vector2 reference4 = ref array3[num];
				reference4 = vector2 + new Vector2(0f, 0.499f);
				num++;
				ref Vector3 reference5 = ref array[num];
				reference5 = vector + new Vector3(201.5635f, 0f, 0f);
				ref Vector3 reference6 = ref array2[num];
				reference6 = Vector3.up;
				ref Vector2 reference7 = ref array3[num];
				reference7 = vector2 + new Vector2(0.499f, 0f);
				num++;
				ref Vector3 reference8 = ref array[num];
				reference8 = vector + new Vector3(201.5635f, 0f, 201.5635f);
				ref Vector3 reference9 = ref array2[num];
				reference9 = Vector3.up;
				ref Vector2 reference10 = ref array3[num];
				reference10 = vector2 + new Vector2(0.499f, 0.499f);
				num++;
				array4[num2++] = num3;
				array4[num2++] = num3 + 3;
				array4[num2++] = num3 + 2;
				array4[num2++] = num3 + 1;
				array4[num2++] = num3 + 3;
				array4[num2++] = num3;
			}
		}
		TerrainMesh.vertices = array;
		TerrainMesh.normals = array2;
		TerrainMesh.uv = array3;
		TerrainMesh.triangles = array4;
		TerrainMesh.tangents = null;
	}

	protected override IEnumerator LoadGrass(byte[] tileBiomeData)
	{
		yield return null;
	}

	protected override bool AddGrass(byte rawBiome, int x, int y)
	{
		if (Util.IsNotPlantableMasked(rawBiome))
		{
			return false;
		}
		return true;
	}

	public override void RemoveGrass(Point2 tile)
	{
	}

	protected override IEnumerator LoadNaturals(IList<NaturalInfo> naturalData)
	{
		if (naturalData == null)
		{
			yield break;
		}
		for (int i = 0; i < naturalData.Count; i++)
		{
			AddNaturalEntity(naturalData[i]);
			if ((i + 1) % 10 == 0)
			{
				yield return null;
			}
		}
	}

	public override bool AddNaturalEntity(NaturalInfo natural, Action<GameObject> callback = null)
	{
		if (!DataHelper.IsNaturalObject(natural.EntityType))
		{
			return false;
		}
		Point2 point = FromWorldTile(new Point2(natural.X, natural.Y));
		BiomeSpriteInfo biomeSpriteInfo = DataHelper.GetBiomeSpriteInfo(natural.EntityType);
		if (biomeSpriteInfo == null)
		{
			return false;
		}
		string text = RandomNaturaleName(biomeSpriteInfo, point);
		if (string.IsNullOrEmpty(text))
		{
			return false;
		}
		if (text.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
		{
			AddNaturalPrefab(string.Empty, natural.EntityType, text, point);
			return false;
		}
		AddNaturalHqPrefabFromSpriteName(biomeSpriteInfo, text, point);
		return false;
	}

	private bool AddNaturalHqPrefabFromSpriteName(BiomeSpriteInfo spriteInfo, string spriteName, Point2 propTile)
	{
		SpriteGroup kSpriteGroup = Singleton<SpriteManager>.Instance().GetKSpriteGroup(spriteInfo.SpriteObjectType);
		if (kSpriteGroup == null)
		{
			return false;
		}
		SpriteCollectionInfo spriteCollectionInfo = Singleton<SpriteManager>.Instance().GetSpriteCollectionInfo(spriteName, autoLoad: false);
		if (spriteCollectionInfo == null)
		{
			return false;
		}
		string directoryName = Path.GetDirectoryName(spriteCollectionInfo.SpriteCollectionPath);
		if (!string.IsNullOrEmpty(directoryName) && directoryName.StartsWith("Sprite/"))
		{
			string text = "Models/Prop/hq_nature/" + directoryName.Substring("Sprite/".Length);
			int num = text.LastIndexOf('/');
			if (text.Substring(num + 1).StartsWith("Data"))
			{
				text = text.Substring(0, num);
			}
			string prefab = text + "/hq_" + spriteName + ".prefab";
			AddNaturalPrefab(string.Empty, (ushort)spriteInfo.EntityType, prefab, propTile);
		}
		return true;
	}

	protected override void AssignMaterial(int[] tileToBiome)
	{
		Terrain_PC terrain_PC = Terrain_PC.Instance();
		string[] array = new string[6] { "TILE1", "TILE2", "TILE3", "TILE4", "TILE5", "TILE6" };
		for (int i = 0; i < 6; i++)
		{
			TerrainMaterial.DisableKeyword(array[i]);
		}
		TerrainMaterial.EnableKeyword(array[tileToBiome.Length - 1]);
		TerrainMaterial.SetTexture("_MaskTex", terrain_PC.GetMaskTexture());
		for (int j = 0; j < tileToBiome.Length; j++)
		{
			Terrain_PC.TextureSet tileTextureSetFromBiome = terrain_PC.GetTileTextureSetFromBiome((Biome)tileToBiome[j]);
			TerrainMaterial.SetTexture("_TileTex" + (j + 1), tileTextureSetFromBiome.Base);
			TerrainMaterial.SetTexture("_PBRTex" + (j + 1), tileTextureSetFromBiome.Pbr);
		}
	}
}
