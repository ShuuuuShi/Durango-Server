using System.Runtime.InteropServices;
using MsgPack;

namespace Messages;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct OccupyGardenGrid
{
	public static void Pack(Packer packer, OccupyGardenGrid val, bool hint = false)
	{
		packer.PackArrayHeader(0);
	}

	public static OccupyGardenGrid Unpack(Unpacker unpacker)
	{
		OccupyGardenGrid result = default(OccupyGardenGrid);
		return result;
	}

	public override string ToString()
	{
		return "<OccupyGardenGrid>";
	}
}
