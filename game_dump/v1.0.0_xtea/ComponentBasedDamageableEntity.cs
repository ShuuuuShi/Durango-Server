using UnityEngine;

public abstract class ComponentBasedDamageableEntity<T> : DamageableEntity where T : MonoBehaviour
{
	public T OwnerComponent { get; private set; }

	protected ComponentBasedDamageableEntity(T component)
		: base(((Component)component).gameObject)
	{
		OwnerComponent = component;
	}
}
