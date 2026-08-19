using System;
using System.Collections.Generic;
using System.Text;
using Durango.Logic.Faction;
using Durango.UI.Control;
using Durango.Utils;
using L10N;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionTalksViewer : MonoBehaviour
{
	private struct Talk
	{
		public Shared.Faction.Messenger Messenger;

		public string Message;
	}

	public Action<Talks> MoveToClicked;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private GameObject _bottomBar;

	[SerializeField]
	private SelectableButton _prevButton;

	[SerializeField]
	private SelectableButton _nextButton;

	[SerializeField]
	private RectLayout _layout;

	private KInfiniteScrollView.View<Talk, FactionTalkNode> _view;

	private readonly List<Talk> _talks = new List<Talk>();

	private Talks _prev;

	private Talks _next;

	private bool _isInit;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_view = _scrollView.Initialize(delegate(FactionTalkNode node, Talk talk)
		{
			node.Set(talk.Messenger, talk.Message);
			node.SeparatorOn(_view.CurrentIndex < _view.Count - 1);
		});
		_prevButton.Text = T._("이전 노트");
		_nextButton.Text = T._("다음 노트");
		_prevButton.Clicked = delegate
		{
			if (_prev != null && MoveToClicked != null)
			{
				MoveToClicked(_prev);
			}
		};
		_nextButton.Clicked = delegate
		{
			if (_next != null && MoveToClicked != null)
			{
				MoveToClicked(_next);
			}
		};
	}

	private void AppendTalk(Yaml.Talk talk, StringBuilder text)
	{
		if (text.Length > 0)
		{
			text.AppendLine();
		}
		text.Append(FactionGroup.FactionTalksToString(talk));
	}

	public void Show(FactionType type, Talks talks)
	{
		Init();
		_titleLabel.text = talks.Title;
		_talks.Clear();
		Shared.Faction.Messenger messenger = Shared.Faction.Messenger.Invalid;
		using (Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop())
		{
			StringBuilder value = reusable.Value;
			int i = 0;
			for (int size = KUtility.GetSize(talks.List); i < size; i++)
			{
				Yaml.Talk talk = talks.List[i];
				if (talk.Messenger != messenger)
				{
					if (value.Length > 0)
					{
						_talks.Add(new Talk
						{
							Messenger = messenger,
							Message = value.ToString()
						});
					}
					messenger = talk.Messenger;
					value.Length = 0;
				}
				AppendTalk(talk, value);
			}
			if (value.Length > 0)
			{
				_talks.Add(new Talk
				{
					Messenger = messenger,
					Message = value.ToString()
				});
			}
		}
		UpdateTalksIndex(type, talks);
		_view.SetList(_talks);
		_scrollView.ResetPosition();
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void UpdateTalksIndex(FactionType type, Talks talks)
	{
		int num = -1;
		int num2 = 0;
		_prev = null;
		_next = null;
		Talks[] array = SingletonDict<FactionType, Talks[]>.Get(type);
		Durango.Logic.Faction.Faction faction = GameSystem<FactionSystem>.Instance().GetFaction(type);
		if (faction != null && faction.Level > 0)
		{
			int point = faction.Point;
			int i = 0;
			for (int size = KUtility.GetSize(array); i < size; i++)
			{
				Talks talks2 = array[i];
				if (talks2.FriendshipPoint <= point)
				{
					if (talks2 == talks)
					{
						num = num2;
					}
					else if (num == -1)
					{
						_prev = talks2;
					}
					else if (_next == null)
					{
						_next = talks2;
					}
					num2++;
				}
			}
		}
		_countLabel.text = ((num != -1) ? $"<em>{num + 1}</em> [FFFFFF7F]/[-] {num2}" : string.Empty);
		_prevButton.gameObject.SetActive(_prev != null);
		_nextButton.gameObject.SetActive(_next != null);
		_bottomBar.SetActive(_prev != null || _next != null);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
