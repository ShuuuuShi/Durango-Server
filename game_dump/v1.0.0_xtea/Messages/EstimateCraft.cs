using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct EstimateCraft
{
	public const uint TypeCode = 7u;

	public string RecipeId;

	public Dictionary<string, ulong[]> Materials;

	public ulong? ToolItemId;

	public ulong? WorkbenchEntityId;

	public Point2? WorkbenchTile;

	public short? WorkbenchPassword;

	public static void Pack(Packer packer, EstimateCraft val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(7);
			packer.Pack(7u);
		}
		else
		{
			packer.PackArrayHeader(6);
		}
		if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
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

	public static EstimateCraft Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		EstimateCraft result = default(EstimateCraft);
		result.RecipeId = ((MessagePackObject)(ref lastReadData)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Materials = new Dictionary<string, ulong[]>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData3)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			int num2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
			ulong[] array = new ulong[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				int num3 = j;
				MessagePackObject lastReadData5 = unpacker.LastReadData;
				array[num3] = ((MessagePackObject)(ref lastReadData5)).AsUInt64();
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
			ushort num4 = default(ushort);
			unpacker.ReadUInt16(ref num4);
			Point2 value3 = default(Point2);
			value3.x = num4;
			unpacker.ReadUInt16(ref num4);
			value3.y = num4;
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
		return $"<EstimateCraft RecipeId={RecipeId} Materials={Materials} ToolItemId={ToolItemId} WorkbenchEntityId={WorkbenchEntityId} WorkbenchTile={WorkbenchTile} WorkbenchPassword={WorkbenchPassword}>";
	}
}
