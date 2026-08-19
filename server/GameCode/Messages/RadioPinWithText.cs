using MsgPack;

namespace Messages;

public struct RadioPinWithText
{
	public const uint TypeCode = 2609u;

	public string RegionId;

	public string RegionName;

	public Point2 Tile;

	public string Text;

	public static void Pack(Packer packer, RadioPinWithText val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(2609u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.RegionId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RegionId);
		}
		if (val.RegionName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.RegionName);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.PackString(val.Text);
	}

	public static RadioPinWithText Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RadioPinWithText result = default(RadioPinWithText);
		result.RegionId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RegionName = null;
		}
		else
		{
			string regionName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.RegionName = regionName;
		}
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.Text = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<RadioPinWithText RegionId={RegionId} RegionName={RegionName} Tile={Tile} Text={Text}>";
	}
}
