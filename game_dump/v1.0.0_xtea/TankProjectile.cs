using UnityEngine;

public class TankProjectile : MonoBehaviour
{
	public float Speed;

	public float Lifetime;

	private void Start()
	{
		((MonoBehaviour)this).Invoke("DestroySelf", Lifetime);
	}

	private void DestroySelf()
	{
		Object.Destroy((Object)(object)((Component)this).gameObject);
	}

	private void Update()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		((Component)this).transform.position = ((Component)this).transform.position + ((Component)this).transform.forward * Speed * Time.deltaTime;
	}
}
