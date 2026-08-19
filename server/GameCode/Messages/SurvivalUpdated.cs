using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct SurvivalUpdated
{
	public const uint TypeCode = 183u;

	public string EntityId;

	public Dictionary<string, Gauge> Updated;

	public string[] Removed;

	public static void Pack(Packer packer, SurvivalUpdated val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(183u);
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
		if (val.Updated == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Updated.Count);
			foreach (KeyValuePair<string, Gauge> item in val.Updated)
			{
				if (item.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(item.Key);
				}
				Gauge.PackTo(item.Value, packer);
			}
		}
		if (val.Removed == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Removed.Length);
		for (int i = 0; i < val.Removed.Length; i++)
		{
			if (val.Removed[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Removed[i]);
			}
		}
	}

	public static SurvivalUpdated Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SurvivalUpdated result = default(SurvivalUpdated);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Updated = new Dictionary<string, Gauge>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			string key = unpacker.LastReadData.AsString();
			unpacker.Read();
			Gauge value = Gauge.UnpackFrom(unpacker);
			result.Updated.Add(key, value);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Removed = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.Removed[j] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SurvivalUpdated EntityId={EntityId} Updated={Updated} Removed={Removed}>";
	}
}
