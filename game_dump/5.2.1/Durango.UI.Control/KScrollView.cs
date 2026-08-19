using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI.Control;

public class KScrollView : NodesScrollView
{
	[CanBeNull]
	private PageIndexSprite _pageIndexSprite;

	private float _scrollIndexCache;

	protected override float OnUpdateLayout(bool instant)
	{
		Vector3 basePosition = GetBasePosition();
		return UIUtility.WidgetsReposition(base.Nodes, base.Vector, basePosition, base.Margin, 0f, instant);
	}

	private void Update()
	{
		if (_pageIndexSprite == null)
		{
			return;
		}
		float currentOffset = base.CurrentOffset;
		if (!Mathf.Approximately(currentOffset, _scrollIndexCache))
		{
			_scrollIndexCache = currentOffset;
			if (GetNodeCount() > 0)
			{
				_pageIndexSprite.Set(base.OffsetRatio);
			}
		}
	}

	public void AttachPageIndexSprite(PageIndexSprite pageIndexSprite)
	{
		_pageIndexSprite = pageIndexSprite;
	}
}
