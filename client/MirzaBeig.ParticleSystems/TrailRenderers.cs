using UnityEngine;

namespace MirzaBeig.ParticleSystems;

public class TrailRenderers : MonoBehaviour
{
	[HideInInspector]
	public TrailRenderer[] trailRenderers;

	protected virtual void Awake()
	{
	}

	protected virtual void Start()
	{
		trailRenderers = GetComponentsInChildren<TrailRenderer>();
	}

	protected virtual void Update()
	{
	}

	public void setAutoDestruct(bool value)
	{
		for (int i = 0; i < trailRenderers.Length; i++)
		{
			trailRenderers[i].autodestruct = value;
		}
	}
}
