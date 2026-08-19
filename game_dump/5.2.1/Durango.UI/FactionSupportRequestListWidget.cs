using System.Collections.Generic;
using Durango.Logic.Faction;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class FactionSupportRequestListWidget : UIWidget
{
	private const int MaxSupportRequestVisibleCount = 3;

	[SerializeField]
	private FactionSupportRequestWidget _nodeBase;

	[SerializeField]
	private FactionSupportRequestLockWidget _lockedRequestWidget;

	private ListObjectPool<FactionSupportRequestWidget> _nodes;

	private float _nodeMargin;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_nodes = new ListObjectPool<FactionSupportRequestWidget>();
			_nodes.BaseObject = _nodeBase;
		}
	}

	private void UpdateLayout()
	{
		Point2 point = new Point2(base.width, base.height);
		if (UIManager.IsPortraitWidget(base.gameObject))
		{
			Point2 point2 = point;
			point2.y = (base.height - 20) / 3;
			_nodeMargin = (float)(base.height - point2.y * 3) / 2f;
			_nodes.BaseObject.SetDimensions(point2.x, point2.y);
			for (int i = 0; i < _nodes.Count; i++)
			{
				_nodes[i].UpdateLayout(point2.x, point2.y);
			}
			UIUtility.WidgetsReposition(_nodes, this, Vector3.down, _nodeMargin);
		}
		else
		{
			Point2 point3 = point;
			point3.x = (base.width - 20) / 3;
			_nodeMargin = (float)(base.width - point3.x * 3) / 2f;
			_nodes.BaseObject.SetDimensions(point3.x, point3.y);
			for (int j = 0; j < _nodes.Count; j++)
			{
				_nodes[j].UpdateLayout(point3.x, point3.y);
			}
			UIUtility.WidgetsReposition(_nodes, this, Vector3.right, _nodeMargin);
		}
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Set(Durango.Logic.Faction.Faction faction, int level, List<SupportRequest> requests)
	{
		Init();
		int size = KUtility.GetSize(requests);
		if (faction.Level < level)
		{
			_lockedRequestWidget.gameObject.SetActive(value: true);
			_lockedRequestWidget.Set(faction.Type, level, requests);
			_nodes.Clear();
		}
		else
		{
			_lockedRequestWidget.gameObject.SetActive(value: false);
			_nodes.BeginLoad();
			for (int i = 0; i < 3; i++)
			{
				FactionSupportRequestWidget next = _nodes.GetNext();
				if (i < size)
				{
					next.Set(requests[i]);
				}
				else
				{
					next.SetEmpty();
				}
			}
			_nodes.EndLoad();
			UIUtility.WidgetsReposition(_nodes, this, (!UIManager.IsPortraitWidget(base.gameObject)) ? Vector3.right : Vector3.down, _nodeMargin);
		}
		UpdateLayout();
	}

	public Transform GetRequestAvailableButtonTransform()
	{
		for (int i = 0; i < _nodes.Count; i++)
		{
			FactionSupportRequestWidget factionSupportRequestWidget = _nodes.Get<FactionSupportRequestWidget>(i);
			if (!(factionSupportRequestWidget == null))
			{
				Transform buttonTransformIfRequestAvailable = factionSupportRequestWidget.GetButtonTransformIfRequestAvailable();
				if (buttonTransformIfRequestAvailable != null)
				{
					return buttonTransformIfRequestAvailable;
				}
			}
		}
		return null;
	}
}
