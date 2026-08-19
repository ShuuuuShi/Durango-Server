using UnityEngine;

namespace Durango.Logic.Statue;

public class VoxelStatue
{
	public byte[] Bytes;

	public Size3 Size;

	public Color[] Colors;

	public byte GetVoxel(int x, int y, int z)
	{
		int num = z * (Size.X * Size.Y) + y * Size.X + x;
		return Bytes[num];
	}

	public void SetVoxel(int x, int y, int z, byte value)
	{
		int num = z * (Size.X * Size.Y) + y * Size.X + x;
		Bytes[num] = value;
	}

	public bool TryGetVoxel(int x, int y, int z, out byte value)
	{
		if (x < 0 || x >= Size.X || y < 0 || y >= Size.Y || z < 0 || z >= Size.Z)
		{
			value = 0;
			return false;
		}
		value = GetVoxel(x, y, z);
		return true;
	}
}
