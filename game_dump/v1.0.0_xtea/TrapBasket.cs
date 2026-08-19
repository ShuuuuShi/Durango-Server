using UnityEngine;

public class TrapBasket : TrapBase
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
