using Durango.UI.Prologue;
using InteractionData;
using UnityEngine;

public abstract class SelectableObject : MonoBehaviour
{
	[SerializeField]
	public bool Selectable = true;

	[SerializeField]
	private string _entityId = "999";

	[SerializeField]
	private float _interactableDistance = 100f;

	public string EntityId => _entityId;

	public float InteractableDistance => _interactableDistance;

	public abstract void InteractionTouched();

	public abstract bool MenuClicked(GameObject target, InteractionMenuData menu);

	public static GameObject FindSelectable(GameObject o)
	{
		if (o == null)
		{
			return null;
		}
		SelectableObject componentInParent = o.GetComponentInParent<SelectableObject>();
		if (componentInParent != null && componentInParent.Selectable)
		{
			return componentInParent.gameObject;
		}
		return null;
	}

	protected static void PlayMotion(string motionState, float time, string equipment = null, ItemColor color = default(ItemColor))
	{
		PrologueInteractionButtonGroupBase.ShowInteractionButton("Subject", show: false);
		GameSystem<InputSystem>.Instance().MoveLock = true;
		PlayerController.MotionUpdater.Motion(motionState, time, 1f, forceTransition: false, overrideIdleMotion: false, equipment, color);
	}

	protected static void OnPlayMotionFinished()
	{
		PrologueInteractionButtonGroupBase.ShowInteractionButton("Subject", show: true);
		GameSystem<InputSystem>.Instance().MoveLock = false;
	}

	public virtual string GetName()
	{
		return base.gameObject.name;
	}
}
