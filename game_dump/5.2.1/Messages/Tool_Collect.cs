using MsgPack;

namespace Messages;

public struct Tool_Collect
{
	public const uint TypeCode = 330u;

	public string CollectibleId;

	public string GeneratorId;

	public static void Pack(Packer packer, Tool_Collect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(330u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.CollectibleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CollectibleId);
		}
		if (val.GeneratorId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.GeneratorId);
		}
	}

	public static Tool_Collect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Tool_Collect result = default(Tool_Collect);
		result.CollectibleId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.GeneratorId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<Tool_Collect CollectibleId=" + CollectibleId + " GeneratorId=" + GeneratorId + ">";
	}
}
