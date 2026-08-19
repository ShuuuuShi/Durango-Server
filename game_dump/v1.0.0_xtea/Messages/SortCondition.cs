using MsgPack;

namespace Messages;

public struct SortCondition
{
	public string Field;

	public bool Ascending;

	public static void Pack(Packer packer, SortCondition val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		if (val.Field == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Field);
		}
		packer.Pack(val.Ascending);
	}

	public static SortCondition Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SortCondition result = default(SortCondition);
		result.Field = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Ascending = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<SortCondition Field={Field} Ascending={Ascending}>";
	}
}
