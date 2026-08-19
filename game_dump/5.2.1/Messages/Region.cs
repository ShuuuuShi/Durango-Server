using MsgPack;
using Shared.Region;

namespace Messages;

public struct Region
{
	public const uint TypeCode = 2041u;

	public string Id;

	public string TerrainId;

	public string TemplateId;

	public Role Role;

	public string Name;

	public double CreatedAt;

	public static void Pack(Packer packer, Region val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(2041u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		if (val.TerrainId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TerrainId);
		}
		if (val.TemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TemplateId);
		}
		packer.Pack((int)val.Role);
		if (val.Name == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.Name);
		}
		packer.Pack(val.CreatedAt);
	}

	public static Region Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Region result = default(Region);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TerrainId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TemplateId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 9 < num)
		{
			result.Role = Role.Invalid;
		}
		else
		{
			result.Role = (Role)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Name = null;
		}
		else
		{
			string name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.Name = name;
		}
		unpacker.Read();
		result.CreatedAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Region Id={Id} TerrainId={TerrainId} TemplateId={TemplateId} Role={Role} Name={Name} CreatedAt={CreatedAt}>";
	}
}
