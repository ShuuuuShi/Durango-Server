using MsgPack;

namespace Messages;

public struct ExpectedCropBooster
{
	public const uint TypeCode = 37124u;

	public string CropBooster;

	public int BoosterLevel;

	public static void Pack(Packer packer, ExpectedCropBooster val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(37124u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.CropBooster == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CropBooster);
		}
		packer.Pack(val.BoosterLevel);
	}

	public static ExpectedCropBooster Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ExpectedCropBooster result = default(ExpectedCropBooster);
		result.CropBooster = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.BoosterLevel = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ExpectedCropBooster CropBooster={CropBooster} BoosterLevel={BoosterLevel}>";
	}
}
