using MsgPack;

namespace Messages;

public struct ResearchState
{
	public const uint TypeCode = 3701u;

	public string ResearchId;

	public double ResearchStartAt;

	public static void Pack(Packer packer, ResearchState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3701u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ResearchId == null)
		{
			packer.PackNull();
		}
		else if (val.ResearchId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ResearchId);
		}
		packer.Pack(val.ResearchStartAt);
	}

	public static ResearchState Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ResearchState result = default(ResearchState);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.ResearchId = null;
		}
		else
		{
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			string researchId = ((MessagePackObject)(ref lastReadData2)).AsString();
			result.ResearchId = researchId;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.ResearchStartAt = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<ResearchState ResearchId={ResearchId} ResearchStartAt={ResearchStartAt}>";
	}
}
