using MsgPack;

namespace Messages;

public struct MergeBattleEnded
{
	public const uint TypeCode = 980123u;

	public string EntityId;

	public double EventAt;

	public static void Pack(Packer packer, MergeBattleEnded val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(980123u);
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

	public static MergeBattleEnded Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MergeBattleEnded result = default(MergeBattleEnded);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<MergeBattleEnded EntityId={EntityId} EventAt={EventAt}>";
	}
}
