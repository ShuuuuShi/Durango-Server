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
		unpacker.Read();
		Weather result = default(Weather);
		result._Weather = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.WeatherRatio = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Weather _Weather={_Weather} WeatherRatio={WeatherRatio}>";
	}
}
