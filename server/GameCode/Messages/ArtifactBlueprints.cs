using MsgPack;

namespace Messages;

public struct ArtifactBlueprints
{
	public const uint TypeCode = 316u;

	public string[] Ids;

	public string[] LikedBlueprintIds;

	public string[] NewBlueprintIds;

	public static void Pack(Packer packer, ArtifactBlueprints val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(316u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.Ids == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Ids.Length);
			for (int i = 0; i < val.Ids.Length; i++)
			{
				if (val.Ids[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Ids[i]);
				}
			}
		}
		if (val.LikedBlueprintIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.LikedBlueprintIds.Length);
			for (int j = 0; j < val.LikedBlueprintIds.Length; j++)
			{
				if (val.LikedBlueprintIds[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.LikedBlueprintIds[j]);
				}
			}
		}
		if (val.NewBlueprintIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.NewBlueprintIds.Length);
		for (int k = 0; k < val.NewBlueprintIds.Length; k++)
		{
			if (val.NewBlueprintIds[k] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.NewBlueprintIds[k]);
			}
		}
	}

	public static ArtifactBlueprints Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ArtifactBlueprints result = default(ArtifactBlueprints);
		result.Ids = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Ids[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.LikedBlueprintIds = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.LikedBlueprintIds[j] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.NewBlueprintIds = new string[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			result.NewBlueprintIds[k] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactBlueprints Ids={Ids} LikedBlueprintIds={LikedBlueprintIds} NewBlueprintIds={NewBlueprintIds}>";
	}
}
