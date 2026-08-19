using MsgPack;

namespace Messages;

public struct ChangePolicy
{
	public const uint TypeCode = 606u;

	public string Policy;

	public static void Pack(Packer packer, ChangePolicy val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(606u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Policy == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Policy);
		}
	}

	public static ChangePolicy Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ChangePolicy result = default(ChangePolicy);
		result.Policy = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ChangePolicy Policy={Policy}>";
	}
}
