using MsgPack;

namespace Messages;

public struct Party
{
	public const uint TypeCode = 20002u;

	public string Id;

	public PartyInfo? Info;

	public static void Pack(Packer packer, Party val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(20002u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Id == null)
		{
			packer.PackNull();
		}
		else if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (!val.Info.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			PartyInfo.Pack(packer, val.Info.Value);
		}
	}

	public static Party Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Party result = default(Party);
		if (unpacker.LastReadData.IsNil)
		{
			result.Id = null;
		}
		else
		{
			string id = unpacker.LastReadData.AsString();
			result.Id = id;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Info = null;
		}
		else
		{
			PartyInfo value = PartyInfo.Unpack(unpacker);
			result.Info = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Party Id={Id} Info={Info}>";
	}
}
