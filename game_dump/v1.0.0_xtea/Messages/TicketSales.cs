using MsgPack;

namespace Messages;

public struct TicketSales
{
	public const uint TypeCode = 2131u;

	public int Tier;

	public int Score;

	public int Subscore;

	public int Round;

	public int RemainedScore;

	public bool Reissuable;

	public Ticket[] Tickets;

	public static void Pack(Packer packer, TicketSales val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(8);
			packer.Pack(2131u);
		}
		else
		{
			packer.PackArrayHeader(7);
		}
		packer.Pack(val.Tier);
		packer.Pack(val.Score);
		packer.Pack(val.Subscore);
		packer.Pack(val.Round);
		packer.Pack(val.RemainedScore);
		packer.Pack(val.Reissuable);
		if (val.Tickets == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Tickets.Length);
		for (int i = 0; i < val.Tickets.Length; i++)
		{
			Ticket.Pack(packer, val.Tickets[i]);
		}
	}

	public static TicketSales Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		TicketSales result = default(TicketSales);
		result.Tier = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Score = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Subscore = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.Round = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.RemainedScore = ((MessagePackObject)(ref lastReadData5)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		result.Reissuable = ((MessagePackObject)(ref lastReadData6)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData7)).AsInt32();
		result.Tickets = new Ticket[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Ticket reference = ref result.Tickets[i];
			reference = Ticket.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TicketSales Tier={Tier} Score={Score} Subscore={Subscore} Round={Round} RemainedScore={RemainedScore} Reissuable={Reissuable} Tickets={Tickets}>";
	}
}
