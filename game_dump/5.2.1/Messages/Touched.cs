using MsgPack;

namespace Messages;

public struct Touched
{
	public const uint TypeCode = 2020u;

	public string EntityId;

	public string EntityName;

	public string PrototypeId;

	public int Level;

	public int[] Interactions;

	public int[] DisabledInteractions;

	public int[] AccessDeniedInteractions;

	public Collectible Collectible;

	public Workbench? Workbench;

	public Secured? Secured;

	public ReactingProp? ReactingProp;

	public Mannequin? Mannequin;

	public static void Pack(Packer packer, Touched val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(13);
			packer.Pack(2020u);
		}
		else
		{
			packer.PackArrayHeader(12);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
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
		if (!val.Secured.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Secured.Pack(packer, val.Secured.Value);
		}
		if (!val.ReactingProp.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.ReactingProp.Pack(packer, val.ReactingProp.Value);
		}
		if (!val.Mannequin.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Mannequin.Pack(packer, val.Mannequin.Value);
		}
	}

	public static Touched Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Touched result = default(Touched);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.PrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Interactions = new int[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Interactions[i] = unpacker.LastReadData.AsInt32();
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.DisabledInteractions = new int[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.DisabledInteractions[j] = unpacker.LastReadData.AsInt32();
		}
		unpacker.Read();
		int num3 = unpacker.LastReadData.AsInt32();
		result.AccessDeniedInteractions = new int[num3];
		for (int k = 0; k < num3; k++)
		{
			unpacker.Read();
			result.AccessDeniedInteractions[k] = unpacker.LastReadData.AsInt32();
		}
		unpacker.Read();
		result.Collectible = Collectible.Unpack(unpacker);
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Workbench = null;
		}
		else
		{
			Workbench value = Messages.Workbench.Unpack(unpacker);
			result.Workbench = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Secured = null;
		}
		else
		{
			Secured value2 = Messages.Secured.Unpack(unpacker);
			result.Secured = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ReactingProp = null;
		}
		else
		{
			ReactingProp value3 = Messages.ReactingProp.Unpack(unpacker);
			result.ReactingProp = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Mannequin = null;
		}
		else
		{
			Mannequin value4 = Messages.Mannequin.Unpack(unpacker);
			result.Mannequin = value4;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Touched EntityId={EntityId} EntityName={EntityName} PrototypeId={PrototypeId} Level={Level} Interactions={Interactions} DisabledInteractions={DisabledInteractions} AccessDeniedInteractions={AccessDeniedInteractions} Collectible={Collectible} Workbench={Workbench} Secured={Secured} ReactingProp={ReactingProp} Mannequin={Mannequin}>";
	}
}
