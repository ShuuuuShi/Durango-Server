using MsgPack;

namespace Messages;

public struct RecommendPersonalRegion
{
	public const uint TypeCode = 3002u;

	public string TemplateId;

	public static void Pack(Packer packer, RecommendPersonalRegion val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3002u);
		}
		else
		{
			packer.PackArrayHeader(1);
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

	public static RecommendPersonalRegion Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		RecommendPersonalRegion result = default(RecommendPersonalRegion);
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
		return $"<RecommendPersonalRegion TemplateId={TemplateId}>";
	}
}
