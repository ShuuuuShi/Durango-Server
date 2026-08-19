using System.Collections.Generic;
using UnityEngine;

public class WaterMeshCreator : KSingleton<WaterMeshCreator>
{
	private struct MeshInfo
	{
		public Point2 TileIndex;

		public Point2 TileSize;

		public Point2 ChunkSize;

		public Mesh Mesh;
	}

	private readonly List<MeshInfo> _sharedMeshList = new List<MeshInfo>();

	public Mesh GetSharedMesh(Point2 tileIndex, Point2 tileSize, Point2 chunkSize)
	{
		for (int i = 0; i < _sharedMeshList.Count; i++)
		{
			MeshInfo meshInfo = _sharedMeshList[i];
			if (!(meshInfo.TileIndex != tileIndex) && !(meshInfo.TileSize != tileSize) && !(meshInfo.ChunkSize != chunkSize))
			{
				return meshInfo.Mesh;
			}
		}
		Mesh val = CreateMesh(tileIndex, tileSize, chunkSize);
		_sharedMeshList.Add(new MeshInfo
		{
			TileIndex = tileIndex,
			TileSize = tileSize,
			ChunkSize = chunkSize,
			Mesh = val
		});
		return val;
	}

	public Mesh CreateMesh(Point2 tileIndex, Point2 tileSize, Point2 chunkSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		Mesh val = new Mesh();
		int num = (tileSize.x + 1) * (tileSize.y + 1);
		Vector3[] array = (Vector3[])(object)new Vector3[num];
		Vector2[] array2 = (Vector2[])(object)new Vector2[num];
		Vector3[] array3 = (Vector3[])(object)new Vector3[num];
		int[] array4 = new int[tileSize.x * tileSize.y * 3 * 2];
		int num2 = tileIndex.x * tileSize.x;
		int num3 = tileIndex.y * tileSize.y;
		int num4 = 0;
		int num5 = 0;
		Vector2 val2 = default(Vector2);
		((Vector2)(ref val2))._002Ector((float)num2 / (float)chunkSize.x, (float)num3 / (float)chunkSize.y);
		for (int i = 0; i < tileSize.y + 1; i++)
		{
			for (int j = 0; j < tileSize.x + 1; j++)
			{
				int num6 = num4;
				ref Vector3 reference = ref array[num4];
				reference = new Vector3((float)j / (float)tileSize.x, 0f, (float)i / (float)tileSize.y);
				ref Vector2 reference2 = ref array2[num4];
				reference2 = val2 + new Vector2((float)j / (float)chunkSize.x, (float)i / (float)chunkSize.y);
				ref Vector3 reference3 = ref array3[num4];
				reference3 = new Vector3(0f, 1f, 0f);
				num4++;
				if (j < tileSize.x && i < tileSize.y)
				{
					int num7 = num6 + 1;
					int num8 = j + 1 + (i + 1) * (tileSize.x + 1);
					int num9 = j + (i + 1) * (tileSize.y + 1);
					array4[num5++] = num6;
					array4[num5++] = num9;
					array4[num5++] = num8;
					array4[num5++] = num6;
					array4[num5++] = num8;
					array4[num5++] = num7;
				}
			}
		}
		val.vertices = array;
		val.normals = array3;
		val.triangles = array4;
		val.tangents = null;
		val.uv = array2;
		val.uv2 = array2;
		return val;
	}
}
