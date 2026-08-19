using MsgPack;

namespace Messages;

public struct ActionBegun
{
	public const uint TypeCode = 609u;

	public string ActionSetId;

	public double Since;

	public double Until;

	public double CooldownUntil;

	public static void Pack(Packer packer, ActionBegun val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(609u);
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
		packer.Pack(val.Since);
		packer.Pack(val.Until);
		packer.Pack(val.CooldownUntil);
	}

	public static ActionBegun Unpack(Unpacker unpacker)
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
		ActionBegun result = default(ActionBegun);
		result.ActionSetId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Since = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Until = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.CooldownUntil = ((MessagePackObject)(ref lastReadData4)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ActionBegun ActionSetId={ActionSetId} Since={Since} Until={Until} CooldownUntil={CooldownUntil}>";
	}
}
