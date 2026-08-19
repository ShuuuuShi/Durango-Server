using UnityEngine;

public class TankWeaponController : MonoBehaviour
{
	public TankProjectile ProjectilePrefab;

	public Transform Nozzle;

	private void Update()
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if (!((Component)this).GetComponent<Animation>().isPlaying && Input.GetKeyDown((KeyCode)32))
		{
			((Component)this).GetComponent<Animation>().Play();
			Object.Instantiate((Object)(object)ProjectilePrefab, Nozzle.position, Nozzle.rotation);
		}
	}
}
