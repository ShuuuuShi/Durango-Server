using MsgPack;

namespace Messages;

public struct DrawLava
{
	public const uint TypeCode = 13498u;

	public string ToolItemId;

	public static void Pack(Packer packer, DrawLava val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(13498u);
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

	public static DrawLava Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DrawLava result = default(DrawLava);
		result.ToolItemId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<DrawLava ToolItemId={ToolItemId}>";
	}
}
