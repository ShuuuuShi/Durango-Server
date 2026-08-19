using System.Collections.Generic;
using Durango.Logic.Statue;
using UnityEngine;

namespace Durango.Render;

public static class VoxelMeshBuilder
{
	public static Mesh Make(Mesh mesh, VoxelStatue voxel)
	{
		if (mesh == null)
		{
			mesh = new Mesh();
		}
		List<Vector3> list = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();
		List<Color> list2 = new List<Color>();
		List<int> list3 = new List<int>();
		MakeSide(Vector3.up, voxel, list, uvs, list2, list3, 1f);
		MakeSide(Vector3.left, voxel, list, uvs, list2, list3, 0.9f);
		MakeSide(Vector3.back, voxel, list, uvs, list2, list3, 0.8f);
		mesh.Clear();
		mesh.SetVertices(list);
		mesh.SetUVs(0, uvs);
		mesh.SetColors(list2);
		mesh.SetTriangles(list3, 0);
		return mesh;
	}

	public static void MakeSide(Vector3 side, VoxelStatue voxel, List<Vector3> verts, List<Vector2> uvs, List<Color> cols, List<int> tris, float colRatio)
	{
		int num;
		Size3 size;
		Size3 size2;
		Size3 size3;
		if (side.x != 0f)
		{
			num = voxel.Size.X;
			size = new Size3(0, 1, 0);
			size2 = new Size3(0, 0, 1);
			size3 = new Size3(1, 0, 0);
		}
		else if (side.y != 0f)
		{
			num = voxel.Size.Y;
			size = new Size3(1, 0, 0);
			size2 = new Size3(0, 0, 1);
			size3 = new Size3(0, 1, 0);
		}
		else
		{
			if (side.z == 0f)
			{
				return;
			}
			num = voxel.Size.Z;
			size = new Size3(1, 0, 0);
			size2 = new Size3(0, 1, 0);
			size3 = new Size3(0, 0, 1);
		}
		Size3 size4 = new Size3((int)side.x, (int)side.y, (int)side.z);
		int num2 = size.X * voxel.Size.X + size.Y * voxel.Size.Y + size.Z * voxel.Size.Z;
		int num3 = size2.X * voxel.Size.X + size2.Y * voxel.Size.Y + size2.Z * voxel.Size.Z;
		for (int i = 0; i < num; i++)
		{
			for (int j = 0; j < num2; j++)
			{
				for (int k = 0; k < num3; k++)
				{
					int num4 = j * size.X + k * size2.X + i * size3.X;
					int num5 = j * size.Y + k * size2.Y + i * size3.Y;
					int num6 = j * size.Z + k * size2.Z + i * size3.Z;
					byte voxel2 = voxel.GetVoxel(num4, num5, num6);
					if (voxel2 == 0)
					{
						continue;
					}
					if (!voxel.TryGetVoxel(num4 + size4.X, num5 + size4.Y, num6 + size4.Z, out var value))
					{
						value = 0;
					}
					if (value != 0)
					{
						continue;
					}
					int count = verts.Count;
					if (size4.X != 0)
					{
						if (size4.X > 0)
						{
							verts.Add(new Vector3(num4 + 1, num5, num6));
							verts.Add(new Vector3(num4 + 1, num5 + 1, num6));
							verts.Add(new Vector3(num4 + 1, num5 + 1, num6 + 1));
							verts.Add(new Vector3(num4 + 1, num5, num6 + 1));
						}
						else
						{
							verts.Add(new Vector3(num4, num5, num6));
							verts.Add(new Vector3(num4, num5, num6 + 1));
							verts.Add(new Vector3(num4, num5 + 1, num6 + 1));
							verts.Add(new Vector3(num4, num5 + 1, num6));
						}
					}
					else if (size4.Y != 0)
					{
						if (size4.Y > 0)
						{
							verts.Add(new Vector3(num4, num5 + 1, num6));
							verts.Add(new Vector3(num4, num5 + 1, num6 + 1));
							verts.Add(new Vector3(num4 + 1, num5 + 1, num6 + 1));
							verts.Add(new Vector3(num4 + 1, num5 + 1, num6));
						}
						else
						{
							verts.Add(new Vector3(num4, num5, num6));
							verts.Add(new Vector3(num4 + 1, num5, num6));
							verts.Add(new Vector3(num4 + 1, num5, num6 + 1));
							verts.Add(new Vector3(num4, num5, num6 + 1));
						}
					}
					else if (size4.Z != 0)
					{
						if (size4.Z > 0)
						{
							verts.Add(new Vector3(num4, num5, num6 + 1));
							verts.Add(new Vector3(num4 + 1, num5, num6 + 1));
							verts.Add(new Vector3(num4 + 1, num5 + 1, num6 + 1));
							verts.Add(new Vector3(num4, num5 + 1, num6 + 1));
						}
						else
						{
							verts.Add(new Vector3(num4, num5, num6));
							verts.Add(new Vector3(num4, num5 + 1, num6));
							verts.Add(new Vector3(num4 + 1, num5 + 1, num6));
							verts.Add(new Vector3(num4 + 1, num5, num6));
						}
					}
					uvs.Add(new Vector2(0f, 0f));
					uvs.Add(new Vector2(0f, 1f));
					uvs.Add(new Vector2(1f, 1f));
					uvs.Add(new Vector2(1f, 0f));
					for (int l = 0; l < 4; l++)
					{
						int count2 = cols.Count;
						Vector3 vector = verts[count2];
						Size3 size5 = new Size3((int)vector.x, (int)vector.y, (int)vector.z);
						int num7 = 0;
						if (voxel.TryGetVoxel(size5.X - 1, size5.Y, size5.Z, out var value2) && value2 != 0)
						{
							num7++;
						}
						if (voxel.TryGetVoxel(size5.X, size5.Y, size5.Z - 1, out value2) && value2 != 0)
						{
							num7++;
						}
						if (voxel.TryGetVoxel(size5.X - 1, size5.Y - 1, size5.Z - 1, out value2) && value2 != 0)
						{
							num7++;
						}
						float t = Mathf.Lerp(colRatio * 0.5f, colRatio, 1f - Mathf.Max(0f, (float)(num7 - 1) / 2f));
						Color item = Color.Lerp(Color.black, voxel.Colors[voxel2 - 1], t);
						cols.Add(item);
					}
					tris.Add(count);
					tris.Add(count + 1);
					tris.Add(count + 2);
					tris.Add(count + 2);
					tris.Add(count + 3);
					tris.Add(count);
				}
			}
		}
	}
}
