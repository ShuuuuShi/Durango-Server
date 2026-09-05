using MsgPack;

namespace Messages;

public struct Display
{
	public const uint TypeCode = 102u;

	public string EntityId;

	public static void Pack(Packer packer, Display val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(102u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
	}

	public static Display Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Display result = default(Display);
		result.EntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Display EntityId={EntityId}>";
	}
}
