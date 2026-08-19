using MsgPack;

namespace Messages;

public struct Crafting
{
	public ulong Id;

	public string RecipeId;

	public double Since;

	public float Duration;

	public static void Pack(Packer packer, Crafting val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		packer.Pack(val.Id);
		if (val.RecipeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RecipeId);
		}
		packer.Pack(val.Since);
		packer.Pack(val.Duration);
	}

	public static Crafting Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Crafting result = default(Crafting);
		result.Id = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.RecipeId = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Since = ((MessagePackObject)(ref lastReadData3)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Duration = ((MessagePackObject)(ref lastReadData4)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Crafting Id={Id} RecipeId={RecipeId} Since={Since} Duration={Duration}>";
	}
}
