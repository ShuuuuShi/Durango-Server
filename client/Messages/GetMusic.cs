using MsgPack;

namespace Messages;

public struct GetMusic
{
	public const uint TypeCode = 47852452u;

	public string EntityId;

	public int Slot;

	public static void Pack(Packer packer, GetMusic val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(47852452u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackNull();
		}
		else if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.Slot);
	}

	public static GetMusic Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetMusic result = default(GetMusic);
		if (unpacker.LastReadData.IsNil)
		{
			result.EntityId = null;
		}
		else
		{
			string entityId = unpacker.LastReadData.AsString();
			result.EntityId = entityId;
		}
		unpacker.Read();
		result.Slot = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GetMusic EntityId={EntityId} Slot={Slot}>";
	}
}
