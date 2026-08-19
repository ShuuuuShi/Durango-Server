using MsgPack;

namespace Messages;

public struct PickMilestoneAgain
{
	public const uint TypeCode = 800014u;

	public string PetId;

	public bool WithVoucher;

	public static void Pack(Packer packer, PickMilestoneAgain val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(800014u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PetId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PetId);
		}
		packer.Pack(val.WithVoucher);
	}

	public static PickMilestoneAgain Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PickMilestoneAgain result = default(PickMilestoneAgain);
		result.PetId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.WithVoucher = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<PickMilestoneAgain PetId={PetId} WithVoucher={WithVoucher}>";
	}
}
