using MsgPack;

namespace Messages;

public struct ArtifactBuilt
{
	public const uint TypeCode = 2093u;

	public string EntityId;

	public string BuilderId;

	public static void Pack(Packer packer, ArtifactBuilt val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2093u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.BuilderId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BuilderId);
		}
	}

	public static ArtifactBuilt Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactBuilt result = default(ArtifactBuilt);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.BuilderId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<ArtifactBuilt EntityId=" + EntityId + " BuilderId=" + BuilderId + ">";
	}
}
