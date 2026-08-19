using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct BuildEstimation
{
	public const uint TypeCode = 2415u;

	public int Level;

	public float Durability;

	public Dictionary<string, int> Tags;

	public int UnrevealedRareTagCount;

	public ArtifactPreview ArtifactPreview;

	public static void Pack(Packer packer, BuildEstimation val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(2415u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack(val.Level);
		packer.Pack(val.Durability);
		if (val.Tags == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Tags.Count);
			foreach (KeyValuePair<string, int> tag in val.Tags)
			{
				if (tag.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(tag.Key);
				}
				packer.Pack(tag.Value);
			}
		}
		packer.Pack(val.UnrevealedRareTagCount);
		ArtifactPreview.Pack(packer, val.ArtifactPreview);
	}

	public static BuildEstimation Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BuildEstimation result = default(BuildEstimation);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Durability = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Tags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			int value = unpacker.LastReadData.AsInt32();
			result.Tags.Add(key, value);
		}
		unpacker.Read();
		result.UnrevealedRareTagCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ArtifactPreview = ArtifactPreview.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<BuildEstimation Level={Level} Durability={Durability} Tags={Tags} UnrevealedRareTagCount={UnrevealedRareTagCount} ArtifactPreview={ArtifactPreview}>";
	}
}
