using System.Collections.Generic;
using MsgPack;

namespace Messages;

public struct PioneerGradeInfo
{
	public const uint TypeCode = 812234573u;

	public string EntityId;

	public int Grade;

	public float Point;

	public int PointNeeded;

	public Dictionary<float, float> DailyExchangedPoints;

	public int CurrentMaximumEstateSize;

	public int CurrentAccessLevel;

	public float? PointAdded;

	public double? PaymentEndsAt;

	public static void Pack(Packer packer, PioneerGradeInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(10);
			packer.Pack(812234573u);
		}
		else
		{
			packer.PackArrayHeader(9);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.Grade);
		packer.Pack(val.Point);
		packer.Pack(val.PointNeeded);
		if (val.DailyExchangedPoints == null)
		{
			packer.PackMapHeader(0);
		}
		else
		{
			packer.PackMapHeader(val.DailyExchangedPoints.Count);
			foreach (KeyValuePair<float, float> dailyExchangedPoint in val.DailyExchangedPoints)
			{
				packer.Pack(dailyExchangedPoint.Key);
				packer.Pack(dailyExchangedPoint.Value);
			}
		}
		packer.Pack(val.CurrentMaximumEstateSize);
		packer.Pack(val.CurrentAccessLevel);
		if (!val.PointAdded.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PointAdded.Value);
		}
		if (!val.PaymentEndsAt.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.PaymentEndsAt.Value);
		}
	}

	public static PioneerGradeInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PioneerGradeInfo result = default(PioneerGradeInfo);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Grade = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Point = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		result.PointNeeded = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.DailyExchangedPoints = new Dictionary<float, float>(num);
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			float key = unpacker.LastReadData.AsSingle();
			unpacker.Read();
			float value = unpacker.LastReadData.AsSingle();
			result.DailyExchangedPoints.Add(key, value);
		}
		unpacker.Read();
		result.CurrentMaximumEstateSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.CurrentAccessLevel = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PointAdded = null;
		}
		else
		{
			float value2 = unpacker.LastReadData.AsSingle();
			result.PointAdded = value2;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.PaymentEndsAt = null;
		}
		else
		{
			double value3 = unpacker.LastReadData.AsDouble();
			result.PaymentEndsAt = value3;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PioneerGradeInfo EntityId={EntityId} Grade={Grade} Point={Point} PointNeeded={PointNeeded} DailyExchangedPoints={DailyExchangedPoints} CurrentMaximumEstateSize={CurrentMaximumEstateSize} CurrentAccessLevel={CurrentAccessLevel} PointAdded={PointAdded} PaymentEndsAt={PaymentEndsAt}>";
	}
}
