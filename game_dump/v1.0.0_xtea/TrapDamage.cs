using UnityEngine;

public class TrapDamage : TrapBase
{
	private AnimatingProp _anim;

	private AnimatingProp Anim
	{
		get
		{
			if ((Object)null == (Object)(object)_anim)
			{
				_anim = ((Component)this).GetComponentInChildren<AnimatingProp>();
			}
			return _anim;
		}
	}

	private void Start()
	{
		Anim.Play("Damage_Open", loop: false);
	}

	public override void OnTrapped()
	{
		base.OnTrapped();
		Anim.Play("Damage_Action", loop: false);
	}

	public override void OnBreak()
	{
		base.OnBreak();
		Anim.Play("Damage_Close", loop: false);
	}
}
