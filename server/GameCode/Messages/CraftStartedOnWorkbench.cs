using MsgPack;

namespace Messages;

public struct CraftStartedOnWorkbench
{
	public const uint TypeCode = 66u;

	public string EntityId;

	public float Duration;

	public static void Pack(Packer packer, CraftStartedOnWorkbench val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(66u);
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
		packer.Pack(val.Duration);
	}

	public static CraftStartedOnWorkbench Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CraftStartedOnWorkbench result = default(CraftStartedOnWorkbench);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<CraftStartedOnWorkbench EntityId={EntityId} Duration={Duration}>";
	}
}
