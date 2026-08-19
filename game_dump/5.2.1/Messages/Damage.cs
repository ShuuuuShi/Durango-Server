using MsgPack;
using Shared.Battle;

namespace Messages;

public struct Damage
{
	public DamageResult Result;

	public int Value;

	public BodyPart Part;

	public DamageDirection Direction;

	public AttackType AttackType;

	public DamageEffects Effects;

	public static void Pack(Packer packer, Damage val, bool hint = false)
	{
		packer.PackArrayHeader(6);
		packer.Pack((int)val.Result);
		packer.Pack(val.Value);
		packer.Pack((int)val.Part);
		packer.Pack((int)val.Direction);
		packer.Pack((int)val.AttackType);
		packer.Pack((int)val.Effects);
	}

	public static Damage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Damage result = default(Damage);
		if (num < 0 || 8 < num)
		{
			result.Result = DamageResult.Invalid;
		}
		else
		{
			result.Result = (DamageResult)num;
		}
		unpacker.Read();
		result.Value = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		if (num2 < 0 || 7 < num2)
		{
			result.Part = BodyPart.Invalid;
		}
		else
		{
			result.Part = (BodyPart)num2;
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		if (num3 < 0 || 3 < num3)
		{
			result.Direction = DamageDirection.Invalid;
		}
		else
		{
			result.Direction = (DamageDirection)num3;
		}
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		if (num4 < 0 || 15 < num4)
		{
			result.AttackType = AttackType.Invalid;
		}
		else
		{
			result.AttackType = (AttackType)num4;
		}
		unpacker.Read();
		int num5 = unpacker.LastReadData.AsInt32();
		result.Effects = (DamageEffects)(num5 & 0x5F);
		return result;
	}

	public override string ToString()
	{
		return $"<Damage Result={Result} Value={Value} Part={Part} Direction={Direction} AttackType={AttackType} Effects={Effects}>";
	}
}
