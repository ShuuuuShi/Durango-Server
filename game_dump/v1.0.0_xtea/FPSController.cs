using UnityEngine;

public class FPSController : MonoBehaviour
{
	public Animator CamAnimator;

	public Animator WeaponAnimator;

	public float moveSpeed;

	private void Start()
	{
	}

	private void Update()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		CamAnimator.SetBool("Running", Input.GetKey((KeyCode)119));
		WeaponAnimator.SetBool("Fire", Input.GetKey((KeyCode)32));
		if (Input.GetKey((KeyCode)119))
		{
			((Component)this).transform.position = ((Component)this).transform.position + ((Component)this).transform.forward * moveSpeed * Time.deltaTime;
		}
	}
}
