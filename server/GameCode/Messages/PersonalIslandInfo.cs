using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PersonalIslandInfo
{
	public const uint TypeCode = 231978u;

	public string PlayerEntityId;

	public RadioId RadioId;

	public string RegionId;

	public string TerrainId;

	public string TerrainHash;

	public string TemplateId;

	public Point2 TileCount;

	public Pair<Point2, Point2> EstateCellArea;

	public Dictionary<Point2, string[]> ArtifactsByChunks;

	public Dictionary<Point2, string[]> InteriorPropsByChunks;

	public static void Pack(Packer packer, PersonalIslandInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(11);
			packer.Pack(231978u);
		}
		else
		{
			packer.PackArrayHeader(10);
		}
		if (val.PlayerEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerEntityId);
		}
		RadioId.Pack(packer, val.RadioId);
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		if (val.TerrainId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TerrainId);
		}
		if (val.TerrainHash == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TerrainHash);
		}
		if (val.TemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TemplateId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.TileCount.x);
		packer.Pack((ushort)val.TileCount.y);
		packer.PackArrayHeader(2);
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.EstateCellArea.Item1.x);
		packer.Pack((byte)val.EstateCellArea.Item1.y);
		packer.PackArrayHeader(2);
		packer.Pack((byte)val.EstateCellArea.Item2.x);
		packer.Pack((byte)val.EstateCellArea.Item2.y);
		if (val.ArtifactsByChunks == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.ArtifactsByChunks.Count);
			foreach (KeyValuePair<Point2, string[]> artifactsByChunk in val.ArtifactsByChunks)
			{
				packer.PackArrayHeader(2);
				packer.Pack((byte)artifactsByChunk.Key.x);
				packer.Pack((byte)artifactsByChunk.Key.y);
				if (artifactsByChunk.Value == null)
				{
					packer.PackArrayHeader(0);
					continue;
				}
				packer.PackArrayHeader(artifactsByChunk.Value.Length);
				for (int i = 0; i < artifactsByChunk.Value.Length; i++)
				{
					if (artifactsByChunk.Value[i] == null)
					{
						packer.PackString(string.Empty);
					}
					else
					{
						packer.PackString(artifactsByChunk.Value[i]);
					}
				}
			}
		}
		if (val.InteriorPropsByChunks == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.InteriorPropsByChunks.Count);
		foreach (KeyValuePair<Point2, string[]> interiorPropsByChunk in val.InteriorPropsByChunks)
		{
			packer.PackArrayHeader(2);
			packer.Pack((byte)interiorPropsByChunk.Key.x);
			packer.Pack((byte)interiorPropsByChunk.Key.y);
			if (interiorPropsByChunk.Value == null)
			{
				packer.PackArrayHeader(0);
				continue;
			}
			packer.PackArrayHeader(interiorPropsByChunk.Value.Length);
			for (int j = 0; j < interiorPropsByChunk.Value.Length; j++)
			{
				if (interiorPropsByChunk.Value[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(interiorPropsByChunk.Value[j]);
				}
			}
		}
	}

	public static PersonalIslandInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PersonalIslandInfo result = default(PersonalIslandInfo);
		result.PlayerEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RadioId = RadioId.Unpack(unpacker);
		unpacker.Read();
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TerrainId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TerrainHash = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TemplateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.TileCount.x = result2;
		unpacker.ReadUInt16(out result2);
		result.TileCount.y = result2;
		unpacker.Read();
		unpacker.Read();
		unpacker.ReadByte(out var result3);
		Point2 item = default(Point2);
		item.x = result3;
		unpacker.ReadByte(out result3);
		item.y = result3;
		unpacker.Read();
		unpacker.ReadByte(out var result4);
		Point2 item2 = default(Point2);
		item2.x = result4;
		unpacker.ReadByte(out result4);
		item2.y = result4;
		result.EstateCellArea = new Pair<Point2, Point2>(item, item2);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.ArtifactsByChunks = new Dictionary<Point2, string[]>(num);
		Point2 key = default(Point2);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.ReadByte(out var result5);
			key.x = result5;
			unpacker.ReadByte(out result5);
			key.y = result5;
			unpacker.Read();
			int num2 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				array[j] = unpacker.LastReadData.AsString();
			}
			result.ArtifactsByChunks.Add(key, array);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.InteriorPropsByChunks = new Dictionary<Point2, string[]>(num3);
		Point2 key2 = default(Point2);
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			unpacker.ReadByte(out var result6);
			key2.x = result6;
			unpacker.ReadByte(out result6);
			key2.y = result6;
			unpacker.Read();
			int num4 = unpacker.LastReadData.AsInt32();
			string[] array2 = new string[num4];
			for (int l = 0; l < num4; l++)
			{
				unpacker.Read();
				array2[l] = unpacker.LastReadData.AsString();
			}
			result.InteriorPropsByChunks.Add(key2, array2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PersonalIslandInfo PlayerEntityId={PlayerEntityId} RadioId={RadioId} RegionId={RegionId} TerrainId={TerrainId} TerrainHash={TerrainHash} TemplateId={TemplateId} TileCount={TileCount} EstateCellArea={EstateCellArea} ArtifactsByChunks={ArtifactsByChunks} InteriorPropsByChunks={InteriorPropsByChunks}>";
	}
}
