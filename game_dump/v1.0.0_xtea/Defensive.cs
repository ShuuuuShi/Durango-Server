using System.Collections;
using Messages;
using Shared.Battle;
using UnityEngine;

public class Defensive : ArtifactComponent
{
	private ProjectileController _projectileController;

	private Transform _aimBasis;

	private Coroutine _coUpdateProjectiles;

	public ProjectileController ProjectileController
	{
		get
		{
			if (_projectileController == null)
			{
				_projectileController = new ProjectileController(AimBasis);
				WeaponDisplayInfo weaponDisplayInfo = default(WeaponDisplayInfo);
				weaponDisplayInfo.Projectile = "Arrow";
				WeaponDisplayInfo weaponData = weaponDisplayInfo;
				ProjectileController.SetWeaponData(weaponData);
			}
			return _projectileController;
		}
	}

	private Transform AimBasis
	{
		get
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_aimBasis == (Object)null)
			{
				GameObject val = new GameObject("AimBasis");
				val.transform.parent = ((((Component)base.Artifact).transform.childCount != 0) ? ((Component)base.Artifact).transform.GetChild(0) : ((Component)base.Artifact).transform);
				val.transform.localPosition = new Vector3(0f, 347f, 0f);
				_aimBasis = val.transform;
			}
			return _aimBasis;
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		ProjectileController.OnRemoved();
	}

	public void ShootProjectile(GameObject target, BodyPart part, float damageDelay)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target.transform.position - AimBasis.position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		float num = ProjectileController.EstimateLaunchingTime(magnitude);
		float delay = damageDelay - num;
		KUtility.DelayedCall((MonoBehaviour)(object)base.Artifact, delegate
		{
			ProjectileController.LastAttackBodyPart = part;
			ProjectileController.OnChargedProjectile(target);
			ProjectileController.OnShootProjectile(target);
			if (_coUpdateProjectiles == null)
			{
				_coUpdateProjectiles = ((MonoBehaviour)base.Artifact).StartCoroutine(CoUpdateProjectiles());
			}
		}, delay);
	}

	private IEnumerator CoUpdateProjectiles()
	{
		while (true)
		{
			ProjectileController.UpdateProjectiles();
			yield return null;
		}
	}

	public void DamageResultReceived(Damage damage)
	{
		ProjectileController.DamageResultReceived(damage);
	}
}
