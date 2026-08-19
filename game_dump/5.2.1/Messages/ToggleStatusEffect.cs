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
		unpacker.Read();
		ToggleStatusEffect result = default(ToggleStatusEffect);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Toggle = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ToggleStatusEffect Id={Id} Toggle={Toggle}>";
	}
}
