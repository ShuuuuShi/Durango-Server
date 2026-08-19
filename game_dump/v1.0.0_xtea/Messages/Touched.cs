using MsgPack;

namespace Messages;

public struct Touched
{
	public const uint TypeCode = 2020u;

	public ulong EntityId;

	public string EntityName;

	public string PrototypeId;

	public int Level;

	public int[] Interactions;

	public int[] DisabledInteractions;

	public int[] AccessDeniedInteractions;

	public Collectible Collectible;

	public Workbench? Workbench;

	public Dispenser? Dispenser;

	public Secured? Secured;

	public static void Pack(Packer packer, Touched val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(12);
			packer.Pack(2020u);
		}
		else
		{
			packer.PackArrayHeader(11);
		}
		packer.Pack(val.EntityId);
		packer.PackString(val.EntityName);
		if (val.PrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PrototypeId);
		}
		packer.Pack(val.Level);
		if (val.Interactions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Interactions.Length);
			for (int i = 0; i < val.Interactions.Length; i++)
			{
				packer.Pack(val.Interactions[i]);
			}
		}
		if (val.DisabledInteractions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.DisabledInteractions.Length);
			for (int j = 0; j < val.DisabledInteractions.Length; j++)
			{
				packer.Pack(val.DisabledInteractions[j]);
			}
		}
		if (val.AccessDeniedInteractions == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.AccessDeniedInteractions.Length);
			for (int k = 0; k < val.AccessDeniedInteractions.Length; k++)
			{
				packer.Pack(val.AccessDeniedInteractions[k]);
			}
		}
		Collectible.Pack(packer, val.Collectible);
		if (!val.Workbench.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Workbench.Pack(packer, val.Workbench.Value);
		}
		if (!val.Dispenser.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Dispenser.Pack(packer, val.Dispenser.Value);
		}
		if (!val.Secured.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Secured.Pack(packer, val.Secured.Value);
		}
	}

	public static Touched Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0193: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Touched result = default(Touched);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		result.EntityName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.PrototypeId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.Interactions = new int[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			int[] interactions = result.Interactions;
			int num2 = i;
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			interactions[num2] = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		int num3 = ((MessagePackObject)(ref lastReadData6)).AsInt32();
		result.DisabledInteractions = new int[num3];
		for (int j = 0; j < num3; j++)
		{
			unpacker.Read();
			int[] disabledInteractions = result.DisabledInteractions;
			int num4 = j;
			MessagePackObject lastReadData7 = unpacker.LastReadData;
			disabledInteractions[num4] = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		}
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		int num5 = ((MessagePackObject)(ref lastReadData8)).AsInt32();
		result.AccessDeniedInteractions = new int[num5];
		for (int k = 0; k < num5; k++)
		{
			unpacker.Read();
			int[] accessDeniedInteractions = result.AccessDeniedInteractions;
			int num6 = k;
			MessagePackObject lastReadData9 = unpacker.LastReadData;
			accessDeniedInteractions[num6] = ((MessagePackObject)(ref lastReadData9)).AsInt32();
		}
		unpacker.Read();
		result.Collectible = Collectible.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.Workbench = null;
		}
		else
		{
			Workbench value = Messages.Workbench.Unpack(unpacker);
			result.Workbench = value;
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData11)).IsNil)
		{
			result.Dispenser = null;
		}
		else
		{
			Dispenser value2 = Messages.Dispenser.Unpack(unpacker);
			result.Dispenser = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData12)).IsNil)
		{
			result.Secured = null;
		}
		else
		{
			Secured value3 = Messages.Secured.Unpack(unpacker);
			result.Secured = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Touched EntityId={EntityId} EntityName={EntityName} PrototypeId={PrototypeId} Level={Level} Interactions={Interactions} DisabledInteractions={DisabledInteractions} AccessDeniedInteractions={AccessDeniedInteractions} Collectible={Collectible} Workbench={Workbench} Dispenser={Dispenser} Secured={Secured}>";
	}
}
