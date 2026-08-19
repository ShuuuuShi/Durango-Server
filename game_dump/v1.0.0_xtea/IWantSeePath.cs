using UnityEngine;

public class IWantSeePath : MonoBehaviour
{
	private PathMovable _target;

	private void OnEnable()
	{
		CharacterBehavior component = ((Component)this).GetComponent<CharacterBehavior>();
		if ((Object)(object)component != (Object)null)
		{
			AnimalBehavior animalBehavior = component as AnimalBehavior;
			if ((Object)(object)animalBehavior != (Object)null)
			{
				_target = animalBehavior.PathMovable;
			}
			else
			{
				PlayerBehavior playerBehavior = component as PlayerBehavior;
				_target = ((!Object.op_Implicit((Object)(object)playerBehavior)) ? null : playerBehavior.PathMovable);
			}
		}
		if (_target != null)
		{
			KSingleton<PathDrawer>.Instance().DrawPath(_target);
		}
	}

	private void OnDisable()
	{
		if (_target != null)
		{
			KSingleton<PathDrawer>.Instance().StopDraw(_target);
		}
	}
}
