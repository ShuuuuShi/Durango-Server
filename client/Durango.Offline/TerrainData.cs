using Durango.Terrain;
using JetBrains.Annotations;

namespace Durango.Offline;

public class TerrainData
{
	public byte[] Biomes;

	public byte[] Ocean;

	public byte[] Rivers;

	[CanBeNull]
	public byte[] Landmarks;

	[CanBeNull]
	public byte[] Garden;

	public TerrainInfoJson Info;

	public int Width;

	public int Height;
}
