using MsgPack;

namespace Messages;

public struct GetSimilarProducts
{
	public const uint TypeCode = 5008u;

	public string PrototypeId;

	public int Level;

	public string[] MajorTags;

	public int? Limit;

	public static void Pack(Packer packer, GetSimilarProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(5008u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.Level);
		if (val.MajorTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.MajorTags.Length);
			for (int i = 0; i < val.MajorTags.Length; i++)
			{
				if (val.MajorTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.MajorTags[i]);
				}
			}
		}
		if (!val.Limit.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.Limit.Value);
		}
	}

	public static GetSimilarProducts Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetSimilarProducts result = default(GetSimilarProducts);
		result.PrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.MajorTags = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.MajorTags[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Limit = null;
		}
		else
		{
			int value = unpacker.LastReadData.AsInt32();
			result.Limit = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<GetSimilarProducts PrototypeId={PrototypeId} Level={Level} MajorTags={MajorTags} Limit={Limit}>";
	}
}
