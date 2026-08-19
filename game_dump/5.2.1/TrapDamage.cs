using Durango.Model;

public class TrapDamage : TrapBase
{
	private AnimatingModel _anim;

	private AnimatingModel Anim
	{
		get
		{
			if (null == _anim)
			{
				_anim = GetComponentInChildren<AnimatingModel>();
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
