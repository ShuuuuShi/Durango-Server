using MsgPack;

namespace Messages;

public struct PlayerVoice
{
	public const uint TypeCode = 1017u;

	public string PlayerId;

	public string VoiceData;

	public static void Pack(Packer packer, PlayerVoice val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1017u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PlayerId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PlayerId);
		}
		if (val.VoiceData == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VoiceData);
		}
	}

	public static PlayerVoice Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerVoice result = default(PlayerVoice);
		result.PlayerId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.VoiceData = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<PlayerVoice PlayerId=" + PlayerId + " VoiceData=" + VoiceData + ">";
	}
}
