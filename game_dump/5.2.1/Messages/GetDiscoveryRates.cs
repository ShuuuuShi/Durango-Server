using MsgPack;

namespace Messages;

public struct GetDiscoveryRates
{
	public const uint TypeCode = 5003u;

	public string[] TemplateIds;

	public static void Pack(Packer packer, GetDiscoveryRates val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5003u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.TemplateIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.TemplateIds.Length);
		for (int i = 0; i < val.TemplateIds.Length; i++)
		{
			if (val.TemplateIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.TemplateIds[i]);
			}
		}
	}

	public static GetDiscoveryRates Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		GetDiscoveryRates result = default(GetDiscoveryRates);
		result.TemplateIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.TemplateIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		object[] templateIds = TemplateIds;
		return string.Format("<GetDiscoveryRates TemplateIds={0}>", templateIds);
	}
}
