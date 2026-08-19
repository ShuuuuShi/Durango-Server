using MsgPack;

namespace Messages;

public struct Tag
{
	public string Id;

	public int Level;

	public static void Pack(Packer packer, Tag val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.Level);
	}

	public static Tag Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Tag result = default(Tag);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<Tag Id={Id} Level={Level}>";
	}
}
