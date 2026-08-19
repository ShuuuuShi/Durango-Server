using MsgPack;

namespace Messages;

public struct BlueprintItem
{
	public const uint TypeCode = 19875324u;

	public string BlueprintId;

	public static void Pack(Packer packer, BlueprintItem val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(19875324u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.BlueprintId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BlueprintId);
		}
	}

	public static BlueprintItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BlueprintItem result = default(BlueprintItem);
		result.BlueprintId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<BlueprintItem BlueprintId=" + BlueprintId + ">";
	}
}
