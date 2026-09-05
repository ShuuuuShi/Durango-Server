using MsgPack;

namespace Messages;

public struct PartyInfo
{
	public RadioId LeaderRadioId;

	public PartierStatus LeaderStatus;

	public Pair<PartierStatus, bool>[] MemberStatus;

	public static void Pack(Packer packer, PartyInfo val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		RadioId.Pack(packer, val.LeaderRadioId);
		PartierStatus.Pack(packer, val.LeaderStatus);
		if (val.MemberStatus == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.MemberStatus.Length);
		for (int i = 0; i < val.MemberStatus.Length; i++)
		{
			packer.PackArrayHeader(2);
			PartierStatus.Pack(packer, val.MemberStatus[i].Item1);
			packer.Pack(val.MemberStatus[i].Item2);
		}
	}

	public static PartyInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PartyInfo result = default(PartyInfo);
		result.LeaderRadioId = RadioId.Unpack(unpacker);
		unpacker.Read();
		result.LeaderStatus = PartierStatus.Unpack(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.MemberStatus = new Pair<PartierStatus, bool>[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			unpacker.Read();
			PartierStatus item = PartierStatus.Unpack(unpacker);
			unpacker.Read();
			bool item2 = unpacker.LastReadData.AsBoolean();
			ref Pair<PartierStatus, bool> reference = ref result.MemberStatus[i];
			reference = new Pair<PartierStatus, bool>(item, item2);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<PartyInfo LeaderRadioId={LeaderRadioId} LeaderStatus={LeaderStatus} MemberStatus={MemberStatus}>";
	}
}
