using Durango.Logic.Item;
using UnityEngine;

namespace Durango.UI;

public class TextResizeWithMultipleIconRewardWidget : TextResizeRewardWidget
{
	[SerializeField]
	private RewardIconWidget _rewardIconWidgetBase;

	private Vector3 _originPos;

	private ListObjectPool<RewardIconWidget> _rewardIconWidgets;

	protected override void OnInit()
	{
		base.OnInit();
		_originPos = _rewardIconWidgetBase.transform.localPosition;
		_rewardIconWidgets = new ListObjectPool<RewardIconWidget>();
		_rewardIconWidgets.BaseObject = _rewardIconWidgetBase;
		_rewardIconWidgets.UseBase = true;
	}

	public override void Set(string key, AlarmRewardQueue.Args args)
	{
		Init();
		if (args.ExtraIcons != null)
		{
			_rewardIconWidgets.BeginLoad();
			ItemIcon[] extraIcons = args.ExtraIcons;
			foreach (ItemIcon itemIcon in extraIcons)
			{
				_rewardIconWidgets.GetNext().Set(itemIcon, args.IconScale);
			}
			_rewardIconWidgets.EndLoad();
		}
		else
		{
			_rewardIconWidgets.Clear();
		}
		base.Set(key, args);
	}

	protected override void Play()
	{
		base.Play();
		foreach (RewardIconWidget rewardIconWidget in _rewardIconWidgets)
		{
			rewardIconWidget.PlayTweener();
		}
	}

	protected override void UpdateLayout()
	{
		Vector3 originPos = _originPos;
		originPos.x -= (float)(_rewardIconWidgetBase.width * (_rewardIconWidgets.Count - 1)) / 2f;
		foreach (RewardIconWidget rewardIconWidget in _rewardIconWidgets)
		{
			rewardIconWidget.transform.localPosition = originPos;
			originPos.x += _rewardIconWidgetBase.width;
		}
		base.UpdateLayout();
	}
}
