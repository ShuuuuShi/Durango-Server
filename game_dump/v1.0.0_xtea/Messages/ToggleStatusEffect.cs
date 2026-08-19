using MsgPack;

namespace Messages;

public struct ToggleStatusEffect
{
	public const uint TypeCode = 2081u;

	public string Id;

	public bool Toggle;

	public static void Pack(Packer packer, ToggleStatusEffect val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2081u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.Toggle);
	}

	public static ToggleStatusEffect Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ToggleStatusEffect result = default(ToggleStatusEffect);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Toggle = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ToggleStatusEffect Id={Id} Toggle={Toggle}>";
	}
}
