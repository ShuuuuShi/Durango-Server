using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Faction;
using Durango.UI.Control;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestNodeList : KInfiniteScrollView
{
	private Durango.Logic.Faction.Faction _faction;

	private readonly List<List<SupportRequest>> _requestList = new List<List<SupportRequest>>();

	private View<List<SupportRequest>, FactionSupportRequestListWidget> _view;

	private bool _isInit;

	public float NodeSize => GetSize(base.ViewSize) + (float)base.Margin;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_view = Initialize(delegate(FactionSupportRequestListWidget comp, List<SupportRequest> list)
			{
				comp.Set(_faction, _view.CurrentIndex + 1, list);
			});
		}
	}

	protected override void OnUpdateViewSize()
	{
		Init();
		base.OnUpdateViewSize();
		_view.NodeResize(new Point2(base.ViewSize));
	}

	public void Set(Durango.Logic.Faction.Faction faction)
	{
		Init();
		_faction = faction;
		for (int i = 0; i < _requestList.Count; i++)
		{
			_requestList[i].Clear();
		}
		int maxLevel = _faction.GetMaxLevel();
		List<SupportRequest> supportRequests = _faction.SupportRequests;
		for (int j = 0; j < maxLevel; j++)
		{
			List<SupportRequest> list;
			if (j < _requestList.Count)
			{
				list = _requestList[j];
			}
			else
			{
				list = new List<SupportRequest>();
				_requestList.Add(list);
			}
			int lv = j + 1;
			list.AddRange(supportRequests.Where((SupportRequest x) => x.Level == lv));
		}
		if (_requestList.Count > maxLevel)
		{
			_requestList.RemoveRange(maxLevel, _requestList.Count - maxLevel);
		}
		_view.SetList(_requestList);
		Reposition();
	}

	public Transform GetRequestAvailableButtonTransform()
	{
		foreach (FactionSupportRequestListWidget item in _view.List)
		{
			Transform requestAvailableButtonTransform = item.GetRequestAvailableButtonTransform();
			if (requestAvailableButtonTransform != null)
			{
				return requestAvailableButtonTransform;
			}
		}
		return null;
	}
}
