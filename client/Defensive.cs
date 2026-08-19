using System.Collections;
using Durango.Network;
using Messages;
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
				weaponDisplayInfo.Projectile = "TurretBow";
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
			if (_aimBasis == null)
			{
				GameObject gameObject = new GameObject("AimBasis");
				gameObject.transform.parent = ((base.Artifact.transform.childCount != 0) ? base.Artifact.transform.GetChild(0) : base.Artifact.transform);
				gameObject.transform.localPosition = new Vector3(0f, 347f, 0f);
				_aimBasis = gameObject.transform;
			}
			return _aimBasis;
		}
	}

	public override void OnRemoved()
	{
		base.OnRemoved();
		ProjectileController.OnRemoved();
	}

	public void ShootProjectile(double eventAt)
	{
		DamageableEntity target = ProjectileController.Target;
		if (target == null)
		{
			return;
		}
		float num = (float)(eventAt - Connections.Frontend.GetBufferedServerTime());
		float magnitude = (target.GetCurrentPosition() - AimBasis.position).magnitude;
		float num2 = ProjectileController.EstimateLaunchingTime(magnitude);
		float delay = num - num2;
		KUtility.DelayedCall(base.Artifact, delegate
		{
			ProjectileController.ShootProjectile();
			if (_coUpdateProjectiles == null)
			{
				_coUpdateProjectiles = base.Artifact.StartCoroutine(CoUpdateProjectiles());
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
}
