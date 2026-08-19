using MsgPack;

namespace Messages;

public struct NomadInfo
{
	public const uint TypeCode = 100001u;

	public bool IsNomad;

	public int NomadCount;

	public static void Pack(Packer packer, NomadInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(100001u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.IsNomad);
		packer.Pack(val.NomadCount);
	}

	public static NomadInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		NomadInfo result = default(NomadInfo);
		result.IsNomad = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.NomadCount = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<NomadInfo IsNomad={IsNomad} NomadCount={NomadCount}>";
	}
}
