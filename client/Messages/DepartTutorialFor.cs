using MsgPack;

namespace Messages;

public struct DepartTutorialFor
{
	public const uint TypeCode = 2307u;

	public string TargetRegionId;

	public int EntryPointOffset;

	public static void Pack(Packer packer, DepartTutorialFor val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2307u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.TargetRegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetRegionId);
		}
		packer.Pack(val.EntryPointOffset);
	}

	public static DepartTutorialFor Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DepartTutorialFor result = default(DepartTutorialFor);
		result.TargetRegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntryPointOffset = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<DepartTutorialFor TargetRegionId={TargetRegionId} EntryPointOffset={EntryPointOffset}>";
	}
}
