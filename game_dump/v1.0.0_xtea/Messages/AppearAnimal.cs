using MsgPack;

namespace Messages;

public struct AppearAnimal
{
	public const uint TypeCode = 91u;

	public ulong EntityId;

	public ushort EntityType;

	public AnimalDisplay Display;

	public int Level;

	public Move Move;

	public Survival Survival;

	public static void Pack(Packer packer, AppearAnimal val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(91u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.EntityType);
		AnimalDisplay.Pack(packer, val.Display);
		packer.Pack(val.Level);
		Move.Pack(packer, val.Move);
		Survival.Pack(packer, val.Survival);
	}

	public static AppearAnimal Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AppearAnimal result = default(AppearAnimal);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityType = ((MessagePackObject)(ref lastReadData2)).AsUInt16();
		unpacker.Read();
		result.Display = AnimalDisplay.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		result.Move = Move.Unpack(unpacker);
		unpacker.Read();
		result.Survival = Survival.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<AppearAnimal EntityId={EntityId} EntityType={EntityType} Display={Display} Level={Level} Move={Move} Survival={Survival}>";
	}
}
