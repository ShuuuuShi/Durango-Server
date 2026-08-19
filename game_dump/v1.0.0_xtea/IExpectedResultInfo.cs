public interface IExpectedResultInfo
{
	string Id { get; }

	int Level { get; }

	string Name { get; }

	float DurabilityCurrent { get; }

	float DurabilityMax { get; }

	int ModifiableCount { get; }

	float SuccessRate { get; }
}
