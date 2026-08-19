using MsgPack;

namespace Messages;

public struct Reins
{
	public const uint TypeCode = 801u;

	public ushort PetEntityType;

	public ushort VehicleEntityType;

	public ushort Size;

	public Pet? Pet;

	public bool Domesticated;

	public float DomesticateDuration;

	public float DomesticateSuccessRate;

	public static void Pack(Packer packer, Reins val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(801u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		packer.Pack(val.PetEntityType);
		packer.Pack(val.VehicleEntityType);
		packer.Pack(val.Size);
		if (!val.Pet.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Pet.Pack(packer, val.Pet.Value);
		}
		packer.Pack(val.Domesticated);
		packer.Pack(val.DomesticateDuration);
		packer.Pack(val.DomesticateSuccessRate);
	}

	public static Reins Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Reins result = default(Reins);
		result.PetEntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.VehicleEntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.Size = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Pet = null;
		}
		else
		{
			Pet value = Messages.Pet.Unpack(unpacker);
			result.Pet = value;
		}
		unpacker.Read();
		result.Domesticated = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.DomesticateDuration = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.DomesticateSuccessRate = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Reins PetEntityType={PetEntityType} VehicleEntityType={VehicleEntityType} Size={Size} Pet={Pet} Domesticated={Domesticated} DomesticateDuration={DomesticateDuration} DomesticateSuccessRate={DomesticateSuccessRate}>";
	}
}
