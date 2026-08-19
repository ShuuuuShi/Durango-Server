using Durango.UI.Control;

namespace Durango.UI;

public class TweenerRewardWidget : AlarmRewardWidget
{
	protected TweenerPlayer _tweener;

	protected override void OnInit()
	{
		base.OnInit();
		_tweener = GetComponent<TweenerPlayer>();
	}

	protected override void Play()
	{
		base.Play();
		if (_tweener == null)
		{
			TimeOut();
		}
		else
		{
			_tweener.Play(base.TimeOut);
		}
	}
}
