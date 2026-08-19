using MsgPack;

namespace Messages;

public struct RepairArtifactResult
{
	public const uint TypeCode = 2056u;

	public string Text;

	public static void Pack(Packer packer, RepairArtifactResult val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2056u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackString(val.Text);
	}

	public static RepairArtifactResult Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RepairArtifactResult result = default(RepairArtifactResult);
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RepairArtifactResult Text={Text}>";
	}
}
