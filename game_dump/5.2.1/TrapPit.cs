using Durango.Model;

public class TrapPit : TrapBase
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
		Anim.Play("Pit_Before", loop: false);
	}

	public override void OnTrapped()
	{
		base.OnTrapped();
		Anim.Play("Pit_After", loop: false);
	}
}
