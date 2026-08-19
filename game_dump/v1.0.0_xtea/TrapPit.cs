using UnityEngine;

public class TrapPit : TrapBase
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
		Anim.Play("Pit_Before", loop: false);
	}

	public override void OnTrapped()
	{
		base.OnTrapped();
		Anim.Play("Pit_After", loop: false);
	}
}
