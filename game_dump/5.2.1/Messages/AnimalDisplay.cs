using MsgPack;

namespace Messages;

public struct AnimalDisplay
{
	public const uint TypeCode = 2432u;

	public string EntityId;

	public float BaseScale;

	public CollectibleDisplay? CollectibleDisplay;

	public static void Pack(Packer packer, AnimalDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2432u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.BaseScale);
		if (!val.CollectibleDisplay.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.CollectibleDisplay.Pack(packer, val.CollectibleDisplay.Value);
		}
	}

	public static AnimalDisplay Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AnimalDisplay result = default(AnimalDisplay);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.BaseScale = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CollectibleDisplay = null;
		}
		else
		{
			CollectibleDisplay value = Messages.CollectibleDisplay.Unpack(unpacker);
			result.CollectibleDisplay = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AnimalDisplay EntityId={EntityId} BaseScale={BaseScale} CollectibleDisplay={CollectibleDisplay}>";
	}
}
