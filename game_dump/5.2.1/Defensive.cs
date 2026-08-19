using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Durango.Network;
using Messages;
using UnityEngine;

public class Defensive : ArtifactComponent
{
	[CompilerGenerated]
	private sealed class _003CCoUpdateProjectiles_003Ed__9 : IEnumerator<object>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private object _003C_003E2__current;

		public Defensive _003C_003E4__this;

		object IEnumerator<object>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CCoUpdateProjectiles_003Ed__9(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			Defensive defensive = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
			}
			else
			{
				_003C_003E1__state = -1;
			}
			defensive.ProjectileController.UpdateProjectiles();
			_003C_003E2__current = null;
			_003C_003E1__state = 1;
			return true;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

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
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CCoUpdateProjectiles_003Ed__9(0)
		{
			_003C_003E4__this = this
		};
	}
}
