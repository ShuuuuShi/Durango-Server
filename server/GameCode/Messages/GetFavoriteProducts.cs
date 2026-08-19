using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GetFavoriteProducts
{
	public const uint TypeCode = 179238u;

	public static void Pack(Packer packer, GetFavoriteProducts val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(1);
			packer.Pack(179238u);
		}
		else
		{
			packer.PackArrayHeader(0);
		}
	}

	public static GetFavoriteProducts Unpack(Unpacker unpacker)
	{
		GetFavoriteProducts result = default(GetFavoriteProducts);
		return result;
	}

	public override string ToString()
	{
		return "<GetFavoriteProducts>";
	}
}
