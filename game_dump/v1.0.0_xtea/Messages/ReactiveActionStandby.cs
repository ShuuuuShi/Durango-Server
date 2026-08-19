using MsgPack;

namespace Messages;

public struct ReactiveActionStandby
{
	public const uint TypeCode = 604u;

	public string ActionSetId;

	public string ActionId;

	public double Since;

	public double Until;

	public double CooldownUntil;

	public static void Pack(Packer packer, ReactiveActionStandby val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(604u);
		}
		else
		{
			packer.PackArrayHeader(5);
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
		packer.Pack(val.Since);
		packer.Pack(val.Until);
		packer.Pack(val.CooldownUntil);
	}

	public static ReactiveActionStandby Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ReactiveActionStandby result = default(ReactiveActionStandby);
		result.ActionSetId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ActionId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Since = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Until = ((MessagePackObject)(ref lastReadData4)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.CooldownUntil = ((MessagePackObject)(ref lastReadData5)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ReactiveActionStandby ActionSetId={ActionSetId} ActionId={ActionId} Since={Since} Until={Until} CooldownUntil={CooldownUntil}>";
	}
}
