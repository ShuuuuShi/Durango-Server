using MsgPack;
using Shared.Region;

namespace Messages;

public struct RecommendRegion
{
	public const uint TypeCode = 3001u;

	public string EntityId;

	public Point2 Tile;

	public Role? Role;

	public string TemplateId;

	public static void Pack(Packer packer, RecommendRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3001u);
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
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (!val.Role.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack((int)val.Role.Value);
		}
		if (val.TemplateId == null)
		{
			packer.PackNull();
		}
		else if (val.TemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TemplateId);
		}
	}

	public static RecommendRegion Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RecommendRegion result = default(RecommendRegion);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Role = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			Role value = ((num >= 0 && 9 >= num) ? ((Role)num) : Shared.Region.Role.Invalid);
			result.Role = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.TemplateId = null;
		}
		else
		{
			string templateId = unpacker.LastReadData.AsString();
			result.TemplateId = templateId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<RecommendRegion EntityId={EntityId} Tile={Tile} Role={Role} TemplateId={TemplateId}>";
	}
}
