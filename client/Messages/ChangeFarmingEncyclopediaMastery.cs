using MsgPack;

namespace Messages;

public struct ChangeFarmingEncyclopediaMastery
{
	public const uint TypeCode = 37128u;

	public string SeedPrototypeId;

	public int TargetLevel;

	public int TargetMasteryIndex;

	public bool IsSelectOrSwap;

	public static void Pack(Packer packer, ChangeFarmingEncyclopediaMastery val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(37128u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.SeedPrototypeId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SeedPrototypeId);
		}
		packer.Pack(val.TargetLevel);
		packer.Pack(val.TargetMasteryIndex);
		packer.Pack(val.IsSelectOrSwap);
	}

	public static ChangeFarmingEncyclopediaMastery Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ChangeFarmingEncyclopediaMastery result = default(ChangeFarmingEncyclopediaMastery);
		result.SeedPrototypeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.TargetLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.TargetMasteryIndex = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.IsSelectOrSwap = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<ChangeFarmingEncyclopediaMastery SeedPrototypeId={SeedPrototypeId} TargetLevel={TargetLevel} TargetMasteryIndex={TargetMasteryIndex} IsSelectOrSwap={IsSelectOrSwap}>";
	}
}
