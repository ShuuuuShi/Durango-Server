using MsgPack;

namespace Messages;

public struct Item
{
	public ulong Id;

	public string Name;

	public string Description;

	public string Icon;

	public string Prototype;

	public int Level;

	public int EquipLevel;

	public int ModifiableCount;

	public int Size;

	public Gauge Durability;

	public string ColorR;

	public string ColorG;

	public string ColorB;

	public ulong FounderId;

	public string FounderCategory;

	public Tag[] Tags;

	public Tag[] TagModifications;

	public Performance[] Performance;

	public object Cargo;

	public static void Pack(Packer packer, Item val, bool hint = false)
	{
		packer.PackArrayHeader(19);
		packer.Pack(val.Id);
		packer.PackString(val.Name);
		packer.PackString(val.Description);
		if (val.Icon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Icon);
		}
		if (val.Prototype == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Prototype);
		}
		packer.Pack(val.Level);
		packer.Pack(val.EquipLevel);
		packer.Pack(val.ModifiableCount);
		packer.Pack(val.Size);
		Gauge.PackTo(val.Durability, packer);
		if (val.ColorR == null)
		{
			packer.PackNull();
		}
		else if (val.ColorR == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ColorR);
		}
		if (val.ColorG == null)
		{
			packer.PackNull();
		}
		else if (val.ColorG == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ColorG);
		}
		if (val.ColorB == null)
		{
			packer.PackNull();
		}
		else if (val.ColorB == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ColorB);
		}
		packer.Pack(val.FounderId);
		if (val.FounderCategory == null)
		{
			packer.PackNull();
		}
		else if (val.FounderCategory == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.FounderCategory);
		}
		if (val.Tags == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Tags.Length);
			for (int i = 0; i < val.Tags.Length; i++)
			{
				Tag.Pack(packer, val.Tags[i]);
			}
		}
		if (val.TagModifications == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.TagModifications.Length);
			for (int j = 0; j < val.TagModifications.Length; j++)
			{
				Tag.Pack(packer, val.TagModifications[j]);
			}
		}
		if (val.Performance == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Performance.Length);
			for (int k = 0; k < val.Performance.Length; k++)
			{
				Messages.Performance.Pack(packer, val.Performance[k]);
			}
		}
		if (val.Cargo == null)
		{
			packer.PackNull();
		}
		else if (val.Cargo is Reins)
		{
			Reins.Pack(packer, (Reins)val.Cargo, hint: true);
		}
		else if (val.Cargo is ArtifactCapsule)
		{
			ArtifactCapsule.Pack(packer, (ArtifactCapsule)val.Cargo, hint: true);
		}
		else if (val.Cargo is ArtifactPackage)
		{
			ArtifactPackage.Pack(packer, (ArtifactPackage)val.Cargo, hint: true);
		}
		else if (val.Cargo is Container)
		{
			Container.Pack(packer, (Container)val.Cargo, hint: true);
		}
	}

	public static Item Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0212: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0345: Unknown result type (might be due to invalid IL or missing references)
		//IL_034a: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Item result = default(Item);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Description = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Icon = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Prototype = ((MessagePackObject)(ref lastReadData3)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.EquipLevel = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.ModifiableCount = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.Size = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		unpacker.Read();
		result.Durability = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData8)).IsNil)
		{
			result.ColorR = null;
		}
		else
		{
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			string colorR = ((MessagePackObject)(ref lastReadData9)).AsString();
			result.ColorR = colorR;
		}
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.ColorG = null;
		}
		else
		{
			MessagePackObject lastReadData11 = unpacker.LastReadData;
			string colorG = ((MessagePackObject)(ref lastReadData11)).AsString();
			result.ColorG = colorG;
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData12)).IsNil)
		{
			result.ColorB = null;
		}
		else
		{
			MessagePackObject lastReadData13 = unpacker.LastReadData;
			string colorB = ((MessagePackObject)(ref lastReadData13)).AsString();
			result.ColorB = colorB;
		}
		unpacker.Read();
		MessagePackObject lastReadData14 = unpacker.LastReadData;
		result.FounderId = ((MessagePackObject)(ref lastReadData14)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData15 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData15)).IsNil)
		{
			result.FounderCategory = null;
		}
		else
		{
			MessagePackObject lastReadData16 = unpacker.LastReadData;
			string founderCategory = ((MessagePackObject)(ref lastReadData16)).AsString();
			result.FounderCategory = founderCategory;
		}
		unpacker.Read();
		MessagePackObject lastReadData17 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData17)).AsInt32();
		result.Tags = new Tag[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Tag reference = ref result.Tags[i];
			reference = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData18 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData18)).AsInt32();
		result.TagModifications = new Tag[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Tag reference2 = ref result.TagModifications[j];
			reference2 = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData19 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData19)).AsInt32();
		result.Performance = new Performance[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			ref Performance reference3 = ref result.Performance[k];
			reference3 = Messages.Performance.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData20 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData20)).IsNil)
		{
			result.Cargo = null;
		}
		else
		{
			object cargo = null;
			uint num4 = default(uint);
			if (unpacker.ReadUInt32(ref num4))
			{
				switch (num4)
				{
				case 801u:
					cargo = Reins.Unpack(unpacker);
					break;
				case 3550u:
					cargo = ArtifactCapsule.Unpack(unpacker);
					break;
				case 3694u:
					cargo = ArtifactPackage.Unpack(unpacker);
					break;
				case 2058u:
					cargo = Container.Unpack(unpacker);
					break;
				default:
					Debug.LogError((object)("Unexpected type code: " + num4));
					break;
				}
			}
			result.Cargo = cargo;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Item Id={Id} Name={Name} Description={Description} Icon={Icon} Prototype={Prototype} Level={Level} EquipLevel={EquipLevel} ModifiableCount={ModifiableCount} Size={Size} Durability={Durability} ColorR={ColorR} ColorG={ColorG} ColorB={ColorB} FounderId={FounderId} FounderCategory={FounderCategory} Tags={Tags} TagModifications={TagModifications} Performance={Performance} Cargo={Cargo}>";
	}
}
