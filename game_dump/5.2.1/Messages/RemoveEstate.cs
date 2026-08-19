using MsgPack;

namespace Messages;

public struct RemoveEstate
{
	public const uint TypeCode = 9518234u;

	public string EstateId;

	public static void Pack(Packer packer, RemoveEstate val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(9518234u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EstateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EstateId);
		}
	}

	public static RemoveEstate Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RemoveEstate result = default(RemoveEstate);
		result.EstateId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<RemoveEstate EstateId=" + EstateId + ">";
	}
}
