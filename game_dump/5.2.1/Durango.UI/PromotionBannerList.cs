using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class PromotionBannerList : MonoBehaviour, IUIInitializable, RectLayout.ICompatible
{
	[SerializeField]
	private KScrollView _scroll;

	[SerializeField]
	private PageIndexSprite _pageIndexer;

	[SerializeField]
	private float _rollingPauseTime = 5f;

	private float? _scrollProcessTimer;

	void IUIInitializable.Init()
	{
		_scroll.Nodes.Init(delegate(GameObject obj)
		{
			obj.GetComponent<PromotionBannerWidget>().Pressed += OnLinkPress;
		});
		_scroll.AttachPageIndexSprite(_pageIndexer);
	}

	Vector2 RectLayout.ICompatible.UpdateLayout(float? x, float? y)
	{
		UIUtility.UpdateAnchors(base.transform);
		UIWidget component = GetComponent<UIWidget>();
		Vector2 localSize = component.localSize;
		localSize = new Vector2(x.GetValueOrDefault(localSize.x), y.GetValueOrDefault(localSize.y));
		component.SetDimensions((int)localSize.x, (int)localSize.y);
		UIUtility.UpdateAnchors(base.transform);
		Point2 point = new Point2(_scroll.ViewSize);
		foreach (GameObject node in _scroll.Nodes)
		{
			node.GetComponent<UIWidget>().SetDimensions(point.x, point.y);
		}
		UIUtility.UpdateAnchors(_scroll.ScrollView.transform);
		_scroll.ResetPosition();
		return localSize;
	}

	public bool Set(IList<PromotionLink> promotionLinks)
	{
		_scroll.Nodes.BeginLoad();
		int i = 0;
		for (int size = KUtility.GetSize(promotionLinks); i < size; i++)
		{
			PromotionLink promotionLink = promotionLinks[i];
			if (PromotionBannerWidget.IsShowPeriod(promotionLink))
			{
				_scroll.Nodes.GetNext().GetComponent<PromotionBannerWidget>().Set(promotionLink);
			}
		}
		_scroll.Nodes.EndLoad();
		if (_scroll.Nodes.Count > 0)
		{
			_scroll.ResetPosition();
			_pageIndexer.Make(_scroll.Nodes.Count);
			_scrollProcessTimer = 0f;
			base.gameObject.SetActive(value: true);
			return true;
		}
		base.gameObject.SetActive(value: false);
		return false;
	}

	private void Update()
	{
		if (!_scrollProcessTimer.HasValue)
		{
			return;
		}
		float? scrollProcessTimer = _scrollProcessTimer;
		_scrollProcessTimer = ((!scrollProcessTimer.HasValue) ? null : new float?(scrollProcessTimer.GetValueOrDefault() + Time.deltaTime));
		float? scrollProcessTimer2 = _scrollProcessTimer;
		if (scrollProcessTimer2.HasValue && scrollProcessTimer2.GetValueOrDefault() > _rollingPauseTime)
		{
			_scrollProcessTimer = 0f;
			int nodeCount = _scroll.GetNodeCount();
			int currentNodeIndex = _scroll.GetCurrentNodeIndex();
			if (currentNodeIndex + 1 < nodeCount)
			{
				_scroll.MoveToNode(currentNodeIndex + 1, instant: false);
			}
			else
			{
				_scroll.MoveToNode(0, instant: false);
			}
		}
	}

	private void OnLinkPress(bool press)
	{
		if (press)
		{
			_scrollProcessTimer = null;
		}
		else
		{
			_scrollProcessTimer = 0f;
		}
	}
}
