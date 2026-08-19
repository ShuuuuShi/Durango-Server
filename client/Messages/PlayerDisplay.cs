using MsgPack;
using Shared.Display;

namespace Messages;

public struct PlayerDisplay
{
	public const uint TypeCode = 2431u;

	public string EntityId;

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

	public string PortraitIcon;

	public int VoiceType;

	public float BodySize;

	public bool Invisible;

	public WeaponDisplayInfo WeaponInfo;

	public BoardingOn BoardingOn;

	public TimeRange BoardingTime;

	public string VehicleEntityId;

	public string Accessory;

	public static void Pack(Packer packer, PlayerDisplay val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(30);
			packer.Pack(2431u);
		}
		else
		{
			packer.PackArrayHeader(29);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
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
		if (val.PortraitIcon == null)
		{
			packer.PackNull();
		}
		else if (val.PortraitIcon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PortraitIcon);
		}
		packer.Pack(val.VoiceType);
		packer.Pack(val.BodySize);
		packer.Pack(val.Invisible);
		WeaponDisplayInfo.Pack(packer, val.WeaponInfo);
		packer.Pack((int)val.BoardingOn);
		TimeRange.Pack(packer, val.BoardingTime);
		if (val.VehicleEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.VehicleEntityId);
		}
		if (val.Accessory == null)
		{
			packer.PackNull();
		}
		else if (val.Accessory == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Accessory);
		}
	}

	public static PlayerDisplay Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerDisplay result = default(PlayerDisplay);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.DefaultBody = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.DefaultInner = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.DefaultHead = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.DefaultHair = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Hair = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Body = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Head = null;
		}
		else
		{
			string head = unpacker.LastReadData.AsString();
			result.Head = head;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Equip = null;
		}
		else
		{
			string equip = unpacker.LastReadData.AsString();
			result.Equip = equip;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Beard = null;
		}
		else
		{
			string beard = unpacker.LastReadData.AsString();
			result.Beard = beard;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.BodyColor = null;
		}
		else
		{
			int num = unpacker.LastReadData.AsInt32();
			string[] array = new string[num];
			for (int i = 0; i < num; i++)
			{
				unpacker.Read();
				array[i] = unpacker.LastReadData.AsString();
			}
			result.BodyColor = array;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.HeadColor = null;
		}
		else
		{
			int num2 = unpacker.LastReadData.AsInt32();
			string[] array2 = new string[num2];
			for (int j = 0; j < num2; j++)
			{
				unpacker.Read();
				array2[j] = unpacker.LastReadData.AsString();
			}
			result.HeadColor = array2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EquipColor = null;
		}
		else
		{
			int num3 = unpacker.LastReadData.AsInt32();
			string[] array3 = new string[num3];
			for (int k = 0; k < num3; k++)
			{
				unpacker.Read();
				array3[k] = unpacker.LastReadData.AsString();
			}
			result.EquipColor = array3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SkinColor = null;
		}
		else
		{
			string skinColor = unpacker.LastReadData.AsString();
			result.SkinColor = skinColor;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.HairColor = null;
		}
		else
		{
			string hairColor = unpacker.LastReadData.AsString();
			result.HairColor = hairColor;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EyeColor = null;
		}
		else
		{
			string eyeColor = unpacker.LastReadData.AsString();
			result.EyeColor = eyeColor;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.LipColor = null;
		}
		else
		{
			string lipColor = unpacker.LastReadData.AsString();
			result.LipColor = lipColor;
		}
		unpacker.Read();
		result.Portrait = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.PortraitBg = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PortraitBgColor = null;
		}
		else
		{
			string portraitBgColor = unpacker.LastReadData.AsString();
			result.PortraitBgColor = portraitBgColor;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PortraitIcon = null;
		}
		else
		{
			string portraitIcon = unpacker.LastReadData.AsString();
			result.PortraitIcon = portraitIcon;
		}
		unpacker.Read();
		result.VoiceType = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.BodySize = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Invisible = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.WeaponInfo = WeaponDisplayInfo.Unpack(unpacker);
		unpacker.Read();
		int num4 = unpacker.LastReadData.AsInt32();
		if (num4 < 0 || 3 < num4)
		{
			result.BoardingOn = BoardingOn.Invalid;
		}
		else
		{
			result.BoardingOn = (BoardingOn)num4;
		}
		unpacker.Read();
		result.BoardingTime = TimeRange.Unpack(unpacker);
		unpacker.Read();
		result.VehicleEntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Accessory = null;
		}
		else
		{
			string accessory = unpacker.LastReadData.AsString();
			result.Accessory = accessory;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerDisplay EntityId={EntityId} DefaultBody={DefaultBody} DefaultInner={DefaultInner} DefaultHead={DefaultHead} DefaultHair={DefaultHair} Hair={Hair} Body={Body} Head={Head} Equip={Equip} Beard={Beard} BodyColor={BodyColor} HeadColor={HeadColor} EquipColor={EquipColor} SkinColor={SkinColor} HairColor={HairColor} EyeColor={EyeColor} LipColor={LipColor} Portrait={Portrait} PortraitBg={PortraitBg} PortraitBgColor={PortraitBgColor} PortraitIcon={PortraitIcon} VoiceType={VoiceType} BodySize={BodySize} Invisible={Invisible} WeaponInfo={WeaponInfo} BoardingOn={BoardingOn} BoardingTime={BoardingTime} VehicleEntityId={VehicleEntityId} Accessory={Accessory}>";
	}
}
