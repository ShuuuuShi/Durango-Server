using UnityEngine;

[ExecuteInEditMode]
public class ParticleAlphaDeliverer : MonoBehaviour
{
	private Material _material;

	private void OnEnable()
	{
		_material = ((Component)this).gameObject.GetComponent<Renderer>().material;
		if (!((Object)(object)_material == (Object)null) && _material.HasProperty("_Cutoff"))
		{
		}
	}

	private void Update()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_material == (Object)null))
		{
			ParticleSystem component = ((Component)this).gameObject.GetComponent<ParticleSystem>();
			if (!((Object)(object)component == (Object)null))
			{
				_material.SetFloat("_Cutoff", component.startColor.a);
			}
		}
	}
}
