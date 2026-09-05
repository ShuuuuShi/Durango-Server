using MsgPack;

namespace Messages;

public struct Crafting
{
	public string Id;

	public string RecipeId;

	public double Since;

	public float Duration;

	public Gauge SkipCost;

	public static void Pack(Packer packer, Crafting val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
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
		Gauge.PackTo(val.SkipCost, packer);
	}

	public static Crafting Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Crafting result = default(Crafting);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.RecipeId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Since = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Duration = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.SkipCost = Gauge.UnpackFrom(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<Crafting Id={Id} RecipeId={RecipeId} Since={Since} Duration={Duration} SkipCost={SkipCost}>";
	}
}
