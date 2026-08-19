using MsgPack;
using Shared.Building;

namespace Messages;

public struct ArtifactState
{
	public const uint TypeCode = 323u;

	public string EntityId;

	public Gauge Durability;

	public BuildingState BuildingState;

	public Gauge RepairImmediateCost;

	public Pair<double, double>? Repairement;

	public Postprocess? Postprocess;

	public bool GateOpened;

	public ScribbleContent? Scribble;

	public Trap? Trap;

	public Farming? Farming;

	public Home? Home;

	public object Cage;

	public DomesticCage? DomesticCage;

	public Crack? Crack;

	public StoneCrack? StoneCrack;

	public Effector? Effector;

	public InventoryState? Inventory;

	public ArtifactStats? Stats;

	public ArtifactSet? InteriorSet;

	public ArtifactMood? InteriorMood;

	public ArtifactAccess? Access;

	public CatapultState? Catapult;

	public DefensiveState? Defensive;

	public byte Level;

	public float MaxHealth;

	public string ChangedName;

	public SprinklerState? Sprinkler;

	public WarpAccelerator? Warpaccelerator;

	public Bandstand? Bandstand;

	public static void Pack(Packer packer, ArtifactState val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(30);
			packer.Pack(323u);
		}
		else
		{
			packer.PackArrayHeader(29);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		Gauge.PackTo(val.Durability, packer);
		packer.Pack((int)val.BuildingState);
		if (val.RepairImmediateCost == null)
		{
			packer.PackNull();
		}
		else
		{
			Gauge.PackTo(val.RepairImmediateCost, packer);
		}
		if (!val.Repairement.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			packer.PackArrayHeader(2);
			packer.Pack(val.Repairement.Value.Item1);
			packer.Pack(val.Repairement.Value.Item2);
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
		if (val.Cage == null)
		{
			packer.PackNull();
		}
		else if (val.Cage is Cage)
		{
			Messages.Cage.Pack(packer, (Cage)val.Cage, hint: true);
		}
		else if (val.Cage is GrowCage)
		{
			GrowCage.Pack(packer, (GrowCage)val.Cage, hint: true);
		}
		if (!val.DomesticCage.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.DomesticCage.Pack(packer, val.DomesticCage.Value);
		}
		if (!val.Crack.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Crack.Pack(packer, val.Crack.Value);
		}
		if (!val.StoneCrack.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.StoneCrack.Pack(packer, val.StoneCrack.Value);
		}
		if (!val.Effector.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Effector.Pack(packer, val.Effector.Value);
		}
		if (!val.Inventory.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			InventoryState.Pack(packer, val.Inventory.Value);
		}
		if (!val.Stats.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ArtifactStats.Pack(packer, val.Stats.Value);
		}
		if (!val.InteriorSet.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ArtifactSet.Pack(packer, val.InteriorSet.Value);
		}
		if (!val.InteriorMood.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ArtifactMood.Pack(packer, val.InteriorMood.Value);
		}
		if (!val.Access.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			ArtifactAccess.Pack(packer, val.Access.Value);
		}
		if (!val.Catapult.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			CatapultState.Pack(packer, val.Catapult.Value);
		}
		if (!val.Defensive.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			DefensiveState.Pack(packer, val.Defensive.Value);
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
		if (!val.Sprinkler.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			SprinklerState.Pack(packer, val.Sprinkler.Value);
		}
		if (!val.Warpaccelerator.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			WarpAccelerator.Pack(packer, val.Warpaccelerator.Value);
		}
		if (!val.Bandstand.HasValue)
		{
			packer.PackNull();
		}
		else
		{
			Messages.Bandstand.Pack(packer, val.Bandstand.Value);
		}
	}

	public static ArtifactState Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ArtifactState result = default(ArtifactState);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.Durability = Gauge.UnpackFrom(unpacker);
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 3 < num)
		{
			result.BuildingState = BuildingState.Invalid;
		}
		else
		{
			result.BuildingState = (BuildingState)num;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.RepairImmediateCost = null;
		}
		else
		{
			Gauge repairImmediateCost = Gauge.UnpackFrom(unpacker);
			result.RepairImmediateCost = repairImmediateCost;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Repairement = null;
		}
		else
		{
			unpacker.Read();
			double item = unpacker.LastReadData.AsDouble();
			unpacker.Read();
			double item2 = unpacker.LastReadData.AsDouble();
			Pair<double, double> value = new Pair<double, double>(item, item2);
			result.Repairement = value;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Postprocess = null;
		}
		else
		{
			Postprocess value2 = Messages.Postprocess.Unpack(unpacker);
			result.Postprocess = value2;
		}
		unpacker.Read();
		result.GateOpened = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Scribble = null;
		}
		else
		{
			ScribbleContent value3 = ScribbleContent.Unpack(unpacker);
			result.Scribble = value3;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Trap = null;
		}
		else
		{
			Trap value4 = Messages.Trap.Unpack(unpacker);
			result.Trap = value4;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Farming = null;
		}
		else
		{
			Farming value5 = Messages.Farming.Unpack(unpacker);
			result.Farming = value5;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Home = null;
		}
		else
		{
			Home value6 = Messages.Home.Unpack(unpacker);
			result.Home = value6;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Cage = null;
		}
		else
		{
			object cage = null;
			if (unpacker.ReadUInt32(out var result2))
			{
				switch (result2)
				{
				case 811u:
					cage = Messages.Cage.Unpack(unpacker);
					break;
				case 65100u:
					cage = GrowCage.Unpack(unpacker);
					break;
				default:
					Debug.LogError("Unexpected type code: " + result2);
					break;
				}
			}
			result.Cage = cage;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.DomesticCage = null;
		}
		else
		{
			DomesticCage value7 = Messages.DomesticCage.Unpack(unpacker);
			result.DomesticCage = value7;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Crack = null;
		}
		else
		{
			Crack value8 = Messages.Crack.Unpack(unpacker);
			result.Crack = value8;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.StoneCrack = null;
		}
		else
		{
			StoneCrack value9 = Messages.StoneCrack.Unpack(unpacker);
			result.StoneCrack = value9;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Effector = null;
		}
		else
		{
			Effector value10 = Messages.Effector.Unpack(unpacker);
			result.Effector = value10;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Inventory = null;
		}
		else
		{
			InventoryState value11 = InventoryState.Unpack(unpacker);
			result.Inventory = value11;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Stats = null;
		}
		else
		{
			ArtifactStats value12 = ArtifactStats.Unpack(unpacker);
			result.Stats = value12;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.InteriorSet = null;
		}
		else
		{
			ArtifactSet value13 = ArtifactSet.Unpack(unpacker);
			result.InteriorSet = value13;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.InteriorMood = null;
		}
		else
		{
			ArtifactMood value14 = ArtifactMood.Unpack(unpacker);
			result.InteriorMood = value14;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Access = null;
		}
		else
		{
			ArtifactAccess value15 = ArtifactAccess.Unpack(unpacker);
			result.Access = value15;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Catapult = null;
		}
		else
		{
			CatapultState value16 = CatapultState.Unpack(unpacker);
			result.Catapult = value16;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Defensive = null;
		}
		else
		{
			DefensiveState value17 = DefensiveState.Unpack(unpacker);
			result.Defensive = value17;
		}
		unpacker.Read();
		result.Level = unpacker.LastReadData.AsByte();
		unpacker.Read();
		result.MaxHealth = unpacker.LastReadData.AsSingle();
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.ChangedName = null;
		}
		else
		{
			string changedName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
			result.ChangedName = changedName;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Sprinkler = null;
		}
		else
		{
			SprinklerState value18 = SprinklerState.Unpack(unpacker);
			result.Sprinkler = value18;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Warpaccelerator = null;
		}
		else
		{
			WarpAccelerator value19 = WarpAccelerator.Unpack(unpacker);
			result.Warpaccelerator = value19;
		}
		unpacker.Read();
		if (unpacker.LastReadData.IsNil)
		{
			result.Bandstand = null;
		}
		else
		{
			Bandstand value20 = Messages.Bandstand.Unpack(unpacker);
			result.Bandstand = value20;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<ArtifactState EntityId={EntityId} Durability={Durability} BuildingState={BuildingState} RepairImmediateCost={RepairImmediateCost} Repairement={Repairement} Postprocess={Postprocess} GateOpened={GateOpened} Scribble={Scribble} Trap={Trap} Farming={Farming} Home={Home} Cage={Cage} DomesticCage={DomesticCage} Crack={Crack} StoneCrack={StoneCrack} Effector={Effector} Inventory={Inventory} Stats={Stats} InteriorSet={InteriorSet} InteriorMood={InteriorMood} Access={Access} Catapult={Catapult} Defensive={Defensive} Level={Level} MaxHealth={MaxHealth} ChangedName={ChangedName} Sprinkler={Sprinkler} Warpaccelerator={Warpaccelerator} Bandstand={Bandstand}>";
	}
}
