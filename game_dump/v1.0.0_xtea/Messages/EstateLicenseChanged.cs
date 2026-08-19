using MsgPack;

namespace Messages;

public struct EstateLicenseChanged
{
	public const uint TypeCode = 2430u;

	public ulong? EstateId;

	public ulong? ClanId;

	public static void Pack(Packer packer, EstateLicenseChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2430u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (!val.EstateId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.EstateId.Value);
		}
		if (!val.ClanId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ClanId.Value);
		}
	}

	public static EstateLicenseChanged Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		EstateLicenseChanged result = default(EstateLicenseChanged);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.EstateId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
			result.EstateId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.ClanId = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
			result.ClanId = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EstateLicenseChanged EstateId={EstateId} ClanId={ClanId}>";
	}
}
