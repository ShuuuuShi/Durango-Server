using MsgPack;

namespace Messages;

public struct RequiredItems
{
	public string[] RequiredTags;

	public int Count;

	public static void Pack(Packer packer, RequiredItems val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.RequiredTags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.RequiredTags.Length);
			for (int i = 0; i < val.RequiredTags.Length; i++)
			{
				if (val.RequiredTags[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.RequiredTags[i]);
				}
			}
		}
		packer.Pack(val.Count);
	}

	public static RequiredItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		RequiredItems result = default(RequiredItems);
		result.RequiredTags = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.RequiredTags[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<RequiredItems RequiredTags={RequiredTags} Count={Count}>";
	}
}
