using MsgPack;

namespace Messages;

public struct ArtifactStats
{
	public ArtifactComfortStat Comfort;

	public ArtifactAntibacterialStat Antibacterial;

	public static void Pack(Packer packer, ArtifactStats val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		ArtifactComfortStat.Pack(packer, val.Comfort);
		ArtifactAntibacterialStat.Pack(packer, val.Antibacterial);
	}

	public static ArtifactStats Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactStats result = default(ArtifactStats);
		result.Comfort = ArtifactComfortStat.Unpack(unpacker);
		unpacker.Read();
		result.Antibacterial = ArtifactAntibacterialStat.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactStats Comfort={Comfort} Antibacterial={Antibacterial}>";
	}
}
