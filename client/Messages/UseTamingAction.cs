using MsgPack;

namespace Messages;

public struct UseTamingAction
{
	public const uint TypeCode = 1900u;

	public string EntityId;

	public string ToolItemId;

	public static void Pack(Packer packer, UseTamingAction val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1900u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.ToolItemId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ToolItemId);
		}
	}

	public static UseTamingAction Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		UseTamingAction result = default(UseTamingAction);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ToolItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<UseTamingAction EntityId={EntityId} ToolItemId={ToolItemId}>";
	}
}
