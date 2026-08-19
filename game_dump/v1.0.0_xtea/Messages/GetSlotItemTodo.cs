using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct GetSlotItemTodo
{
	public const uint TypeCode = 3523u;

	public string SlotId;

	public string SlotName;

	public Dictionary<string, int> RequiredTags;

	public Dictionary<string, int> RequiredMaterials;

	public int Count;

	public static void Pack(Packer packer, GetSlotItemTodo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3523u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		if (val.SlotId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SlotId);
		}
		packer.PackString(val.SlotName);
		if (val.RequiredTags == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.RequiredTags.Count);
			foreach (KeyValuePair<string, int> requiredTag in val.RequiredTags)
			{
				if (requiredTag.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(requiredTag.Key);
				}
				packer.Pack(requiredTag.Value);
			}
		}
		if (val.RequiredMaterials == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.RequiredMaterials.Count);
			foreach (KeyValuePair<string, int> requiredMaterial in val.RequiredMaterials)
			{
				if (requiredMaterial.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(requiredMaterial.Key);
				}
				packer.Pack(requiredMaterial.Value);
			}
		}
		packer.Pack(val.Count);
	}

	public static GetSlotItemTodo Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetSlotItemTodo result = default(GetSlotItemTodo);
		result.SlotId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		result.SlotName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.RequiredTags = new Dictionary<string, int>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData3)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int value = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			result.RequiredTags.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.RequiredMaterials = new Dictionary<string, int>(num2);
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			string key2 = ((MessagePackObject)(ref lastReadData6)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			int value2 = ((MessagePackObject)(ref lastReadData7)).AsInt32();
			result.RequiredMaterials.Add(key2, value2);
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		result.Count = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<GetSlotItemTodo SlotId={SlotId} SlotName={SlotName} RequiredTags={RequiredTags} RequiredMaterials={RequiredMaterials} Count={Count}>";
	}
}
