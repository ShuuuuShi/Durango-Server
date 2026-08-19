using MsgPack;
using Shared.Accelerator;

namespace Messages;

public struct WarpAccelerator
{
	public AcceleratorStatus Status;

	public double? StatusSince;

	public double? StatusUntil;

	public int CurrentPhase;

	public int? CurrentWave;

	public int? CurrentMaxWave;

	public int? RemainAnimals;

	public string[] Participants;

	public static void Pack(Packer packer, WarpAccelerator val, bool hint = false)
	{
		packer.PackArrayHeader(8);
		packer.Pack((int)val.Status);
		if (!val.StatusSince.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.StatusSince.Value);
		}
		if (!val.StatusUntil.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.StatusUntil.Value);
		}
		packer.Pack(val.CurrentPhase);
		if (!val.CurrentWave.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.CurrentWave.Value);
		}
		if (!val.CurrentMaxWave.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.CurrentMaxWave.Value);
		}
		if (!val.RemainAnimals.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.Pack(val.RemainAnimals.Value);
		}
		if (val.Participants == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Participants.Length);
		for (int i = 0; i < val.Participants.Length; i++)
		{
			if (val.Participants[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.Participants[i]);
			}
		}
	}

	public static WarpAccelerator Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		WarpAccelerator result = default(WarpAccelerator);
		if (num < 0 || 5 < num)
		{
			result.Status = AcceleratorStatus.Invalid;
		}
		else
		{
			result.Status = (AcceleratorStatus)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.StatusSince = null;
		}
		else
		{
			double value = unpacker.LastReadData.AsDouble();
			result.StatusSince = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.StatusUntil = null;
		}
		else
		{
			double value2 = unpacker.LastReadData.AsDouble();
			result.StatusUntil = value2;
		}
		unpacker.Read();
		result.CurrentPhase = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CurrentWave = null;
		}
		else
		{
			int value3 = unpacker.LastReadData.AsInt32();
			result.CurrentWave = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.CurrentMaxWave = null;
		}
		else
		{
			int value4 = unpacker.LastReadData.AsInt32();
			result.CurrentMaxWave = value4;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RemainAnimals = null;
		}
		else
		{
			int value5 = unpacker.LastReadData.AsInt32();
			result.RemainAnimals = value5;
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.Participants = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			result.Participants[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<WarpAccelerator Status={Status} StatusSince={StatusSince} StatusUntil={StatusUntil} CurrentPhase={CurrentPhase} CurrentWave={CurrentWave} CurrentMaxWave={CurrentMaxWave} RemainAnimals={RemainAnimals} Participants={Participants}>";
	}
}
