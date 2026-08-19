using MsgPack;

namespace Messages;

public struct BattleBegun
{
	public const uint TypeCode = 3278u;

	public string EntityId;

	public double EventAt;

	public string EnemyId;

	public bool StartDamaged;

	public static void Pack(Packer packer, BattleBegun val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3278u);
		}
		else
		{
			packer.PackArrayHeader(4);
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
		if (val.EnemyId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EnemyId);
		}
		packer.Pack(val.StartDamaged);
	}

	public static BattleBegun Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		BattleBegun result = default(BattleBegun);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.EnemyId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.StartDamaged = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<BattleBegun EntityId={EntityId} EventAt={EventAt} EnemyId={EnemyId} StartDamaged={StartDamaged}>";
	}
}
