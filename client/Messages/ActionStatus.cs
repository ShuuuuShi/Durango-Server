using MsgPack;

namespace Messages;

public struct ActionStatus
{
	public string Id;

	public int Stamina;

	public float Cooltime;

	public static void Pack(Packer packer, ActionStatus val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.Id == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Id);
		}
		packer.Pack(val.Stamina);
		packer.Pack(val.Cooltime);
	}

	public static ActionStatus Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ActionStatus result = default(ActionStatus);
		result.Id = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Stamina = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.Cooltime = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<ActionStatus Id={Id} Stamina={Stamina} Cooltime={Cooltime}>";
	}
}
