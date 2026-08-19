using MsgPack;

namespace Messages;

public struct GetArtifactBlueprints
{
	public const uint TypeCode = 2012u;

	public static void Pack(Packer packer, GetArtifactBlueprints val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(2012u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetArtifactBlueprints Unpack(Unpacker unpacker)
	{
		GetArtifactBlueprints result = default(GetArtifactBlueprints);
		return result;
	}

	public override string ToString()
	{
		return "<GetArtifactBlueprints>";
	}
}
