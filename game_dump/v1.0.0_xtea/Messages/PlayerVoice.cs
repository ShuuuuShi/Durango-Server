using MsgPack;

namespace Messages;

public struct PlayerVoice
{
	public const uint TypeCode = 1017u;

	public ulong PlayerId;

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
		packer.Pack(val.PlayerId);
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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerVoice result = default(PlayerVoice);
		result.PlayerId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.VoiceData = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerVoice PlayerId={PlayerId} VoiceData={VoiceData}>";
	}
}
