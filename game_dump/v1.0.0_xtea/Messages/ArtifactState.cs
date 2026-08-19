using System.Collections.Generic;
using MsgPack;
using Shared.Building;

namespace Messages;

public struct ArtifactState
{
	public const uint TypeCode = 323u;

	public ulong EntityId;

	public Gauge Durability;

	public BuildingState BuildingState;

	public KeyValuePair<double, double>? Repairement;

	public Postprocess? Postprocess;

	public bool GateOpened;

	public ScribbleContent? Scribble;

	public Trap? Trap;

	public Farming? Farming;

	public Home? Home;

	public Cage? Cage;

	public EstateInfo? Estate;

	public Crack? Crack;

	public byte Level;

	public float MaxHealth;

	public string ChangedName;

	public static void Pack(Packer packer, ArtifactState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(17);
			packer.Pack(323u);
		}
		else
		{
			packer.PackArrayHeader(16);
		}
		packer.Pack(val.EntityId);
		Gauge.PackTo(val.Durability, packer);
		packer.Pack((int)val.BuildingState);
		if (!val.Repairement.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.Repairement.Value.Key);
			packer.Pack(val.Repairement.Value.Value);
		}
		if (!val.Postprocess.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Postprocess.Pack(packer, val.Postprocess.Value);
		}
		packer.Pack(val.GateOpened);
		if (!val.Scribble.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ScribbleContent.Pack(packer, val.Scribble.Value);
		}
		if (!val.Trap.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Trap.Pack(packer, val.Trap.Value);
		}
		if (!val.Farming.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Farming.Pack(packer, val.Farming.Value);
		}
		if (!val.Home.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Home.Pack(packer, val.Home.Value);
		}
		if (!val.Cage.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Cage.Pack(packer, val.Cage.Value);
		}
		if (!val.Estate.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			EstateInfo.Pack(packer, val.Estate.Value);
		}
		if (!val.Crack.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Crack.Pack(packer, val.Crack.Value);
		}
		packer.Pack(val.Level);
		packer.Pack(val.MaxHealth);
		if (val.ChangedName == null)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackString(val.ChangedName);
		}
	}

	public static ArtifactState Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0341: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_037e: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		ArtifactState result = default(ArtifactState);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		result.Durability = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		if (num < 0 || 2 < num)
		{
			result.BuildingState = BuildingState.Invalid;
		}
		else
		{
			result.BuildingState = (BuildingState)num;
		}
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData3)).IsNil)
		{
			result.Repairement = null;
		}
		else
		{
			unpacker.Read();
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			double key = ((MessagePackObject)(ref lastReadData4)).AsDouble();
			unpacker.Read();
			MessagePackObject lastReadData5 = unpacker.LastReadData;
			double value = ((MessagePackObject)(ref lastReadData5)).AsDouble();
			KeyValuePair<double, double> value2 = new KeyValuePair<double, double>(key, value);
			result.Repairement = value2;
		}
		unpacker.Read();
		MessagePackObject lastReadData6 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData6)).IsNil)
		{
			result.Postprocess = null;
		}
		else
		{
			Postprocess value3 = Messages.Postprocess.Unpack(unpacker);
			result.Postprocess = value3;
		}
		unpacker.Read();
		MessagePackObject lastReadData7 = unpacker.LastReadData;
		result.GateOpened = ((MessagePackObject)(ref lastReadData7)).AsBoolean();
		unpacker.Read();
		MessagePackObject lastReadData8 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData8)).IsNil)
		{
			result.Scribble = null;
		}
		else
		{
			ScribbleContent value4 = ScribbleContent.Unpack(unpacker);
			result.Scribble = value4;
		}
		unpacker.Read();
		MessagePackObject lastReadData9 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData9)).IsNil)
		{
			result.Trap = null;
		}
		else
		{
			Trap value5 = Messages.Trap.Unpack(unpacker);
			result.Trap = value5;
		}
		unpacker.Read();
		MessagePackObject lastReadData10 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData10)).IsNil)
		{
			result.Farming = null;
		}
		else
		{
			Farming value6 = Messages.Farming.Unpack(unpacker);
			result.Farming = value6;
		}
		unpacker.Read();
		MessagePackObject lastReadData11 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData11)).IsNil)
		{
			result.Home = null;
		}
		else
		{
			Home value7 = Messages.Home.Unpack(unpacker);
			result.Home = value7;
		}
		unpacker.Read();
		MessagePackObject lastReadData12 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData12)).IsNil)
		{
			result.Cage = null;
		}
		else
		{
			Cage value8 = Messages.Cage.Unpack(unpacker);
			result.Cage = value8;
		}
		unpacker.Read();
		MessagePackObject lastReadData13 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData13)).IsNil)
		{
			result.Estate = null;
		}
		else
		{
			EstateInfo value9 = EstateInfo.Unpack(unpacker);
			result.Estate = value9;
		}
		unpacker.Read();
		MessagePackObject lastReadData14 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData14)).IsNil)
		{
			result.Crack = null;
		}
		else
		{
			Crack value10 = Messages.Crack.Unpack(unpacker);
			result.Crack = value10;
		}
		unpacker.Read();
		MessagePackObject lastReadData15 = unpacker.LastReadData;
		result.Level = ((MessagePackObject)(ref lastReadData15)).AsByte();
		unpacker.Read();
		MessagePackObject lastReadData16 = unpacker.LastReadData;
		result.MaxHealth = ((MessagePackObject)(ref lastReadData16)).AsSingle();
		unpacker.Read();
		MessagePackObject lastReadData17 = unpacker.LastReadData;
		if (((MessagePackObject)(ref lastReadData17)).IsNil)
		{
			result.ChangedName = null;
		}
		else
		{
			string changedName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.ChangedName = changedName;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactState EntityId={EntityId} Durability={Durability} BuildingState={BuildingState} Repairement={Repairement} Postprocess={Postprocess} GateOpened={GateOpened} Scribble={Scribble} Trap={Trap} Farming={Farming} Home={Home} Cage={Cage} Estate={Estate} Crack={Crack} Level={Level} MaxHealth={MaxHealth} ChangedName={ChangedName}>";
	}
}
