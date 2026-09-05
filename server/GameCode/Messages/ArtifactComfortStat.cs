using MsgPack;

namespace Messages;

public struct ArtifactComfortStat
{
	public int Factor;

	public int Complexity;

	public string Description;

	public static void Pack(Packer packer, ArtifactComfortStat val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.Factor);
		packer.Pack(val.Complexity);
		packer.PackString(val.Description);
	}

	public static ArtifactComfortStat Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactComfortStat result = default(ArtifactComfortStat);
		result.Factor = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Complexity = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Description = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactComfortStat Factor={Factor} Complexity={Complexity} Description={Description}>";
	}
}
