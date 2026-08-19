using MsgPack;

namespace Messages;

public struct SkillCategoryResearching
{
	public double? StartedAt;

	public double? EndsAt;

	public float? SavedTime;

	public Gauge SkipCost;

	public static void Pack(Packer packer, SkillCategoryResearching val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (!val.StartedAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.StartedAt.Value);
		}
		if (!val.EndsAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.EndsAt.Value);
		}
		if (!val.SavedTime.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.SavedTime.Value);
		}
		if (val.SkipCost == null)
		{
			packer.PackNull();
		}
		else
		{
			Gauge.PackTo(val.SkipCost, packer);
		}
	}

	public static SkillCategoryResearching Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		SkillCategoryResearching result = default(SkillCategoryResearching);
		if (unpacker.LastReadData.IsNil)
		{
			result.StartedAt = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.StartedAt = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.EndsAt = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.EndsAt = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SavedTime = null;
		}
		else
		{
			float value3 = unpacker.LastReadData.AsSingle();
			result.SavedTime = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.SkipCost = null;
		}
		else
		{
			Gauge skipCost = Gauge.UnpackFrom(unpacker);
			result.SkipCost = skipCost;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SkillCategoryResearching StartedAt={StartedAt} EndsAt={EndsAt} SavedTime={SavedTime} SkipCost={SkipCost}>";
	}
}
