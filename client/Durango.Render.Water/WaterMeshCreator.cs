using UnityEngine;

namespace Durango.Render.Water;

public static class WaterMeshCreator
{
	public static Mesh CreateMesh(Point2 tileIndex, Point2 tileSize, Point2 chunkSize)
	{
		Mesh mesh = new Mesh();
		int num = (tileSize.x + 1) * (tileSize.y + 1);
		Vector3[] array = new Vector3[num];
		Vector2[] array2 = new Vector2[num];
		Vector3[] array3 = new Vector3[num];
		int[] array4 = new int[tileSize.x * tileSize.y * 3 * 2];
		int num2 = tileIndex.x * tileSize.x;
		int num3 = tileIndex.y * tileSize.y;
		int num4 = 0;
		int num5 = 0;
		Vector2 vector = new Vector2((float)num2 / (float)chunkSize.x, (float)num3 / (float)chunkSize.y);
		for (int i = 0; i < tileSize.y + 1; i++)
		{
			for (int j = 0; j < tileSize.x + 1; j++)
			{
				int num6 = num4;
				ref Vector3 reference = ref array[num4];
				reference = new Vector3((float)j / (float)tileSize.x, 0f, (float)i / (float)tileSize.y);
				ref Vector2 reference2 = ref array2[num4];
				reference2 = vector + new Vector2((float)j / (float)chunkSize.x, (float)i / (float)chunkSize.y);
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
		mesh.vertices = array;
		mesh.normals = array3;
		mesh.triangles = array4;
		mesh.tangents = null;
		mesh.uv = array2;
		mesh.uv2 = array2;
		return mesh;
	}
}
