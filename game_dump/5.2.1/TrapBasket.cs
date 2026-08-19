using Durango.Model;

public class TrapBasket : TrapBase
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
		Anim.Play("Basket_Open", loop: false);
	}

	public override void OnTrapped()
	{
		base.OnTrapped();
		Anim.Play("Basket_Close_Looping");
	}

	public override void OnBreak()
	{
		base.OnBreak();
		Anim.Play("Basket_Close", loop: false);
	}
}
