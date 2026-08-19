using Messages;

public class ExpectedResultInfo : IExpectedResultInfo
{
	public bool IsValid => Id != null;

	public string Id { get; private set; }

	public int Level { get; private set; }

	public string Name { get; private set; }

	public float DurabilityCurrent { get; private set; }

	public float DurabilityMax { get; private set; }

	public int ModifiableCount { get; private set; }

	public float SuccessRate { get; private set; }

	public void Clear()
	{
		Id = null;
	}

	public void Refresh(CraftEstimation estimation)
	{
		Id = estimation.PrototypeId;
		Level = estimation.Level;
		Name = estimation.Name;
		DurabilityCurrent = estimation.Durability.x;
		DurabilityMax = estimation.Durability.y;
		ModifiableCount = estimation.ModifiableCount;
		SuccessRate = estimation.SuccessRate;
	}

	public void Refresh(string id, BuildEstimation estimation)
	{
		Id = id;
		Level = estimation.Level;
		DurabilityMax = estimation.Durability;
	}
}
