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
		return (!(componentInParent != null)) ? null : componentInParent.GetArtifactComponent<EnterableArtifact>();
	}

	private static bool CanBeTriggered(Collider other)
	{
		PlayerBehavior component = other.GetComponent<PlayerBehavior>();
		return component != null && component.IsLocalPlayer;
	}
}
