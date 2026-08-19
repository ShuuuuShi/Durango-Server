using MsgPack;
using Shared.Etc;

namespace Messages;

public struct ArtifactPreview
{
	public const uint TypeCode = 2313u;

	public Point2 Size;

	public Rotation Rotation;

	public ArtifactDisplay Display;

	public bool IsModular;

	public static void Pack(Packer packer, ArtifactPreview val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2313u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Size.x);
		packer.Pack((ushort)val.Size.y);
		packer.Pack((int)val.Rotation);
		ArtifactDisplay.Pack(packer, val.Display);
		packer.Pack(val.IsModular);
	}

	public static ArtifactPreview Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		ArtifactPreview result2 = default(ArtifactPreview);
		result2.Size.x = result;
		unpacker.ReadUInt16(out result);
		result2.Size.y = result;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result2.Rotation = Rotation.Invalid;
		}
		else
		{
			result2.Rotation = (Rotation)num;
		}
		unpacker.Read();
		result2.Display = ArtifactDisplay.Unpack(unpacker);
		unpacker.Read();
		result2.IsModular = unpacker.LastReadData.AsBoolean();
		return result2;
	}

	public override string ToString()
	{
		return $"<ArtifactPreview Size={Size} Rotation={Rotation} Display={Display} IsModular={IsModular}>";
	}
}
