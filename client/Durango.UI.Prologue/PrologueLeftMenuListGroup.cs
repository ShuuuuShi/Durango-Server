using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Prologue;

public class PrologueLeftMenuListGroup : PrologueLeftMenuListGroupBase
{
	[SerializeField]
	private UITweener _tweener;

	[SerializeField]
	private float _width;

	public override bool Show
	{
		get
		{
			return IsShow;
		}
		set
		{
			if (IsShow != value)
			{
				IsShow = value;
				if (IsShow)
				{
					_tweener.gameObject.SetActive(value: true);
					_tweener.tweenFactor = 0f;
					_tweener.PlayForward();
				}
				else
				{
					_tweener.gameObject.SetActive(value: false);
				}
				VisibleController.Hide(base.HideUIFunc, IsShow, "LeftMenu");
			}
		}
	}

	protected override void Start()
	{
		base.Start();
		_tweener.transform.localPosition = Vector3.right * _width / 2f;
		_tweener.gameObject.SetActive(value: false);
	}
}
