using MsgPack;

namespace Messages;

public struct ArtifactAntibacterialStat
{
	public int Factor;

	public int Complexity;

	public string Description;

	public static void Pack(Packer packer, ArtifactAntibacterialStat val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.Factor);
		packer.Pack(val.Complexity);
		packer.PackString(val.Description);
	}

	public static ArtifactAntibacterialStat Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactAntibacterialStat result = default(ArtifactAntibacterialStat);
		result.Factor = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Complexity = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Description = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactAntibacterialStat Factor={Factor} Complexity={Complexity} Description={Description}>";
	}
}
