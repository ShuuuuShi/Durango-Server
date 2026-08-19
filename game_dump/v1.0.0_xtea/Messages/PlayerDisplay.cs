using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PlayerDisplay
{
	public const uint TypeCode = 2431u;

	public ulong EntityId;

	public string DefaultBody;

	public string DefaultInner;

	public string DefaultHead;

	public string DefaultHair;

	public string Hair;

	public string Body;

	public string Head;

	public string Equip;

	public string Beard;

	public string[] BodyColor;

	public string[] HeadColor;

	public string[] EquipColor;

	public string SkinColor;

	public string HairColor;

	public string EyeColor;

	public string LipColor;

	public int Portrait;

	public int PortraitBg;

	public string PortraitBgColor;

	public int VoiceType;

	public float BodySize;

	public bool Invisible;

	public KeyValuePair<string, string>[] Effects;

	public WeaponDisplayInfo WeaponInfo;

	public static void Pack(Packer packer, PlayerDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(26);
			packer.Pack(2431u);
		}
		else
		{
			packer.PackArrayHeader(25);
		}
		packer.Pack(val.EntityId);
		if (val.DefaultBody == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.DefaultBody);
		}
		if (val.DefaultInner == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.DefaultInner);
		}
		if (val.DefaultHead == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.DefaultHead);
		}
		if (val.DefaultHair == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.DefaultHair);
		}
		if (val.Hair == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Hair);
		}
		if (val.Body == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Body);
		}
		if (val.Head == null)
		{
			packer.PackNull();
		}
		else if (val.Head == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Head);
		}
		if (val.Equip == null)
		{
			packer.PackNull();
		}
		else if (val.Equip == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Equip);
		}
		if (val.Beard == null)
		{
			packer.PackNull();
		}
		else if (val.Beard == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Beard);
		}
		if (val.BodyColor == null)
		{
			packer.PackNull();
		}
		else if (val.BodyColor == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.BodyColor.Length);
			for (int i = 0; i < val.BodyColor.Length; i++)
			{
				if (val.BodyColor[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.BodyColor[i]);
				}
			}
		}
		if (val.HeadColor == null)
		{
			packer.PackNull();
		}
		else if (val.HeadColor == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.HeadColor.Length);
			for (int j = 0; j < val.HeadColor.Length; j++)
			{
				if (val.HeadColor[j] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.HeadColor[j]);
				}
			}
		}
		if (val.EquipColor == null)
		{
			packer.PackNull();
		}
		else if (val.EquipColor == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.EquipColor.Length);
			for (int k = 0; k < val.EquipColor.Length; k++)
			{
				if (val.EquipColor[k] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.EquipColor[k]);
				}
			}
		}
		if (val.SkinColor == null)
		{
			packer.PackNull();
		}
		else if (val.SkinColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SkinColor);
		}
		if (val.HairColor == null)
		{
			packer.PackNull();
		}
		else if (val.HairColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.HairColor);
		}
		if (val.EyeColor == null)
		{
			packer.PackNull();
		}
		else if (val.EyeColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EyeColor);
		}
		if (val.LipColor == null)
		{
			packer.PackNull();
		}
		else if (val.LipColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.LipColor);
		}
		packer.Pack(val.Portrait);
		packer.Pack(val.PortraitBg);
		if (val.PortraitBgColor == null)
		{
			packer.PackNull();
		}
		else if (val.PortraitBgColor == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PortraitBgColor);
		}
		packer.Pack(val.VoiceType);
		packer.Pack(val.BodySize);
		packer.Pack(val.Invisible);
		if (val.Effects == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Effects.Length);
			for (int l = 0; l < val.Effects.Length; l++)
			{
				packer.PackArrayHeader(2);
				if (val.Effects[l].Key == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Effects[l].Key);
				}
				if (val.Effects[l].Value == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Effects[l].Value);
				}
			}
		}
		WeaponDisplayInfo.Pack(packer, val.WeaponInfo);
	}

	public static PlayerDisplay Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0179: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_030f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_032b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_034c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0431: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0451: Unknown result type (might be due to invalid IL or missing references)
		//IL_046d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0490: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_04cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_051a: Unknown result type (might be due to invalid IL or missing references)
		//IL_051f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0532: Unknown result type (might be due to invalid IL or missing references)
		//IL_0537: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerDisplay result = default(PlayerDisplay);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.DefaultBody = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.DefaultInner = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.DefaultHead = ((MessagePackObject)(ref lastReadData4)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.DefaultHair = ((MessagePackObject)(ref lastReadData5)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Hair = ((MessagePackObject)(ref lastReadData6)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.Body = ((MessagePackObject)(ref lastReadData7)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData8)).IsNil)
		{
			result.Head = null;
		}
		else
		{
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			string head = ((MessagePackObject)(ref lastReadData9)).AsString();
			result.Head = head;
		}
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.Equip = null;
		}
		else
		{
			MessagePackObject lastReadData11 = unpacker.LastReadData;
			string equip = ((MessagePackObject)(ref lastReadData11)).AsString();
			result.Equip = equip;
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData12)).IsNil)
		{
			result.Beard = null;
		}
		else
		{
			MessagePackObject lastReadData13 = unpacker.LastReadData;
			string beard = ((MessagePackObject)(ref lastReadData13)).AsString();
			result.Beard = beard;
		}
		unpacker.Read();
		MessagePackObject lastReadData14 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData14)).IsNil)
		{
			result.BodyColor = null;
		}
		else
		{
			MessagePackObject lastReadData15 = unpacker.LastReadData;
			int num = ((MessagePackObject)(ref lastReadData15)).AsInt32();
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				int num2 = i;
				MessagePackObject lastReadData16 = unpacker.LastReadData;
				array[num2] = ((MessagePackObject)(ref lastReadData16)).AsString();
			}
			result.BodyColor = array;
		}
		unpacker.Read();
		MessagePackObject lastReadData17 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData17)).IsNil)
		{
			result.HeadColor = null;
		}
		else
		{
			MessagePackObject lastReadData18 = unpacker.LastReadData;
			int num3 = ((MessagePackObject)(ref lastReadData18)).AsInt32();
			string[] array2 = new string[num3];
			for (int j = 0; j < num3; j++)
			{
				unpacker.Read();
				int num4 = j;
				MessagePackObject lastReadData19 = unpacker.LastReadData;
				array2[num4] = ((MessagePackObject)(ref lastReadData19)).AsString();
			}
			result.HeadColor = array2;
		}
		unpacker.Read();
		MessagePackObject lastReadData20 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData20)).IsNil)
		{
			result.EquipColor = null;
		}
		else
		{
			MessagePackObject lastReadData21 = unpacker.LastReadData;
			int num5 = ((MessagePackObject)(ref lastReadData21)).AsInt32();
			string[] array3 = new string[num5];
			for (int k = 0; k < num5; k++)
			{
				unpacker.Read();
				int num6 = k;
				MessagePackObject lastReadData22 = unpacker.LastReadData;
				array3[num6] = ((MessagePackObject)(ref lastReadData22)).AsString();
			}
			result.EquipColor = array3;
		}
		unpacker.Read();
		MessagePackObject lastReadData23 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData23)).IsNil)
		{
			result.SkinColor = null;
		}
		else
		{
			MessagePackObject lastReadData24 = unpacker.LastReadData;
			string skinColor = ((MessagePackObject)(ref lastReadData24)).AsString();
			result.SkinColor = skinColor;
		}
		unpacker.Read();
		MessagePackObject lastReadData25 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData25)).IsNil)
		{
			result.HairColor = null;
		}
		else
		{
			MessagePackObject lastReadData26 = unpacker.LastReadData;
			string hairColor = ((MessagePackObject)(ref lastReadData26)).AsString();
			result.HairColor = hairColor;
		}
		unpacker.Read();
		MessagePackObject lastReadData27 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData27)).IsNil)
		{
			result.EyeColor = null;
		}
		else
		{
			MessagePackObject lastReadData28 = unpacker.LastReadData;
			string eyeColor = ((MessagePackObject)(ref lastReadData28)).AsString();
			result.EyeColor = eyeColor;
		}
		unpacker.Read();
		MessagePackObject lastReadData29 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData29)).IsNil)
		{
			result.LipColor = null;
		}
		else
		{
			MessagePackObject lastReadData30 = unpacker.LastReadData;
			string lipColor = ((MessagePackObject)(ref lastReadData30)).AsString();
			result.LipColor = lipColor;
		}
		unpacker.Read();
		MessagePackObject lastReadData31 = unpacker.LastReadData;
		result.Portrait = ((MessagePackObject)(ref lastReadData31)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData32 = unpacker.LastReadData;
		result.PortraitBg = ((MessagePackObject)(ref lastReadData32)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData33 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData33)).IsNil)
		{
			result.PortraitBgColor = null;
		}
		else
		{
			MessagePackObject lastReadData34 = unpacker.LastReadData;
			string portraitBgColor = ((MessagePackObject)(ref lastReadData34)).AsString();
			result.PortraitBgColor = portraitBgColor;
		}
		unpacker.Read();
		MessagePackObject lastReadData35 = unpacker.LastReadData;
		result.VoiceType = ((MessagePackObject)(ref lastReadData35)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData36 = unpacker.LastReadData;
		result.BodySize = ((MessagePackObject)(ref lastReadData36)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData37 = unpacker.LastReadData;
		result.Invisible = ((MessagePackObject)(ref lastReadData37)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData38 = unpacker.LastReadData;
		int num7 = ((MessagePackObject)(ref lastReadData38)).AsInt32();
		result.Effects = new KeyValuePair<string, string>[num7];
		for (int l = 0; l < num7; l++)
		{
			unpacker.Read();
			unpacker.Read();
			MessagePackObject lastReadData39 = unpacker.LastReadData;
			string key = ((MessagePackObject)(ref lastReadData39)).AsString();
			unpacker.Read();
			MessagePackObject lastReadData40 = unpacker.LastReadData;
			string value = ((MessagePackObject)(ref lastReadData40)).AsString();
			ref KeyValuePair<string, string> reference = ref result.Effects[l];
			reference = new KeyValuePair<string, string>(key, value);
		}
		unpacker.Read();
		result.WeaponInfo = WeaponDisplayInfo.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerDisplay EntityId={EntityId} DefaultBody={DefaultBody} DefaultInner={DefaultInner} DefaultHead={DefaultHead} DefaultHair={DefaultHair} Hair={Hair} Body={Body} Head={Head} Equip={Equip} Beard={Beard} BodyColor={BodyColor} HeadColor={HeadColor} EquipColor={EquipColor} SkinColor={SkinColor} HairColor={HairColor} EyeColor={EyeColor} LipColor={LipColor} Portrait={Portrait} PortraitBg={PortraitBg} PortraitBgColor={PortraitBgColor} VoiceType={VoiceType} BodySize={BodySize} Invisible={Invisible} Effects={Effects} WeaponInfo={WeaponInfo}>";
	}
}
