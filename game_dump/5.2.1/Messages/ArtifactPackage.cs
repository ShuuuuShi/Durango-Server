using MsgPack;
using Shared.Item;

namespace Messages;

public struct ArtifactPackage
{
	public const uint TypeCode = 3694u;

	public int Size;

	public PackageStatus Status;

	public PackedArtifact[] Artifacts;

	public static void Pack(Packer packer, ArtifactPackage val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3694u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.Size);
		packer.Pack((int)val.Status);
		if (val.Artifacts == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Artifacts.Length);
		for (int i = 0; i < val.Artifacts.Length; i++)
		{
			PackedArtifact.Pack(packer, val.Artifacts[i]);
		}
	}

	public static ArtifactPackage Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactPackage result = default(ArtifactPackage);
		result.Size = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 2 < num)
		{
			result.Status = PackageStatus.Invalid;
		}
		else
		{
			result.Status = (PackageStatus)num;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Artifacts = new PackedArtifact[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ref PackedArtifact reference = ref result.Artifacts[i];
			reference = PackedArtifact.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactPackage Size={Size} Status={Status} Artifacts={Artifacts}>";
	}
}
