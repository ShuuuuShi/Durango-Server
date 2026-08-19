using MsgPack;

namespace Messages;

public struct Weather
{
	public const uint TypeCode = 331u;

	public string _Weather;

	public float WeatherRatio;

	public static void Pack(Packer packer, Weather val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(331u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val._Weather == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val._Weather);
		}
		packer.Pack(val.WeatherRatio);
	}

	public static Weather Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Weather result = default(Weather);
		result._Weather = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.WeatherRatio = ((MessagePackObject)(ref lastReadData2)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Weather _Weather={_Weather} WeatherRatio={WeatherRatio}>";
	}
}
