using MsgPack;
using Shared.Ability;

namespace Messages;

public struct ExpGained
{
	public const uint TypeCode = 3707u;

	public string EntityId;

	public int Exp;

	public int BonusExp;

	public Derived? ResistanceType;

	public int ResistanceExp;

	public static void Pack(Packer packer, ExpGained val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3707u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.Exp);
		packer.Pack(val.BonusExp);
		if (!val.ResistanceType.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.ResistanceType.Value);
		}
		packer.Pack(val.ResistanceExp);
	}

	public static ExpGained Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ExpGained result = default(ExpGained);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Exp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.BonusExp = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ResistanceType = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Derived value = ((num >= 0 && 322 >= num) ? ((Derived)num) : Derived.Invalid);
			result.ResistanceType = value;
		}
		unpacker.Read();
		result.ResistanceExp = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ExpGained EntityId={EntityId} Exp={Exp} BonusExp={BonusExp} ResistanceType={ResistanceType} ResistanceExp={ResistanceExp}>";
	}
}
