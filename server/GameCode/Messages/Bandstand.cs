using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct Bandstand
{
	public const uint TypeCode = 63459083u;

	public PropKey PropKey;

	public string Host;

	public double? ExpiresAt;

	public Dictionary<int, Band> Bands;

	public static void Pack(Packer packer, Bandstand val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(63459083u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		PropKey.Pack(packer, val.PropKey);
		if (val.Host == null)
		{
			packer.PackNull();
		}
		else if (val.Host == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Host);
		}
		if (!val.ExpiresAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ExpiresAt.Value);
		}
		if (val.Bands == null)
		{
			packer.PackMapHeader(0);
			return;
		}
		packer.PackMapHeader(val.Bands.Count);
		foreach (KeyValuePair<int, Band> band in val.Bands)
		{
			packer.Pack(band.Key);
			Band.Pack(packer, band.Value);
		}
	}

	public static Bandstand Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Bandstand result = default(Bandstand);
		result.PropKey = PropKey.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Host = null;
		}
		else
		{
			string host = unpacker.LastReadData.AsString();
			result.Host = host;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ExpiresAt = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.ExpiresAt = value;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Bands = new Dictionary<int, Band>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int key = unpacker.LastReadData.AsInt32();
			unpacker.Read();
			Band value2 = Band.Unpack(unpacker);
			result.Bands.Add(key, value2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Bandstand PropKey={PropKey} Host={Host} ExpiresAt={ExpiresAt} Bands={Bands}>";
	}
}
