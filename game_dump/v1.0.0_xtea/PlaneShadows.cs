using UnityEngine;

public class PlaneShadows : MeshCloner
{
	[SerializeField]
	private Material _material;

	protected override Material GetSourceMaterial()
	{
		return _material;
	}
}
