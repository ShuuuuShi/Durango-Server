using MsgPack;

namespace Messages;

public struct Damaged
{
	public const uint TypeCode = 12u;

	public string VictimId;

	public string AttackerId;

	public Damage Damage;

	public double EventAt;

	public static void Pack(Packer packer, Damaged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(12u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.VictimId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VictimId);
		}
		if (val.AttackerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AttackerId);
		}
		Damage.Pack(packer, val.Damage);
		packer.Pack(val.EventAt);
	}

	public static Damaged Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Damaged result = default(Damaged);
		result.VictimId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.AttackerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Damage = Damage.Unpack(unpacker);
		unpacker.Read();
		result.EventAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Damaged VictimId={VictimId} AttackerId={AttackerId} Damage={Damage} EventAt={EventAt}>";
	}
}
