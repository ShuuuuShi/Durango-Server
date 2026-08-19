using System.Collections.Generic;
using System.IO;

namespace Durango.Terrain;

public class LandmarkInfo : CoordInfo
{
	public const int RawLandmarkDataSize = 16;

	public ushort Id { get; set; }

	public byte Rotate { get; set; }

	public short OffsetX { get; set; }

	public short OffsetY { get; set; }

	public short OffsetZ { get; set; }

	public byte ScaleX { get; set; }

	public byte ScaleY { get; set; }

	public byte ScaleZ { get; set; }

	public override string ToString()
	{
		string landmarkPrefab = TerrainMeta.GetLandmarkPrefab(Id);
		return $"{landmarkPrefab} ({base.X},{base.Y})";
	}

	private static void ToBytes(LandmarkInfo info, BinaryWriter writer)
	{
		writer.Write(info.X);
		writer.Write(info.Y);
		writer.Write(info.Id);
		writer.Write(info.Rotate);
		writer.Write(info.OffsetX);
		writer.Write(info.OffsetY);
		writer.Write(info.OffsetZ);
		writer.Write(info.ScaleX);
		writer.Write(info.ScaleY);
		writer.Write(info.ScaleZ);
	}

	private static LandmarkInfo FromBytes(BinaryReader reader)
	{
		return new LandmarkInfo
		{
			X = reader.ReadUInt16(),
			Y = reader.ReadUInt16(),
			Id = reader.ReadUInt16(),
			Rotate = reader.ReadByte(),
			OffsetX = reader.ReadInt16(),
			OffsetY = reader.ReadInt16(),
			OffsetZ = reader.ReadInt16(),
			ScaleX = reader.ReadByte(),
			ScaleY = reader.ReadByte(),
			ScaleZ = reader.ReadByte()
		};
	}

	public static byte[] ToBytes(LandmarkInfo info)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter writer = new BinaryWriter(memoryStream))
		{
			ToBytes(info, writer);
		}
		return memoryStream.ToArray();
	}

	public static byte[] ToBytes(IList<LandmarkInfo> infos)
	{
		MemoryStream memoryStream = new MemoryStream();
		using (BinaryWriter writer = new BinaryWriter(memoryStream))
		{
			for (int i = 0; i < infos.Count; i++)
			{
				ToBytes(infos[i], writer);
			}
		}
		return memoryStream.ToArray();
	}

	public static LandmarkInfo[] FromBytes(byte[] rawLandmarkData, int offset = 0)
	{
		int num = rawLandmarkData.Length - offset;
		if (num % 16 != 0)
		{
			return null;
		}
		int num2 = num / 16;
		using BinaryReader reader = new BinaryReader(new MemoryStream(rawLandmarkData, offset, num));
		LandmarkInfo[] array = new LandmarkInfo[num2];
		for (int i = 0; i < num2; i++)
		{
			array[i] = FromBytes(reader);
		}
		return array;
	}
}
