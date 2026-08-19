using MsgPack;

namespace Messages;

public struct EmotionPurchaseContent
{
	public const uint TypeCode = 71294575u;

	public string Emotion;

	public static void Pack(Packer packer, EmotionPurchaseContent val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(71294575u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.Emotion == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Emotion);
		}
	}

	public static EmotionPurchaseContent Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		EmotionPurchaseContent result = default(EmotionPurchaseContent);
		result.Emotion = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<EmotionPurchaseContent Emotion={Emotion}>";
	}
}
