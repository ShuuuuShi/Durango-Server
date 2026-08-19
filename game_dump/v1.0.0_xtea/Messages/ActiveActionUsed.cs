using MsgPack;

namespace Messages;

public struct ActiveActionUsed
{
	public const uint TypeCode = 603u;

	public string ActionSetId;

	public string ActionId;

	public double UsedAt;

	public double CooldownUntil;

	public static void Pack(Packer packer, ActiveActionUsed val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(603u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.ActionSetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ActionSetId);
		}
		if (val.ActionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ActionId);
		}
		packer.Pack(val.UsedAt);
		packer.Pack(val.CooldownUntil);
	}

	public static ActiveActionUsed Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ActiveActionUsed result = default(ActiveActionUsed);
		result.ActionSetId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ActionId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.UsedAt = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.CooldownUntil = ((MessagePackObject)(ref lastReadData4)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ActiveActionUsed ActionSetId={ActionSetId} ActionId={ActionId} UsedAt={UsedAt} CooldownUntil={CooldownUntil}>";
	}
}
