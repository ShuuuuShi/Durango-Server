using MsgPack;

namespace Messages;

public struct InteriorSetEffectChanged
{
	public const uint TypeCode = 739812u;

	public string ModularEntityId;

	public ArtifactState ModularArtifactState;

	public static void Pack(Packer packer, InteriorSetEffectChanged val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(739812u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.ModularEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ModularEntityId);
		}
		ArtifactState.Pack(packer, val.ModularArtifactState);
	}

	public static InteriorSetEffectChanged Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InteriorSetEffectChanged result = default(InteriorSetEffectChanged);
		result.ModularEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.ModularArtifactState = ArtifactState.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<InteriorSetEffectChanged ModularEntityId={ModularEntityId} ModularArtifactState={ModularArtifactState}>";
	}
}
