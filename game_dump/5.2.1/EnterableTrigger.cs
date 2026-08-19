using UnityEngine;

public class EnterableTrigger : MonoBehaviour
{
	[SerializeField]
	private EnterableArtifact.TriggerFlag _type;

	private void OnTriggerEnter(Collider other)
	{
		if (CanBeTriggered(other))
		{
			GetEnterable()?.OnTriggerEnter(_type);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (CanBeTriggered(other))
		{
			GetEnterable()?.OnTriggerExit(_type);
		}
	}

	private EnterableArtifact GetEnterable()
	{
		Artifact componentInParent = GetComponentInParent<Artifact>();
		if (componentInParent != null)
		{
			return componentInParent.GetArtifactComponent<EnterableArtifact>();
		}
		return null;
	}

	private static bool CanBeTriggered(Collider other)
	{
		PlayerBehavior component = other.GetComponent<PlayerBehavior>();
		if (component != null)
		{
			return component.IsLocalPlayer;
		}
		return false;
	}
}
