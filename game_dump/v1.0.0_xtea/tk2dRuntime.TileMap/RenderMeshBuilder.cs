using System.Collections.Generic;
using UnityEngine;

namespace tk2dRuntime.TileMap;

public static class RenderMeshBuilder
{
	public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, ColorChunk colorChunk, bool useColor, bool skipPrefabs, int baseX, int baseY)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0299: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0402: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_041f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0315: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0324: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0333: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0395: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>();
		List<Color> list2 = new List<Color>();
		List<Vector2> list3 = new List<Vector2>();
		List<Vector2> list4 = new List<Vector2>();
		int[] spriteIds = chunk.spriteIds;
		Vector3 tileSize = tileMap.data.tileSize;
		int num = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
		Object[] tilePrefabs = (Object[])(object)tileMap.data.tilePrefabs;
		tk2dSpriteDefinition firstValidDefinition = tileMap.SpriteCollectionInst.FirstValidDefinition;
		bool flag = firstValidDefinition != null && firstValidDefinition.normals != null && firstValidDefinition.normals.Length > 0;
		bool generateUv = tileMap.data.generateUv2;
		tk2dTileMapData.ColorMode colorMode = tileMap.data.colorMode;
		Color32 val = Color32.op_Implicit((!useColor || tileMap.ColorChannel == null) ? Color.white : tileMap.ColorChannel.clearColor);
		if (colorChunk == null || colorChunk.colors.Length == 0)
		{
			useColor = false;
		}
		BuilderUtil.GetLoopOrder(tileMap.data.sortMethod, tileMap.partitionSizeX, tileMap.partitionSizeY, out var x, out var x2, out var dx, out var y, out var y2, out var dy);
		float x3 = 0f;
		float y3 = 0f;
		tileMap.data.GetTileOffset(out x3, out y3);
		List<int>[] array = new List<int>[tileMap.SpriteCollectionInst.materials.Length];
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = new List<int>();
		}
		int num2 = tileMap.partitionSizeX + 1;
		Vector3 val2 = default(Vector3);
		for (int j = y; j != y2; j += dy)
		{
			float num3 = (float)((baseY + j) & 1) * x3;
			for (int k = x; k != x2; k += dx)
			{
				int rawTile = spriteIds[j * tileMap.partitionSizeX + k];
				int tileFromRawTile = BuilderUtil.GetTileFromRawTile(rawTile);
				bool flag2 = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.FlipX);
				bool flag3 = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.FlipY);
				bool rot = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.Rot90);
				((Vector3)(ref val2))._002Ector(tileSize.x * ((float)k + num3), tileSize.y * (float)j, 0f);
				if (tileFromRawTile < 0 || tileFromRawTile >= num || (skipPrefabs && Object.op_Implicit(tilePrefabs[tileFromRawTile])))
				{
					continue;
				}
				tk2dSpriteDefinition tk2dSpriteDefinition = tileMap.SpriteCollectionInst.spriteDefinitions[tileFromRawTile];
				int count = list.Count;
				for (int l = 0; l < tk2dSpriteDefinition.positions.Length; l++)
				{
					Vector3 val3 = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, tk2dSpriteDefinition, tk2dSpriteDefinition.positions[l], flag2, flag3, rot);
					if (useColor && colorChunk != null)
					{
						Color val4 = Color32.op_Implicit(colorChunk.colors[j * num2 + k]);
						Color val5 = Color32.op_Implicit(colorChunk.colors[j * num2 + k + 1]);
						Color val6 = Color32.op_Implicit(colorChunk.colors[(j + 1) * num2 + k]);
						Color val7 = Color32.op_Implicit(colorChunk.colors[(j + 1) * num2 + (k + 1)]);
						switch (colorMode)
						{
						case tk2dTileMapData.ColorMode.Interpolate:
						{
							Vector3 val8 = val3 - tk2dSpriteDefinition.untrimmedBoundsData[0];
							Vector3 val9 = val8 + tileMap.data.tileSize * 0.5f;
							float num4 = Mathf.Clamp01(val9.x / tileMap.data.tileSize.x);
							float num5 = Mathf.Clamp01(val9.y / tileMap.data.tileSize.y);
							Color item = Color.Lerp(Color.Lerp(val4, val5, num4), Color.Lerp(val6, val7, num4), num5);
							list2.Add(item);
							break;
						}
						case tk2dTileMapData.ColorMode.Solid:
							list2.Add(val4);
							break;
						}
					}
					else
					{
						list2.Add(Color32.op_Implicit(val));
					}
					if (generateUv)
					{
						if (tk2dSpriteDefinition.normalizedUvs.Length == 0)
						{
							list4.Add(Vector2.zero);
						}
						else
						{
							list4.Add(tk2dSpriteDefinition.normalizedUvs[l]);
						}
					}
					list.Add(val2 + val3);
					list3.Add(tk2dSpriteDefinition.uvs[l]);
				}
				bool flag4 = false;
				if (flag2)
				{
					flag4 = !flag4;
				}
				if (flag3)
				{
					flag4 = !flag4;
				}
				List<int> list5 = array[tk2dSpriteDefinition.materialId];
				for (int m = 0; m < tk2dSpriteDefinition.indices.Length; m++)
				{
					int num6 = ((!flag4) ? m : (tk2dSpriteDefinition.indices.Length - 1 - m));
					list5.Add(count + tk2dSpriteDefinition.indices[num6]);
				}
			}
		}
		if ((Object)(object)chunk.mesh == (Object)null)
		{
			chunk.mesh = tk2dUtil.CreateMesh();
		}
		chunk.mesh.Clear();
		chunk.mesh.vertices = list.ToArray();
		chunk.mesh.uv = list3.ToArray();
		if (generateUv)
		{
			chunk.mesh.uv2 = list4.ToArray();
		}
		chunk.mesh.colors = list2.ToArray();
		List<Material> list6 = new List<Material>();
		int num7 = 0;
		int num8 = 0;
		List<int>[] array2 = array;
		foreach (List<int> list7 in array2)
		{
			if (list7.Count > 0)
			{
				list6.Add(tileMap.SpriteCollectionInst.materialInsts[num7]);
				num8++;
			}
			num7++;
		}
		if (num8 > 0)
		{
			chunk.mesh.subMeshCount = num8;
			chunk.gameObject.GetComponent<Renderer>().materials = list6.ToArray();
			int num9 = 0;
			List<int>[] array3 = array;
			foreach (List<int> list8 in array3)
			{
				if (list8.Count > 0)
				{
					chunk.mesh.SetTriangles(list8.ToArray(), num9);
					num9++;
				}
			}
		}
		chunk.mesh.RecalculateBounds();
		if (flag)
		{
			chunk.mesh.RecalculateNormals();
		}
		MeshFilter component = chunk.gameObject.GetComponent<MeshFilter>();
		component.sharedMesh = chunk.mesh;
	}

	public static void Build(tk2dTileMap tileMap, bool editMode, bool forceBuild)
	{
		bool skipPrefabs = !editMode;
		bool flag = !forceBuild;
		int numLayers = tileMap.data.NumLayers;
		for (int i = 0; i < numLayers; i++)
		{
			Layer layer = tileMap.Layers[i];
			if (layer.IsEmpty)
			{
				continue;
			}
			LayerInfo layerInfo = tileMap.data.Layers[i];
			bool useColor = !tileMap.ColorChannel.IsEmpty && tileMap.data.Layers[i].useColor;
			bool useSortingLayers = tileMap.data.useSortingLayers;
			for (int j = 0; j < layer.numRows; j++)
			{
				int baseY = j * layer.divY;
				for (int k = 0; k < layer.numColumns; k++)
				{
					int baseX = k * layer.divX;
					SpriteChunk chunk = layer.GetChunk(k, j);
					ColorChunk chunk2 = tileMap.ColorChannel.GetChunk(k, j);
					bool flag2 = chunk2?.Dirty ?? false;
					if (flag && !flag2 && !chunk.Dirty)
					{
						continue;
					}
					if ((Object)(object)chunk.mesh != (Object)null)
					{
						chunk.mesh.Clear();
					}
					if (chunk.IsEmpty)
					{
						continue;
					}
					if (editMode || (!editMode && !layerInfo.skipMeshGeneration))
					{
						BuildForChunk(tileMap, chunk, chunk2, useColor, skipPrefabs, baseX, baseY);
						if ((Object)(object)chunk.gameObject != (Object)null && useSortingLayers)
						{
							Renderer component = chunk.gameObject.GetComponent<Renderer>();
							if ((Object)(object)component != (Object)null)
							{
								component.sortingLayerName = layerInfo.sortingLayerName;
								component.sortingOrder = layerInfo.sortingOrder;
							}
						}
					}
					if ((Object)(object)chunk.mesh != (Object)null)
					{
						tileMap.TouchMesh(chunk.mesh);
					}
				}
			}
		}
	}
}
