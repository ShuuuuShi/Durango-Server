using System;
using System.Collections.Generic;
using Algorithms;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Quest;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class QuestFinishedEffect : MonoBehaviour, ITimeSequencePlayer
{
	private class Item : ITimeSequenceItem
	{
		public QuestRewardResults Value;

		public Action OnFinish;

		public float At { get; set; }
	}

	private const string HideKey = "QuestFinishedEffect";

	[SerializeField]
	private TweenerPlayer _tweeners;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _rewardTitleLabel;

	[SerializeField]
	private UISprite[] _iconSprites;

	[SerializeField]
	private ListObjectPool _rewardList;

	[SerializeField]
	private SoundEventType _sound;

	private Vector3? _rewardContainerBase;

	private PriorityQueueB<Item> _arguments;

	private Item _current;

	private UIBase _parent;

	private bool _isFoundParent;

	private UIBase Parent
	{
		get
		{
			if (!_isFoundParent)
			{
				_isFoundParent = true;
				_parent = UIUtility.FindComponentInParent<UIBase>(base.gameObject);
			}
			return _parent;
		}
	}

	public void Set(QuestRewardResults msg, Action onFinish)
	{
		if (_arguments == null)
		{
			_arguments = new PriorityQueueB<Item>(new TimeSequenceItemComparer<Item>());
		}
		_arguments.Push(new Item
		{
			At = Time.time,
			Value = msg,
			OnFinish = onFinish
		});
	}

	private void SetRewards(RewardInfo reward)
	{
		_rewardList.BeginLoad();
		foreach (KeyValuePair<Currency, long> item in reward.Currency)
		{
			GameObject next = _rewardList.GetNext();
			SetMoneyReward(next, new Money(item.Value, item.Key));
		}
		int i = 0;
		for (int size = KUtility.GetSize(reward.Items); i < size; i++)
		{
			GameObject next2 = _rewardList.GetNext();
			SetItemReward(next2, reward.Items[i]);
		}
		_rewardList.EndLoad();
		float num = UIUtility.WidgetsReposition(_rewardList, Vector3.right, Vector3.zero);
		Transform parent = _rewardList.BaseObject.transform.parent;
		Vector3? rewardContainerBase = _rewardContainerBase;
		if (!rewardContainerBase.HasValue)
		{
			_rewardContainerBase = parent.localPosition;
		}
		Vector3 value = _rewardContainerBase.Value;
		value += num * 0.5f * Vector3.left;
		parent.localPosition = value;
	}

	private void SetMoneyReward(GameObject node, Money money)
	{
		Transform obj = node.transform;
		obj.Find("Lv").gameObject.SetActive(value: false);
		obj.Find("RGBIcon").gameObject.SetActive(value: false);
		obj.Find("Bottom").gameObject.SetActive(value: false);
		UISprite component = obj.Find("Icon").GetComponent<UISprite>();
		UILabel component2 = obj.Find("Count").GetComponent<UILabel>();
		component.spriteName = Durango.Logic.Item.Inventory.GetIcon(money.Currency);
		component2.text = $"x{money.Amount}";
		component.gameObject.SetActive(value: true);
		component2.gameObject.SetActive(value: true);
	}

	private void SetItemReward(GameObject node, Messages.RewardItem item)
	{
		Transform obj = node.transform;
		obj.Find("Icon").gameObject.SetActive(value: false);
		obj.Find("Bottom").gameObject.SetActive(value: false);
		ItemIconTex component = obj.Find("RGBIcon").GetComponent<ItemIconTex>();
		UILabel component2 = obj.Find("Count").GetComponent<UILabel>();
		UILabel component3 = obj.Find("Lv").GetComponent<UILabel>();
		component.SetIcon(item);
		component2.text = "x1";
		component3.text = LocalizeUtil.FormatLevel(item.Level);
		component.gameObject.SetActive(value: true);
		component2.gameObject.SetActive(value: true);
		component3.gameObject.SetActive(value: true);
	}

	private void OnShow()
	{
		base.gameObject.SetActive(value: true);
		UIBase parent = Parent;
		if (parent != null)
		{
			parent.VisibleController.HideExceptForMe(hide: true, "QuestFinishedEffect", 0.3f);
		}
	}

	private void OnStop()
	{
		base.gameObject.SetActive(value: false);
		UIBase parent = Parent;
		if (parent != null)
		{
			parent.VisibleController.HideExceptForMe(hide: false, "QuestFinishedEffect", 0.3f);
		}
	}

	private void OnFinish()
	{
		Item current = _current;
		_current = null;
		if (current != null && current.OnFinish != null)
		{
			current.OnFinish();
		}
		OnStop();
	}

	public float? NextAt()
	{
		if (_arguments == null || _arguments.Count == 0)
		{
			return null;
		}
		return _arguments.Peek().At;
	}

	public void Play()
	{
		if (_arguments == null || _arguments.Count == 0)
		{
			return;
		}
		_current = _arguments.Pop();
		QuestRewardResults value = _current.Value;
		QuestYml questYml = SingletonDict<string, QuestYml>.Get(value.QuestId);
		if (questYml == null)
		{
			_current = null;
			return;
		}
		Category category = GameSystem<QuestSystem>.Instance().GetCategory(questYml.Category);
		Season? season = GameSystem<SeasonSystem>.Instance().GetSeason(category?.Season);
		_titleLabel.text = ((!season.HasValue) ? questYml.Subject.ToString() : season.Value.Name);
		_rewardTitleLabel.text = T._("아래의 보상을 획득하였습니다.");
		int i = 0;
		for (int size = KUtility.GetSize(_iconSprites); i < size; i++)
		{
			_iconSprites[i].spriteName = questYml.Icon;
		}
		if (value.Reward.HasValue)
		{
			SetRewards(value.Reward.Value);
		}
		else
		{
			_rewardList.Clear();
		}
		SoundManager.PlayEvent(_sound);
		_tweeners.Play(OnFinish);
		OnShow();
	}

	public void Stop()
	{
		_tweeners.Stop();
		OnStop();
	}

	public bool IsPlaying()
	{
		return _current != null;
	}
}
