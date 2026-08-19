using System;
using System.Collections.Generic;
using UnityEngine;

namespace tk2dRuntime.TileMap;

public static class ColliderBuilder3D
{
	public static void Build(tk2dTileMap tileMap, bool forceBuild)
	{
		bool flag = !forceBuild;
		int num = tileMap.Layers.Length;
		for (int i = 0; i < num; i++)
		{
			Layer layer = tileMap.Layers[i];
			if (layer.IsEmpty || !tileMap.data.Layers[i].generateCollider)
			{
				continue;
			}
			for (int j = 0; j < layer.numRows; j++)
			{
				int baseY = j * layer.divY;
				for (int k = 0; k < layer.numColumns; k++)
				{
					int baseX = k * layer.divX;
					SpriteChunk chunk = layer.GetChunk(k, j);
					if ((!flag || chunk.Dirty) && !chunk.IsEmpty)
					{
						BuildForChunk(tileMap, chunk, baseX, baseY);
						PhysicMaterial physicMaterial = tileMap.data.Layers[i].physicMaterial;
						if ((Object)(object)chunk.meshCollider != (Object)null)
						{
							((Collider)chunk.meshCollider).sharedMaterial = physicMaterial;
						}
					}
				}
			}
		}
	}

	public static void BuildForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY)
	{
		Vector3[] vertices = (Vector3[])(object)new Vector3[0];
		int[] indices = new int[0];
		BuildLocalMeshForChunk(tileMap, chunk, baseX, baseY, ref vertices, ref indices);
		if (indices.Length > 6)
		{
			vertices = WeldVertices(vertices, ref indices);
			indices = RemoveDuplicateFaces(indices);
		}
		foreach (EdgeCollider2D edgeCollider in chunk.edgeColliders)
		{
			if ((Object)(object)edgeCollider != (Object)null)
			{
				tk2dUtil.DestroyImmediate((Object)(object)edgeCollider);
			}
		}
		chunk.edgeColliders.Clear();
		if (vertices.Length > 0)
		{
			if ((Object)(object)chunk.colliderMesh != (Object)null)
			{
				tk2dUtil.DestroyImmediate((Object)(object)chunk.colliderMesh);
				chunk.colliderMesh = null;
			}
			if ((Object)(object)chunk.meshCollider == (Object)null)
			{
				chunk.meshCollider = chunk.gameObject.GetComponent<MeshCollider>();
				if ((Object)(object)chunk.meshCollider == (Object)null)
				{
					chunk.meshCollider = tk2dUtil.AddComponent<MeshCollider>(chunk.gameObject);
				}
			}
			chunk.colliderMesh = tk2dUtil.CreateMesh();
			chunk.colliderMesh.vertices = vertices;
			chunk.colliderMesh.triangles = indices;
			chunk.colliderMesh.RecalculateBounds();
			chunk.meshCollider.sharedMesh = chunk.colliderMesh;
		}
		else
		{
			chunk.DestroyColliderData(tileMap);
		}
	}

	private static void BuildLocalMeshForChunk(tk2dTileMap tileMap, SpriteChunk chunk, int baseX, int baseY, ref Vector3[] vertices, ref int[] indices)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		List<Vector3> list = new List<Vector3>();
		List<int> list2 = new List<int>();
		int num = tileMap.SpriteCollectionInst.spriteDefinitions.Length;
		Vector3 tileSize = tileMap.data.tileSize;
		GameObject[] tilePrefabs = tileMap.data.tilePrefabs;
		float x = 0f;
		float y = 0f;
		tileMap.data.GetTileOffset(out x, out y);
		int[] spriteIds = chunk.spriteIds;
		Vector3 val = default(Vector3);
		for (int i = 0; i < tileMap.partitionSizeY; i++)
		{
			float num2 = (float)((baseY + i) & 1) * x;
			for (int j = 0; j < tileMap.partitionSizeX; j++)
			{
				int rawTile = spriteIds[i * tileMap.partitionSizeX + j];
				int tileFromRawTile = BuilderUtil.GetTileFromRawTile(rawTile);
				((Vector3)(ref val))._002Ector(tileSize.x * ((float)j + num2), tileSize.y * (float)i, 0f);
				if (tileFromRawTile < 0 || tileFromRawTile >= num || Object.op_Implicit((Object)(object)tilePrefabs[tileFromRawTile]))
				{
					continue;
				}
				bool flag = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.FlipX);
				bool flag2 = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.FlipY);
				bool rot = BuilderUtil.IsRawTileFlagSet(rawTile, tk2dTileFlags.Rot90);
				bool flag3 = false;
				if (flag)
				{
					flag3 = !flag3;
				}
				if (flag2)
				{
					flag3 = !flag3;
				}
				tk2dSpriteDefinition tk2dSpriteDefinition = tileMap.SpriteCollectionInst.spriteDefinitions[tileFromRawTile];
				int count = list.Count;
				if (tk2dSpriteDefinition.colliderType == tk2dSpriteDefinition.ColliderType.Box)
				{
					Vector3 val2 = tk2dSpriteDefinition.colliderVertices[0];
					Vector3 val3 = tk2dSpriteDefinition.colliderVertices[1];
					Vector3 val4 = val2 - val3;
					Vector3 val5 = val2 + val3;
					Vector3[] array = (Vector3[])(object)new Vector3[8]
					{
						new Vector3(val4.x, val4.y, val4.z),
						new Vector3(val4.x, val4.y, val5.z),
						new Vector3(val5.x, val4.y, val4.z),
						new Vector3(val5.x, val4.y, val5.z),
						new Vector3(val4.x, val5.y, val4.z),
						new Vector3(val4.x, val5.y, val5.z),
						new Vector3(val5.x, val5.y, val4.z),
						new Vector3(val5.x, val5.y, val5.z)
					};
					for (int k = 0; k < 8; k++)
					{
						Vector3 val6 = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, tk2dSpriteDefinition, array[k], flag, flag2, rot);
						list.Add(val6 + val);
					}
					int[] array2 = new int[24]
					{
						2, 1, 0, 3, 1, 2, 4, 5, 6, 6,
						5, 7, 6, 7, 3, 6, 3, 2, 1, 5,
						4, 0, 1, 4
					};
					int[] array3 = array2;
					for (int l = 0; l < array3.Length; l++)
					{
						int num3 = ((!flag3) ? l : (array3.Length - 1 - l));
						list2.Add(count + array3[num3]);
					}
				}
				else if (tk2dSpriteDefinition.colliderType == tk2dSpriteDefinition.ColliderType.Mesh)
				{
					for (int m = 0; m < tk2dSpriteDefinition.colliderVertices.Length; m++)
					{
						Vector3 val7 = BuilderUtil.ApplySpriteVertexTileFlags(tileMap, tk2dSpriteDefinition, tk2dSpriteDefinition.colliderVertices[m], flag, flag2, rot);
						list.Add(val7 + val);
					}
					int[] colliderIndicesFwd = tk2dSpriteDefinition.colliderIndicesFwd;
					for (int n = 0; n < colliderIndicesFwd.Length; n++)
					{
						int num4 = ((!flag3) ? n : (colliderIndicesFwd.Length - 1 - n));
						list2.Add(count + colliderIndicesFwd[num4]);
					}
				}
			}
		}
		vertices = list.ToArray();
		indices = list2.ToArray();
	}

	private static int CompareWeldVertices(Vector3 a, Vector3 b)
	{
		float num = 0.01f;
		float num2 = a.x - b.x;
		if (Mathf.Abs(num2) > num)
		{
			return (int)Mathf.Sign(num2);
		}
		float num3 = a.y - b.y;
		if (Mathf.Abs(num3) > num)
		{
			return (int)Mathf.Sign(num3);
		}
		float num4 = a.z - b.z;
		if (Mathf.Abs(num4) > num)
		{
			return (int)Mathf.Sign(num4);
		}
		return 0;
	}

	private static Vector3[] WeldVertices(Vector3[] vertices, ref int[] indices)
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		int[] array = new int[vertices.Length];
		for (int i = 0; i < vertices.Length; i++)
		{
			array[i] = i;
		}
		Array.Sort(array, (int a, int b) => CompareWeldVertices(vertices[a], vertices[b]));
		List<Vector3> list = new List<Vector3>();
		int[] array2 = new int[vertices.Length];
		Vector3 val = vertices[array[0]];
		list.Add(val);
		array2[array[0]] = list.Count - 1;
		for (int j = 1; j < array.Length; j++)
		{
			Vector3 val2 = vertices[array[j]];
			if (CompareWeldVertices(val2, val) != 0)
			{
				val = val2;
				list.Add(val);
				array2[array[j]] = list.Count - 1;
			}
			array2[array[j]] = list.Count - 1;
		}
		for (int k = 0; k < indices.Length; k++)
		{
			indices[k] = array2[indices[k]];
		}
		return list.ToArray();
	}

	private static int CompareDuplicateFaces(int[] indices, int face0index, int face1index)
	{
		for (int i = 0; i < 3; i++)
		{
			int num = indices[face0index + i] - indices[face1index + i];
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	private static int[] RemoveDuplicateFaces(int[] indices)
	{
		int[] sortedFaceIndices = new int[indices.Length];
		for (int i = 0; i < indices.Length; i += 3)
		{
			int[] array = new int[3]
			{
				indices[i],
				indices[i + 1],
				indices[i + 2]
			};
			Array.Sort(array);
			sortedFaceIndices[i] = array[0];
			sortedFaceIndices[i + 1] = array[1];
			sortedFaceIndices[i + 2] = array[2];
		}
		int[] array2 = new int[indices.Length / 3];
		for (int j = 0; j < indices.Length; j += 3)
		{
			array2[j / 3] = j;
		}
		Array.Sort(array2, (int a, int b) => CompareDuplicateFaces(sortedFaceIndices, a, b));
		List<int> list = new List<int>();
		for (int k = 0; k < array2.Length; k++)
		{
			if (k != array2.Length - 1 && CompareDuplicateFaces(sortedFaceIndices, array2[k], array2[k + 1]) == 0)
			{
				k++;
				continue;
			}
			for (int l = 0; l < 3; l++)
			{
				list.Add(indices[array2[k] + l]);
			}
		}
		return list.ToArray();
	}
}
