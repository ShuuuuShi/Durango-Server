using MsgPack;

namespace Messages;

public struct TodayAttendanceReward
{
	public int RewardNumber;

	public int RestorableDays;

	public bool AppendixRewardable;

	public string Name;

	public string ShortName;

	public double Since;

	public double Until;

	public double NextAttendTime;

	public string Image;

	public string BgImage;

	public static void Pack(Packer packer, TodayAttendanceReward val, bool hint = false)
	{
		packer.PackArrayHeader(10);
		packer.Pack(val.RewardNumber);
		packer.Pack(val.RestorableDays);
		packer.Pack(val.AppendixRewardable);
		packer.PackString(val.Name);
		packer.PackString(val.ShortName);
		packer.Pack(val.Since);
		packer.Pack(val.Until);
		packer.Pack(val.NextAttendTime);
		if (val.Image == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Image);
		}
		if (val.BgImage == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.BgImage);
		}
	}

	public static TodayAttendanceReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TodayAttendanceReward result = default(TodayAttendanceReward);
		result.RewardNumber = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.RestorableDays = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.AppendixRewardable = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		result.Name = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.ShortName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.Since = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Until = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.NextAttendTime = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Image = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.BgImage = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<TodayAttendanceReward RewardNumber={RewardNumber} RestorableDays={RestorableDays} AppendixRewardable={AppendixRewardable} Name={Name} ShortName={ShortName} Since={Since} Until={Until} NextAttendTime={NextAttendTime} Image={Image} BgImage={BgImage}>";
	}
}
