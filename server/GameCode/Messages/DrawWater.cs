using MsgPack;

namespace Messages;

public struct DrawWater
{
	public const uint TypeCode = 3493u;

	public string ToolItemId;

	public static void Pack(Packer packer, DrawWater val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3493u);
		}
		else
		{
			packer.PackArrayHeader(1);
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

	public static DrawWater Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DrawWater result = default(DrawWater);
		result.ToolItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<DrawWater ToolItemId={ToolItemId}>";
	}
}
