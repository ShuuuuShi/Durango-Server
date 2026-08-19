using PigeonCoopToolkit.Effects.Trails;
using UnityEngine;

public class FPSWeaponTrigger : MonoBehaviour
{
	public Transform ShellEjectionTransform;

	public float EjectionForce;

	public Rigidbody Shell;

	public Transform Muzzle;

	public GameObject Bullet;

	public float SmokeAfter;

	public float SmokeMax;

	public float SmokeIncrement;

	public SmokePlume MuzzlePlume;

	public GameObject MuzzleFlashObject;

	private float _smoke;

	private void Update()
	{
		MuzzlePlume.Emit = _smoke > SmokeAfter;
		_smoke -= Time.deltaTime;
		if (_smoke > SmokeMax)
		{
			_smoke = SmokeMax;
		}
		if (_smoke < 0f)
		{
			_smoke = 0f;
		}
	}

	public void Fire()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		MuzzleFlashObject.SetActive(true);
		((MonoBehaviour)this).Invoke("LightsOff", 0.05f);
		_smoke += SmokeIncrement;
		Object obj = Object.Instantiate((Object)(object)((Component)Shell).gameObject, ShellEjectionTransform.position, ShellEjectionTransform.rotation);
		Rigidbody component = ((GameObject)((obj is GameObject) ? obj : null)).GetComponent<Rigidbody>();
		component.velocity = ShellEjectionTransform.right * EjectionForce + Random.onUnitSphere * 0.25f;
		component.angularVelocity = Random.onUnitSphere * EjectionForce;
		Object.Instantiate((Object)(object)Bullet, ((Component)Muzzle).transform.position, Muzzle.rotation);
	}

	private void LightsOff()
	{
		MuzzleFlashObject.SetActive(false);
	}
}
