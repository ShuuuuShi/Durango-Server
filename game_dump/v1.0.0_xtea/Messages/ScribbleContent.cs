using MsgPack;
using Shared.MessageBoard;

namespace Messages;

public struct ScribbleContent
{
	public Drawing Type;

	public byte[] Data;

	public ulong Scribbler;

	public static void Pack(Packer packer, ScribbleContent val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack((int)val.Type);
		if (val.Data == null)
		{
			packer.PackBinary(new byte[0]);
		}
		else
		{
			packer.PackBinary(val.Data);
		}
		packer.Pack(val.Scribbler);
	}

	public static ScribbleContent Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		ScribbleContent result = default(ScribbleContent);
		if (num < 0 || 1 < num)
		{
			result.Type = Drawing.Invalid;
		}
		else
		{
			result.Type = (Drawing)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Data = ((MessagePackObject)(ref lastReadData2)).AsBinary();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Scribbler = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<ScribbleContent Type={Type} Data={Data} Scribbler={Scribbler}>";
	}
}
