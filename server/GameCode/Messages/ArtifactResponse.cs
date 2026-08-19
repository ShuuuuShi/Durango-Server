using MsgPack;

namespace Messages;

public struct ArtifactResponse
{
	public const uint TypeCode = 310u;

	public string ArtifactId;

	public string Action;

	public bool Success;

	public static void Pack(Packer packer, ArtifactResponse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(310u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.ArtifactId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ArtifactId);
		}
		if (val.Action == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Action);
		}
		packer.Pack(val.Success);
	}

	public static ArtifactResponse Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactResponse result = default(ArtifactResponse);
		result.ArtifactId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Action = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Success = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactResponse ArtifactId={ArtifactId} Action={Action} Success={Success}>";
	}
}
