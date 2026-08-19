using System.Collections.Generic;
using System.IO;

namespace Durango.Terrain;

public class NaturalInfo : CoordInfo
{
	public const int RawNaturalDataSize = 6;

	public ushort EntityType;

	public override string ToString()
	{
		return $"Natural: {EntityType} ({base.X},{base.Y})";
	}

	private static void ToBytes(NaturalInfo info, BinaryWriter writer)
	{
		writer.Write(info.X);
		writer.Write(info.Y);
		writer.Write(info.EntityType);
	}

	private static NaturalInfo FromBytes(BinaryReader reader)
	{
		NaturalInfo naturalInfo = new NaturalInfo();
		naturalInfo.X = reader.ReadUInt16();
		naturalInfo.Y = reader.ReadUInt16();
		naturalInfo.EntityType = reader.ReadUInt16();
		return naturalInfo;
	}

	public static byte[] ToBytes(IList<NaturalInfo> infos)
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

	public static NaturalInfo[] FromBytes(byte[] rawLandmarkData)
	{
		int num = rawLandmarkData.Length / 6;
		using BinaryReader reader = new BinaryReader(new MemoryStream(rawLandmarkData));
		NaturalInfo[] array = new NaturalInfo[num];
		for (int i = 0; i < num; i++)
		{
			array[i] = FromBytes(reader);
		}
		return array;
	}
}
