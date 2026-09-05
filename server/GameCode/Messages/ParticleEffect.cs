using MsgPack;

namespace Messages;

public struct ParticleEffect
{
	public const uint TypeCode = 503u;

	public string EntityId;

	public string Path;

	public string Bone;

	public bool Follow;

	public static void Pack(Packer packer, ParticleEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(503u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Path == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Path);
		}
		if (val.Bone == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Bone);
		}
		packer.Pack(val.Follow);
	}

	public static ParticleEffect Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ParticleEffect result = default(ParticleEffect);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Path = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Bone = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Follow = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ParticleEffect EntityId={EntityId} Path={Path} Bone={Bone} Follow={Follow}>";
	}
}
