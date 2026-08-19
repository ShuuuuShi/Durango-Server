using MsgPack;

namespace Messages;

public struct Item
{
	public string Id;

	public string Name;

	public string Description;

	public string Icon;

	public string SubIcon;

	public string Prototype;

	public int Level;

	public int OriginalLevel;

	public int ModifiableCount;

	public int ModifiedCount;

	public int Size;

	public Gauge Durability;

	public string ColorR;

	public string ColorG;

	public string ColorB;

	public bool Unstable;

	public RepairRequirement? RepairRequirement;

	public string FounderId;

	public string FounderCategory;

	public Tag[] Tags;

	public Tag[] TagModifications;

	public Performance[] Performance;

	public object Ext;

	public string CollectibleId;

	public string GeneratorId;

	public string[] EmotionalMotions;

	public float PioneerCost;

	public bool Tradable;

	public ReformSlot[] ReformSlots;

	public static void Pack(Packer packer, Item val, bool hint = false)
	{
		packer.PackArrayHeader(29);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
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
		if (val.SubIcon == null)
		{
			packer.PackNull();
		}
		else if (val.SubIcon == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SubIcon);
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
		packer.Pack(val.OriginalLevel);
		packer.Pack(val.ModifiableCount);
		packer.Pack(val.ModifiedCount);
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
		packer.Pack(val.Unstable);
		if (!val.RepairRequirement.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.RepairRequirement.Pack(packer, val.RepairRequirement.Value);
		}
		if (val.FounderId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.FounderId);
		}
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
		if (val.Ext == null)
		{
			packer.PackNull();
		}
		else if (val.Ext is DeodorantItem)
		{
			DeodorantItem.Pack(packer, (DeodorantItem)val.Ext, hint: true);
		}
		else if (val.Ext is Container)
		{
			Container.Pack(packer, (Container)val.Ext, hint: true);
		}
		else if (val.Ext is Reins)
		{
			Reins.Pack(packer, (Reins)val.Ext, hint: true);
		}
		else if (val.Ext is LootBoxItem)
		{
			LootBoxItem.Pack(packer, (LootBoxItem)val.Ext, hint: true);
		}
		else if (val.Ext is ArtifactCapsule)
		{
			ArtifactCapsule.Pack(packer, (ArtifactCapsule)val.Ext, hint: true);
		}
		else if (val.Ext is BlueprintItem)
		{
			BlueprintItem.Pack(packer, (BlueprintItem)val.Ext, hint: true);
		}
		else if (val.Ext is ArtifactPackage)
		{
			ArtifactPackage.Pack(packer, (ArtifactPackage)val.Ext, hint: true);
		}
		if (val.CollectibleId == null)
		{
			packer.PackNull();
		}
		else if (val.CollectibleId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CollectibleId);
		}
		if (val.GeneratorId == null)
		{
			packer.PackNull();
		}
		else if (val.GeneratorId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.GeneratorId);
		}
		if (val.EmotionalMotions == null)
		{
			packer.PackNull();
		}
		else if (val.EmotionalMotions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.EmotionalMotions.Length);
			for (int l = 0; l < val.EmotionalMotions.Length; l++)
			{
				if (val.EmotionalMotions[l] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.EmotionalMotions[l]);
				}
			}
		}
		packer.Pack(val.PioneerCost);
		packer.Pack(val.Tradable);
		if (val.ReformSlots == null)
		{
			packer.PackNull();
			return;
		}
		if (val.ReformSlots == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ReformSlots.Length);
		for (int m = 0; m < val.ReformSlots.Length; m++)
		{
			ReformSlot.Pack(packer, val.ReformSlots[m]);
		}
	}

	public static Item Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Item result = default(Item);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Description = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Icon = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SubIcon = null;
		}
		else
		{
			string subIcon = unpacker.LastReadData.AsString();
			result.SubIcon = subIcon;
		}
		unpacker.Read();
		result.Prototype = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.OriginalLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ModifiableCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ModifiedCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Size = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Durability = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ColorR = null;
		}
		else
		{
			string colorR = unpacker.LastReadData.AsString();
			result.ColorR = colorR;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ColorG = null;
		}
		else
		{
			string colorG = unpacker.LastReadData.AsString();
			result.ColorG = colorG;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ColorB = null;
		}
		else
		{
			string colorB = unpacker.LastReadData.AsString();
			result.ColorB = colorB;
		}
		unpacker.Read();
		result.Unstable = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RepairRequirement = null;
		}
		else
		{
			RepairRequirement value = Messages.RepairRequirement.Unpack(unpacker);
			result.RepairRequirement = value;
		}
		unpacker.Read();
		result.FounderId = unpacker.LastReadData.AsString();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.FounderCategory = null;
		}
		else
		{
			string founderCategory = unpacker.LastReadData.AsString();
			result.FounderCategory = founderCategory;
		}
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Tags = new Tag[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Tag reference = ref result.Tags[i];
			reference = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.TagModifications = new Tag[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Tag reference2 = ref result.TagModifications[j];
			reference2 = Tag.Unpack(unpacker);
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.Performance = new Performance[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			ref Performance reference3 = ref result.Performance[k];
			reference3 = Messages.Performance.Unpack(unpacker);
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Ext = null;
		}
		else
		{
			object ext = null;
			if (unpacker.ReadUInt32(out var result2))
			{
				switch (result2)
				{
				case 55555u:
					ext = DeodorantItem.Unpack(unpacker);
					break;
				case 2058u:
					ext = Container.Unpack(unpacker);
					break;
				case 801u:
					ext = Reins.Unpack(unpacker);
					break;
				case 29875325u:
					ext = LootBoxItem.Unpack(unpacker);
					break;
				case 3550u:
					ext = ArtifactCapsule.Unpack(unpacker);
					break;
				case 19875324u:
					ext = BlueprintItem.Unpack(unpacker);
					break;
				case 3694u:
					ext = ArtifactPackage.Unpack(unpacker);
					break;
				default:
					Debug.LogError("Unexpected type code: " + result2);
					break;
				}
			}
			result.Ext = ext;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CollectibleId = null;
		}
		else
		{
			string collectibleId = unpacker.LastReadData.AsString();
			result.CollectibleId = collectibleId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.GeneratorId = null;
		}
		else
		{
			string generatorId = unpacker.LastReadData.AsString();
			result.GeneratorId = generatorId;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EmotionalMotions = null;
		}
		else
		{
			int num4 = unpacker.LastReadData.AsInt32();
			string[] array = new string[num4];
			for (int l = 0; l < num4; l++)
			{
				unpacker.Read();
				array[l] = unpacker.LastReadData.AsString();
			}
			result.EmotionalMotions = array;
		}
		unpacker.Read();
		result.PioneerCost = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.Tradable = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ReformSlots = null;
		}
		else
		{
			int num5 = unpacker.LastReadData.AsInt32();
			ReformSlot[] array2 = new ReformSlot[num5];
			for (int m = 0; m < num5; m++)
			{
				unpacker.Read();
				ref ReformSlot reference4 = ref array2[m];
				reference4 = ReformSlot.Unpack(unpacker);
			}
			result.ReformSlots = array2;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Item Id={Id} Name={Name} Description={Description} Icon={Icon} SubIcon={SubIcon} Prototype={Prototype} Level={Level} OriginalLevel={OriginalLevel} ModifiableCount={ModifiableCount} ModifiedCount={ModifiedCount} Size={Size} Durability={Durability} ColorR={ColorR} ColorG={ColorG} ColorB={ColorB} Unstable={Unstable} RepairRequirement={RepairRequirement} FounderId={FounderId} FounderCategory={FounderCategory} Tags={Tags} TagModifications={TagModifications} Performance={Performance} Ext={Ext} CollectibleId={CollectibleId} GeneratorId={GeneratorId} EmotionalMotions={EmotionalMotions} PioneerCost={PioneerCost} Tradable={Tradable} ReformSlots={ReformSlots}>";
	}
}
