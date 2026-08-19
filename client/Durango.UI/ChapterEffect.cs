using System;
using Algorithms;
using Durango.UI.Control;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class ChapterEffect : MonoBehaviour, ITimeSequencePlayer
{
	private class Item : ITimeSequenceItem
	{
		public string Title;

		public string SubTitle;

		public Action OnFinish;

		public float At { get; set; }
	}

	private const string HideKey = "Chapter";

	[SerializeField]
	private TweenerPlayer _tweeners;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _subtitleLabel;

	[SerializeField]
	private SoundEventType _sound;

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

	public void Set(string title, string subtitle, Action onFinish)
	{
		if (_arguments == null)
		{
			_arguments = new PriorityQueueB<Item>(new TimeSequenceItemComparer<Item>());
		}
		_arguments.Push(new Item
		{
			At = Time.time,
			Title = title,
			SubTitle = subtitle,
			OnFinish = onFinish
		});
	}

	private void OnShow()
	{
		base.gameObject.SetActive(value: true);
		UIBase parent = Parent;
		if (parent != null)
		{
			parent.VisibleController.HideExceptForMe(hide: true, "Chapter", 0.3f);
		}
	}

	private void OnStop()
	{
		base.gameObject.SetActive(value: false);
		UIBase parent = Parent;
		if (parent != null)
		{
			parent.VisibleController.HideExceptForMe(hide: false, "Chapter", 0.3f);
		}
	}

	public void Stop()
	{
		_tweeners.Stop();
		OnStop();
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
		if (_arguments != null && _arguments.Count != 0)
		{
			_current = _arguments.Pop();
			_titleLabel.text = _current.Title;
			if (_subtitleLabel != null)
			{
				_subtitleLabel.text = _current.SubTitle;
			}
			SoundManager.PlayEvent(_sound);
			_tweeners.Play(OnFinish);
			OnShow();
		}
	}

	public bool IsPlaying()
	{
		return _current != null;
	}
}
