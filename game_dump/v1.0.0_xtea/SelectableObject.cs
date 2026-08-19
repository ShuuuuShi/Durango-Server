using System.Runtime.InteropServices;
using InteractionData;
using UnityEngine;

public abstract class SelectableObject : MonoBehaviour
{
	[SerializeField]
	public bool Selectable = true;

	[SerializeField]
	private ulong _entityId = 999uL;

	[SerializeField]
	private float _interactableDistance = 100f;

	public ulong EntityId => _entityId;

	public float InteractableDistance => _interactableDistance;

	public abstract void InteractionTouched();

	public abstract bool MenuClicked(GameObject target, InteractionMenuData menu);

	public static GameObject FindSelectable(GameObject o)
	{
		SelectableObject componentInParent = o.GetComponentInParent<SelectableObject>();
		if ((Object)(object)componentInParent != (Object)null && componentInParent.Selectable)
		{
			return ((Component)componentInParent).gameObject;
		}
		return null;
	}

	public static void PlayMotion(string motionState, float time, string equipment = null, [Optional] ItemColor color)
	{
		ShowInteractionButton(show: false);
		KSingleton<PlayerController>.Instance().Motion(motionState, time, 1f, forceTransition: false, equipment, color);
	}

	protected static void ShowInteractionButton(bool show)
	{
		InteractionGroupHelper.ShowInteractionButtons("Subject", show);
	}

	public virtual string GetName()
	{
		return ((Object)((Component)this).gameObject).name;
	}
}
