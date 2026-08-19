using MsgPack;

namespace Messages;

public struct MilestoneInfo
{
	public int Level;

	public int MilestoneTableId;

	public string TagId;

	public bool Acquired;

	public static void Pack(Packer packer, MilestoneInfo val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.Level);
		packer.Pack(val.MilestoneTableId);
		if (val.TagId == null)
		{
			packer.PackNull();
		}
		else if (val.TagId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TagId);
		}
		packer.Pack(val.Acquired);
	}

	public static MilestoneInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MilestoneInfo result = default(MilestoneInfo);
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MilestoneTableId = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TagId = null;
		}
		else
		{
			string tagId = unpacker.LastReadData.AsString();
			result.TagId = tagId;
		}
		unpacker.Read();
		result.Acquired = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<MilestoneInfo Level={Level} MilestoneTableId={MilestoneTableId} TagId={TagId} Acquired={Acquired}>";
	}
}
