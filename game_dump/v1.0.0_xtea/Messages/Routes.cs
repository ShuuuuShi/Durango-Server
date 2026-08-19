using System.Collections.Generic;
using MsgPack;
using Shared.Region;

namespace Messages;

public struct Routes
{
	public const uint TypeCode = 2032u;

	public Dictionary<Role, Dictionary<string, KeyValuePair<bool, Route[]>>> _Routes;

	public KeyValuePair<int, int> RecommendRandomFee;

	public KeyValuePair<int, int> RecommendSafeFee;

	public static void Pack(Packer packer, Routes val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2032u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val._Routes == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val._Routes.Count);
			foreach (KeyValuePair<Role, Dictionary<string, KeyValuePair<bool, Route[]>>> route in val._Routes)
			{
				packer.Pack((int)route.Key);
				if (route.Value == null)
				{
					packer.PackMapHeader(0);
					continue;
				}
				packer.PackMapHeader(route.Value.Count);
				foreach (KeyValuePair<string, KeyValuePair<bool, Route[]>> item in route.Value)
				{
					if (item.Key == null)
					{
						packer.PackString(string.Empty);
					}
					else
					{
						packer.PackString(item.Key);
					}
					packer.PackArrayHeader(2);
					packer.Pack(item.Value.Key);
					if (item.Value.Value == null)
					{
						packer.PackArrayHeader(0);
						continue;
					}
					packer.PackArrayHeader(item.Value.Value.Length);
					for (int i = 0; i < item.Value.Value.Length; i++)
					{
						Route.Pack(packer, item.Value.Value[i]);
					}
				}
			}
		}
		packer.PackArrayHeader(2);
		packer.Pack(val.RecommendRandomFee.Key);
		packer.Pack(val.RecommendRandomFee.Value);
		packer.PackArrayHeader(2);
		packer.Pack(val.RecommendSafeFee.Key);
		packer.Pack(val.RecommendSafeFee.Value);
	}

	public static Routes Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		Routes result = default(Routes);
		result._Routes = new Dictionary<Role, Dictionary<string, KeyValuePair<bool, Route[]>>>(num, default(RoleComparer));
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData2 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
			Role key = ((num2 >= 0 && 5 >= num2) ? ((Role)num2) : Role.Invalid);
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
			Dictionary<string, KeyValuePair<bool, Route[]>> dictionary = new Dictionary<string, KeyValuePair<bool, Route[]>>(num3);
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				MessagePackObject lastReadData4 = unpacker.LastReadData;
				string key2 = ((MessagePackObject)(ref lastReadData4)).AsString();
				unpacker.Read();
				unpacker.Read();
				MessagePackObject lastReadData5 = unpacker.LastReadData;
				bool key3 = ((MessagePackObject)(ref lastReadData5)).AsBoolean();
				unpacker.Read();
				MessagePackObject lastReadData6 = unpacker.LastReadData;
				int num4 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
				Route[] array = new Route[num4];
				for (int k = 0; k < num4; k++)
				{
					unpacker.Read();
					ref Route reference = ref array[k];
					reference = Route.Unpack(unpacker);
				}
				KeyValuePair<bool, Route[]> value = new KeyValuePair<bool, Route[]>(key3, array);
				dictionary.Add(key2, value);
			}
			result._Routes.Add(key, dictionary);
		}
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		int key4 = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		int value2 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		result.RecommendRandomFee = new KeyValuePair<int, int>(key4, value2);
		unpacker.Read();
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		int key5 = ((MessagePackObject)(ref lastReadData9)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		int value3 = ((MessagePackObject)(ref lastReadData10)).AsInt32();
		result.RecommendSafeFee = new KeyValuePair<int, int>(key5, value3);
		return result;
	}

	public override string ToString()
	{
		return $"<Routes _Routes={_Routes} RecommendRandomFee={RecommendRandomFee} RecommendSafeFee={RecommendSafeFee}>";
	}
}
