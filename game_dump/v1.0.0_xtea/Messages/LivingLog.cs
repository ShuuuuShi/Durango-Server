using MsgPack;

namespace Messages;

public struct LivingLog
{
	public const uint TypeCode = 2059u;

	public string Log;

	public static void Pack(Packer packer, LivingLog val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2059u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Log == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Log);
		}
	}

	public static LivingLog Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		LivingLog result = default(LivingLog);
		result.Log = ((MessagePackObject)(ref lastReadData)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<LivingLog Log={Log}>";
	}
}
