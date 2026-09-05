using MsgPack;

namespace Messages;

public struct GetDiscoveryInfo
{
	public const uint TypeCode = 5000u;

	public string TemplateId;

	public static void Pack(Packer packer, GetDiscoveryInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5000u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TemplateId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TemplateId);
		}
	}

	public static GetDiscoveryInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetDiscoveryInfo result = default(GetDiscoveryInfo);
		result.TemplateId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<GetDiscoveryInfo TemplateId={TemplateId}>";
	}
}
