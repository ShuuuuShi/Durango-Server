using MsgPack;

namespace Messages;

public struct Options
{
	public BoolOption[] Bool;

	public IntegerOption[] Int;

	public FloatOption[] Float;

	public static void Pack(Packer packer, Options val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.Bool == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Bool.Length);
			for (int i = 0; i < val.Bool.Length; i++)
			{
				BoolOption.Pack(packer, val.Bool[i]);
			}
		}
		if (val.Int == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Int.Length);
			for (int j = 0; j < val.Int.Length; j++)
			{
				IntegerOption.Pack(packer, val.Int[j]);
			}
		}
		if (val.Float == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Float.Length);
		for (int k = 0; k < val.Float.Length; k++)
		{
			FloatOption.Pack(packer, val.Float[k]);
		}
	}

	public static Options Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		Options result = default(Options);
		result.Bool = new BoolOption[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref BoolOption reference = ref result.Bool[i];
			reference = BoolOption.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Int = new IntegerOption[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref IntegerOption reference2 = ref result.Int[j];
			reference2 = IntegerOption.Unpack(unpacker);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Float = new FloatOption[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			ref FloatOption reference3 = ref result.Float[k];
			reference3 = FloatOption.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Options Bool={Bool} Int={Int} Float={Float}>";
	}
}
