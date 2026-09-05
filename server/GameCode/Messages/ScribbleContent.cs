using MsgPack;
using Shared.MessageBoard;

namespace Messages;

public struct ScribbleContent
{
	public Drawing Type;

	public byte[] Data;

	public string Scribbler;

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
		if (val.Scribbler == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Scribbler);
		}
	}

	public static ScribbleContent Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
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
		result.Data = unpacker.LastReadData.AsBinary();
		unpacker.Read();
		result.Scribbler = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<ScribbleContent Type={Type} Data={Data} Scribbler={Scribbler}>";
	}
}
