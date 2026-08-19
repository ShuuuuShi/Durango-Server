using MsgPack;
using Shared.Region;

namespace Messages;

public struct Region
{
	public const uint TypeCode = 2041u;

	public ulong Id;

	public ulong TerrainId;

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
		packer.Pack(val.Id);
		packer.Pack(val.TerrainId);
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Region result = default(Region);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.TerrainId = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.TemplateId = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		if (num < 0 || 5 < num)
		{
			result.Role = Role.Invalid;
		}
		else
		{
			result.Role = (Role)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData5)).IsNil)
		{
			result.Name = null;
		}
		else
		{
			string name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.Name = name;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.CreatedAt = ((MessagePackObject)(ref lastReadData6)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Region Id={Id} TerrainId={TerrainId} TemplateId={TemplateId} Role={Role} Name={Name} CreatedAt={CreatedAt}>";
	}
}
