using System;
using System.Collections.Generic;
using Durango.Logic.Faction;
using Durango.UI.Control;
using L10N;
using Shared.Faction;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class FactionTalksList : MonoBehaviour
{
	public Action<Talks> TalksClicked;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UILabel _countLabel;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	private KInfiniteScrollView.View<Talks, FactionTalksNode> _view;

	private readonly List<Talks> _talksList = new List<Talks>();

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_view = _scrollView.Initialize(delegate(FactionTalksNode comp, Talks talks)
			{
				comp.Set(talks);
			}, delegate(FactionTalksNode node)
			{
				node.Clicked = OnNodeClick;
			});
		}
	}

	private void OnNodeClick()
	{
		FactionTalksNode obj = (FactionTalksNode)Selectable.Current;
		int num = _view.IndexOf(obj);
		if (num != -1 && TalksClicked != null)
		{
			TalksClicked(_talksList[num]);
		}
	}

	public void Show(Durango.Logic.Faction.Faction faction)
	{
		Init();
		Yaml.Faction faction2 = SingletonDict<FactionType, Yaml.Faction>.Get(faction.Type);
		_titleLabel.text = T._("{0} 통신 기록", faction2.Name.ToString());
		Talks[] array = SingletonDict<FactionType, Talks[]>.Get(faction.Type);
		int point = faction.Point;
		_talksList.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(array); i < size; i++)
		{
			Talks talks = array[i];
			if (talks.FriendshipPoint > point)
			{
				break;
			}
			if (KUtility.GetSize(talks.List) != 0)
			{
				_talksList.Add(talks);
			}
		}
		_countLabel.text = _talksList.Count.ToString();
		_view.SetList(_talksList);
		_scrollView.Reposition();
		base.gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}
}
