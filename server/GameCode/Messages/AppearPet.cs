using MsgPack;

namespace Messages;

public struct AppearPet
{
	public const uint TypeCode = 94u;

	public string EntityId;

	public ushort EntityType;

	public bool IsAlive;

	public Move? Move;

	public Survival Survival;

	public Pet? PetData;

	public static void Pack(Packer packer, AppearPet val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(94u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EntityType);
		packer.Pack(val.IsAlive);
		if (!val.Move.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Move.Pack(packer, val.Move.Value);
		}
		Survival.Pack(packer, val.Survival);
		if (!val.PetData.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Pet.Pack(packer, val.PetData.Value);
		}
	}

	public static AppearPet Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AppearPet result = default(AppearPet);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsAlive = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Move = null;
		}
		else
		{
			Move value = Messages.Move.Unpack(unpacker);
			result.Move = value;
		}
		unpacker.Read();
		result.Survival = Survival.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PetData = null;
		}
		else
		{
			Pet value2 = Pet.Unpack(unpacker);
			result.PetData = value2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AppearPet EntityId={EntityId} EntityType={EntityType} IsAlive={IsAlive} Move={Move} Survival={Survival} PetData={PetData}>";
	}
}
