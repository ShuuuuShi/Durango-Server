using System;
using System.Collections.Generic;
using UnityEngine;

public class ParticleInstance : MonoBehaviour
{
	[Serializable]
	private class ParticleContainer
	{
		public ParticleType Path;

		public Transform Parent;

		public Vector3 Position;

		public Vector3 Rotation;
	}

	[SerializeField]
	private List<ParticleContainer> _particles;

	private void Awake()
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		int count = _particles.Count;
		for (int i = 0; i < count; i++)
		{
			ParticleContainer particleContainer = _particles[i];
			if ((Object)(object)particleContainer.Parent == (Object)null)
			{
				particleContainer.Parent = ((Component)this).transform;
			}
			ParticleManager.Emit(particleContainer.Path, particleContainer.Position, Quaternion.Euler(particleContainer.Rotation), particleContainer.Parent, useLocalPosition: true, comeForwardToCamera: false, groundDecal: false, reusable: false);
		}
	}
}
