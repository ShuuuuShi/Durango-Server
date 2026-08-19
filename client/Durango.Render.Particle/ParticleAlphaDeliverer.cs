using UnityEngine;

namespace Durango.Render.Particle;

[ExecuteInEditMode]
public class ParticleAlphaDeliverer : MonoBehaviour
{
	private Material _material;

	private void OnEnable()
	{
		_material = base.gameObject.GetComponent<Renderer>().material;
		if (!(_material == null) && _material.HasProperty("_Cutoff"))
		{
		}
	}

	private void Update()
	{
		if (!(_material == null))
		{
			ParticleSystem component = base.gameObject.GetComponent<ParticleSystem>();
			if (!(component == null))
			{
				_material.SetFloat("_Cutoff", component.main.startColor.color.a);
			}
		}
	}
}
