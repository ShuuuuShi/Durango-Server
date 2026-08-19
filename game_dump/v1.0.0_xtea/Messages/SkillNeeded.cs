using MsgPack;

namespace Messages;

public struct SkillNeeded
{
	public const uint TypeCode = 2449u;

	public string SkillId;

	public int Level;

	public string SubId;

	public static void Pack(Packer packer, SkillNeeded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2449u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.SkillId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkillId);
		}
		packer.Pack(val.Level);
		if (val.SubId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SubId);
		}
	}

	public static SkillNeeded Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SkillNeeded result = default(SkillNeeded);
		result.SkillId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.SubId = ((MessagePackObject)(ref lastReadData3)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<SkillNeeded SkillId={SkillId} Level={Level} SubId={SubId}>";
	}
}
