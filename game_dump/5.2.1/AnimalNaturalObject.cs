public class AnimalNaturalObject : NaturalPrefabObject
{
	[ExposedInEditor(null)]
	protected override void OnSetEntity()
	{
		AnimalBehavior component = GetComponent<AnimalBehavior>();
		if (component != null)
		{
			component.SetAsDead();
		}
	}

	protected override void OnUpdateEntityId()
	{
		AnimalBehavior component = GetComponent<AnimalBehavior>();
		if (component != null)
		{
			component.EntityId = base.EntityId;
		}
	}
}
