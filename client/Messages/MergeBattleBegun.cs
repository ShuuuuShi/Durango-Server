using MsgPack;

namespace Messages;

public struct MergeBattleBegun
{
	public const uint TypeCode = 123798u;

	public string EntityId;

	public double EventAt;

	public static void Pack(Packer packer, MergeBattleBegun val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(123798u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EventAt);
	}

	public static MergeBattleBegun Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MergeBattleBegun result = default(MergeBattleBegun);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<MergeBattleBegun EntityId={EntityId} EventAt={EventAt}>";
	}
}
