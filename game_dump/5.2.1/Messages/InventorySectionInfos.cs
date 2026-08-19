using MsgPack;

namespace Messages;

public struct InventorySectionInfos
{
	public string SectionName;

	public int UsedSize;

	public int MaxSize;

	public static void Pack(Packer packer, InventorySectionInfos val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.SectionName == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SectionName);
		}
		packer.Pack(val.UsedSize);
		packer.Pack(val.MaxSize);
	}

	public static InventorySectionInfos Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InventorySectionInfos result = default(InventorySectionInfos);
		result.SectionName = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.UsedSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MaxSize = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<InventorySectionInfos SectionName={SectionName} UsedSize={UsedSize} MaxSize={MaxSize}>";
	}
}
