using MsgPack;

namespace Messages;

public struct Postprocess
{
	public double StartedAt;

	public double EndsAt;

	public string[] Helpers;

	public int MaxHelperCount;

	public string RemodelSlotId;

	public static void Pack(Packer packer, Postprocess val, bool hint = false)
	{
		packer.PackArrayHeader(5);
		packer.Pack(val.StartedAt);
		packer.Pack(val.EndsAt);
		if (val.Helpers == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Helpers.Length);
			for (int i = 0; i < val.Helpers.Length; i++)
			{
				if (val.Helpers[i] == null)
				{
					packer.PackString(string.Empty);
				}
				else
				{
					packer.PackString(val.Helpers[i]);
				}
			}
		}
		packer.Pack(val.MaxHelperCount);
		if (val.RemodelSlotId == null)
		{
			packer.PackNull();
		}
		else if (val.RemodelSlotId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.RemodelSlotId);
		}
	}

	public static Postprocess Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Postprocess result = default(Postprocess);
		result.StartedAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.EndsAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Helpers = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.Helpers[i] = unpacker.LastReadData.AsString();
		}
		unpacker.Read();
		result.MaxHelperCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RemodelSlotId = null;
		}
		else
		{
			string remodelSlotId = unpacker.LastReadData.AsString();
			result.RemodelSlotId = remodelSlotId;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Postprocess StartedAt={StartedAt} EndsAt={EndsAt} Helpers={Helpers} MaxHelperCount={MaxHelperCount} RemodelSlotId={RemodelSlotId}>";
	}
}
