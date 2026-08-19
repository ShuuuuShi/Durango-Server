using System.Collections.Generic;
using MsgPack;
using Shared.Item;

namespace Messages;

public struct EstimateDye
{
	public const uint TypeCode = 3668u;

	public ColorChannel Channel;

	public Dictionary<string, ulong[]> Materials;

	public ulong? ToolItemId;

	public ulong? WorkbenchEntityId;

	public Point2? WorkbenchTile;

	public short? WorkbenchPassword;

	public static void Pack(Packer packer, EstimateDye val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(3668u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		packer.Pack((int)val.Channel);
		if (val.Materials == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Materials.Count);
			foreach (KeyValuePair<string, ulong[]> material in val.Materials)
			{
				if (material.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(material.Key);
				}
				if (material.Value == null)
				{
					packer.PackArrayHeader(0);
					continue;
				}
				packer.PackArrayHeader(material.Value.Length);
				for (int i = 0; i < material.Value.Length; i++)
				{
					packer.Pack(material.Value[i]);
				}
			}
		}
		if (!val.ToolItemId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.ToolItemId.Value);
		}
		if (!val.WorkbenchEntityId.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.WorkbenchEntityId.Value);
		}
		if (!val.WorkbenchTile.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack((ushort)val.WorkbenchTile.Value.x);
			packer.Pack((ushort)val.WorkbenchTile.Value.y);
		}
		if (!val.WorkbenchPassword.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.WorkbenchPassword.Value);
		}
	}

	public static EstimateDye Unpack(Unpacker unpacker)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_016d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0172: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData)).AsInt32();
		EstimateDye result = default(EstimateDye);
		if (num < 0 || 2 < num)
		{
			result.Channel = ColorChannel.Invalid;
		}
		else
		{
			result.Channel = (ColorChannel)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Materials = new Dictionary<string, ulong[]>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData3)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			ulong[] array = new ulong[num3];
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int num4 = j;
				MessagePackObject lastReadData5 = unpacker.LastReadData;
				array[num4] = ((MessagePackObject)(ref lastReadData5)).AsUInt64();
			}
			result.Materials.Add(key, array);
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.ToolItemId = null;
		}
		else
		{
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			ulong value = ((MessagePackObject)(ref lastReadData7)).AsUInt64();
			result.ToolItemId = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData8)).IsNil)
		{
			result.WorkbenchEntityId = null;
		}
		else
		{
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			ulong value2 = ((MessagePackObject)(ref lastReadData9)).AsUInt64();
			result.WorkbenchEntityId = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.WorkbenchTile = null;
		}
		else
		{
			ushort num5 = default(ushort);
			unpacker.ReadUInt16(ref num5);
			Point2 value3 = default(Point2);
			value3.x = num5;
			unpacker.ReadUInt16(ref num5);
			value3.y = num5;
			result.WorkbenchTile = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData11)).IsNil)
		{
			result.WorkbenchPassword = null;
		}
		else
		{
			MessagePackObject lastReadData12 = unpacker.LastReadData;
			short value4 = ((MessagePackObject)(ref lastReadData12)).AsInt16();
			result.WorkbenchPassword = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<EstimateDye Channel={Channel} Materials={Materials} ToolItemId={ToolItemId} WorkbenchEntityId={WorkbenchEntityId} WorkbenchTile={WorkbenchTile} WorkbenchPassword={WorkbenchPassword}>";
	}
}
