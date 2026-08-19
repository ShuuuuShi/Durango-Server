using MsgPack;

namespace Messages;

public struct Tool_Collect
{
	public const uint TypeCode = 330u;

	public string CollectibleId;

	public string GeneratorId;

	public static void Pack(Packer packer, Tool_Collect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(330u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.CollectibleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CollectibleId);
		}
		if (val.GeneratorId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.GeneratorId);
		}
	}

	public static Tool_Collect Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Tool_Collect result = default(Tool_Collect);
		result.CollectibleId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.GeneratorId = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<Tool_Collect CollectibleId={CollectibleId} GeneratorId={GeneratorId}>";
	}
}
