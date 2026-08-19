public class PetDamageableEntity : CharacterDamageableEntity
{
	public PetAI PetAI { get; private set; }

	public PetDamageableEntity(CharacterBehavior component, PetAI petAI)
		: base(component)
	{
		PetAI = petAI;
	}

	public override int GetEntityTypeId()
	{
		return PetAI.AnimalEntityType;
	}
}
