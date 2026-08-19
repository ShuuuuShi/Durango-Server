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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Damage result = default(Damage);
		if (num < 0 || 7 < num)
		{
			result.Result = DamageResult.Invalid;
		}
		else
		{
			result.Result = (DamageResult)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Value = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		if (num2 < 0 || 7 < num2)
		{
			result.Part = BodyPart.Invalid;
		}
		else
		{
			result.Part = (BodyPart)num2;
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		if (num3 < 0 || 3 < num3)
		{
			result.Direction = DamageDirection.Invalid;
		}
		else
		{
			result.Direction = (DamageDirection)num3;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num4 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		if (num4 < 0 || 15 < num4)
		{
			result.AttackType = AttackType.Invalid;
		}
		else
		{
			result.AttackType = (AttackType)num4;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		int num5 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		result.Effects = (DamageEffects)(num5 & 0x7F);
		return result;
	}

	public override string ToString()
	{
		return $"<Damage Result={Result} Value={Value} Part={Part} Direction={Direction} AttackType={AttackType} Effects={Effects}>";
	}
}
