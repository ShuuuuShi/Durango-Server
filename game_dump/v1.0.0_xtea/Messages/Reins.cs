using MsgPack;

namespace Messages;

public struct Reins
{
	public const uint TypeCode = 801u;

	public ushort Capacity;

	public Item[] Contents;

	public ushort VehicleEntityType;

	public string PetName;

	public byte Size;

	public Gauge Hungry;

	public string[] EatableTags;

	public static void Pack(Packer packer, Reins val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(801u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		packer.Pack(val.Capacity);
		if (val.Contents == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Contents.Length);
			for (int i = 0; i < val.Contents.Length; i++)
			{
				Item.Pack(packer, val.Contents[i]);
			}
		}
		packer.Pack(val.VehicleEntityType);
		if (val.PetName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.PetName);
		}
		packer.Pack(val.Size);
		Gauge.PackTo(val.Hungry, packer);
		if (val.EatableTags == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.EatableTags.Length);
		for (int j = 0; j < val.EatableTags.Length; j++)
		{
			if (val.EatableTags[j] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.EatableTags[j]);
			}
		}
	}

	public static Reins Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Reins result = default(Reins);
		result.Capacity = ((MessagePackObject)(ref lastReadData)).AsUInt16();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Contents = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Contents[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.VehicleEntityType = ((MessagePackObject)(ref lastReadData3)).AsUInt16();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData4)).IsNil)
		{
			result.PetName = null;
		}
		else
		{
			string petName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.PetName = petName;
		}
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.Size = ((MessagePackObject)(ref lastReadData5)).AsByte();
		unpacker.Read();
		result.Hungry = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		result.EatableTags = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			string[] eatableTags = result.EatableTags;
			int num3 = j;
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			eatableTags[num3] = ((MessagePackObject)(ref lastReadData7)).AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Reins Capacity={Capacity} Contents={Contents} VehicleEntityType={VehicleEntityType} PetName={PetName} Size={Size} Hungry={Hungry} EatableTags={EatableTags}>";
	}
}
