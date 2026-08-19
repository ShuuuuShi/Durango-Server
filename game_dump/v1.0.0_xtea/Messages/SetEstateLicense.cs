using MsgPack;

namespace Messages;

public struct SetEstateLicense
{
	public const uint TypeCode = 2420u;

	public ulong EstateId;

	public License License;

	public static void Pack(Packer packer, SetEstateLicense val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2420u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.EstateId);
		License.Pack(packer, val.License);
	}

	public static SetEstateLicense Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetEstateLicense result = default(SetEstateLicense);
		result.EstateId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		result.License = License.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<SetEstateLicense EstateId={EstateId} License={License}>";
	}
}
