using MsgPack;

namespace Messages;

public struct DepartTutorialReady
{
	public const uint TypeCode = 2305u;

	public string TargetRegionId;

	public int EntryPointOffset;

	public static void Pack(Packer packer, DepartTutorialReady val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2305u);
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

	public static DepartTutorialReady Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		DepartTutorialReady result = default(DepartTutorialReady);
		result.TargetRegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntryPointOffset = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<DepartTutorialReady TargetRegionId={TargetRegionId} EntryPointOffset={EntryPointOffset}>";
	}
}
