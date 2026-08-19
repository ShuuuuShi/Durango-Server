using MsgPack;

namespace Messages;

public struct BattleScenario
{
	public const uint TypeCode = 798135u;

	public Move[] Moves;

	public Damaged[] Damages;

	public static void Pack(Packer packer, BattleScenario val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(798135u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Moves == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Moves.Length);
			for (int i = 0; i < val.Moves.Length; i++)
			{
				Move.Pack(packer, val.Moves[i]);
			}
		}
		if (val.Damages == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Damages.Length);
		for (int j = 0; j < val.Damages.Length; j++)
		{
			Damaged.Pack(packer, val.Damages[j]);
		}
	}

	public static BattleScenario Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		BattleScenario result = default(BattleScenario);
		result.Moves = new Move[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Move reference = ref result.Moves[i];
			reference = Move.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Damages = new Damaged[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Damaged reference2 = ref result.Damages[j];
			reference2 = Damaged.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<BattleScenario Moves={Moves} Damages={Damages}>";
	}
}
