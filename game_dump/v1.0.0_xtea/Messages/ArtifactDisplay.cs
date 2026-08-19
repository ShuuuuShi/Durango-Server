using System.Collections.Generic;
using MsgPack;
using Shared.Building;

namespace Messages;

public struct ArtifactDisplay
{
	public const uint TypeCode = 2433u;

	public ulong EntityId;

	public Condition Condition;

	public string Color;

	public Dictionary<string, string> Parts;

	public Dictionary<string, KeyValuePair<string, string>> Decorations;

	public Dictionary<int, KeyValuePair<string, string>> AddOns;

	public string Crop;

	public ushort[] PetEntityTypes;

	public string Effect;

	public static void Pack(Packer packer, ArtifactDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(2433u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		packer.Pack(val.EntityId);
		packer.Pack((int)val.Condition);
		if (val.Color == null)
		{
			packer.PackNull();
		}
		else if (val.Color == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Color);
		}
		if (val.Parts == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Parts.Count);
			foreach (KeyValuePair<string, string> part in val.Parts)
			{
				if (part.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(part.Key);
				}
				if (part.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(part.Value);
				}
			}
		}
		if (val.Decorations == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.Decorations.Count);
			foreach (KeyValuePair<string, KeyValuePair<string, string>> decoration in val.Decorations)
			{
				if (decoration.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(decoration.Key);
				}
				packer.PackArrayHeader(2);
				if (decoration.Value.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(decoration.Value.Key);
				}
				if (decoration.Value.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(decoration.Value.Value);
				}
			}
		}
		if (val.AddOns == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.AddOns.Count);
			foreach (KeyValuePair<int, KeyValuePair<string, string>> addOn in val.AddOns)
			{
				packer.Pack(addOn.Key);
				packer.PackArrayHeader(2);
				if (addOn.Value.Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(addOn.Value.Key);
				}
				if (addOn.Value.Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(addOn.Value.Value);
				}
			}
		}
		if (val.Crop == null)
		{
			packer.PackNull();
		}
		else if (val.Crop == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Crop);
		}
		if (val.PetEntityTypes == null)
		{
			packer.PackNull();
		}
		else if (val.PetEntityTypes == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.PetEntityTypes.Length);
			for (int i = 0; i < val.PetEntityTypes.Length; i++)
			{
				packer.Pack(val.PetEntityTypes[i]);
			}
		}
		if (val.Effect == null)
		{
			packer.PackNull();
		}
		else if (val.Effect == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Effect);
		}
	}

	public static ArtifactDisplay Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0267: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactDisplay result = default(ArtifactDisplay);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 3 < num)
		{
			result.Condition = Condition.Invalid;
		}
		else
		{
			result.Condition = (Condition)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.Color = null;
		}
		else
		{
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			string color = ((MessagePackObject)(ref lastReadData4)).AsString();
			result.Color = color;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		result.Parts = new Dictionary<string, string>(num2);
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			MessagePackObject lastReadData6 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData6)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			string value = ((MessagePackObject)(ref lastReadData7)).AsString();
			result.Parts.Add(key, value);
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		result.Decorations = new Dictionary<string, KeyValuePair<string, string>>(num3);
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			string key2 = ((MessagePackObject)(ref lastReadData9)).AsString();
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData10 = unpacker.LastReadData;
			string key3 = ((MessagePackObject)(ref lastReadData10)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData11 = unpacker.LastReadData;
			string value2 = ((MessagePackObject)(ref lastReadData11)).AsString();
			KeyValuePair<string, string> value3 = new KeyValuePair<string, string>(key3, value2);
			result.Decorations.Add(key2, value3);
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		int num4 = ((MessagePackObject)(ref lastReadData12)).AsInt32();
		result.AddOns = new Dictionary<int, KeyValuePair<string, string>>(num4);
		for (int k = 0; k < num4; k++)
		{
			unpacker.Read();
			MessagePackObject lastReadData13 = unpacker.LastReadData;
			int key4 = ((MessagePackObject)(ref lastReadData13)).AsInt32();
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData14 = unpacker.LastReadData;
			string key5 = ((MessagePackObject)(ref lastReadData14)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData15 = unpacker.LastReadData;
			string value4 = ((MessagePackObject)(ref lastReadData15)).AsString();
			KeyValuePair<string, string> value5 = new KeyValuePair<string, string>(key5, value4);
			result.AddOns.Add(key4, value5);
		}
		unpacker.Read();
		MessagePackObject lastReadData16 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData16)).IsNil)
		{
			result.Crop = null;
		}
		else
		{
			MessagePackObject lastReadData17 = unpacker.LastReadData;
			string crop = ((MessagePackObject)(ref lastReadData17)).AsString();
			result.Crop = crop;
		}
		unpacker.Read();
		MessagePackObject lastReadData18 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData18)).IsNil)
		{
			result.PetEntityTypes = null;
		}
		else
		{
			MessagePackObject lastReadData19 = unpacker.LastReadData;
			int num5 = ((MessagePackObject)(ref lastReadData19)).AsInt32();
			ushort[] array = new ushort[num5];
			for (int l = 0; l < num5; l++)
			{
				unpacker.Read();
				int num6 = l;
				MessagePackObject lastReadData20 = unpacker.LastReadData;
				array[num6] = ((MessagePackObject)(ref lastReadData20)).AsUInt16();
			}
			result.PetEntityTypes = array;
		}
		unpacker.Read();
		MessagePackObject lastReadData21 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData21)).IsNil)
		{
			result.Effect = null;
		}
		else
		{
			MessagePackObject lastReadData22 = unpacker.LastReadData;
			string effect = ((MessagePackObject)(ref lastReadData22)).AsString();
			result.Effect = effect;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactDisplay EntityId={EntityId} Condition={Condition} Color={Color} Parts={Parts} Decorations={Decorations} AddOns={AddOns} Crop={Crop} PetEntityTypes={PetEntityTypes} Effect={Effect}>";
	}
}
