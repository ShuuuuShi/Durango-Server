using Durango.Logic;
using Durango.UI.Control;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class StoryMainViewer : MonoBehaviour
{
	[SerializeField]
	private GameObject _prev;

	[SerializeField]
	private GameObject _next;

	[SerializeField]
	private KScrollView _scroll;

	[SerializeField]
	private CenterFixedScrollBar _lowerScroll;

	[SerializeField]
	private StoryViewScrollNode.Shape _selected;

	[SerializeField]
	private StoryViewScrollNode.Shape _deselected;

	[SerializeField]
	private StoryChapterDetailViewer _detailViewer;

	private int _scrollIndex;

	private float _scrollOffsetRatio;

	private void Awake()
	{
		_scroll.Nodes.UseBase = false;
		UIEventListener.Get(_prev).onClick = delegate
		{
			_scroll.MoveToNode(_scrollIndex - 1, instant: false);
		};
		UIEventListener.Get(_next).onClick = delegate
		{
			_scroll.MoveToNode(_scrollIndex + 1, instant: false);
		};
	}

	private void Update()
	{
		int num = _scroll.GetNodeCount() - 1;
		if (_scroll.Nodes.Count == 0)
		{
			return;
		}
		float offsetRatio = _scroll.OffsetRatio;
		if (!Mathf.Approximately(offsetRatio, _scrollOffsetRatio))
		{
			_scrollOffsetRatio = offsetRatio;
			for (int i = 0; i < _lowerScroll.Nodes.Count; i++)
			{
				float value = Mathf.Abs(offsetRatio - (float)i);
				StoryViewScrollNode.Shape shape = _selected.Lerp(value, _deselected);
				_lowerScroll.Nodes.Get<StoryViewScrollNode>(i).SetShape(shape);
			}
			int num2 = Mathf.Clamp((int)Mathf.Round(offsetRatio), 0, num);
			if (_scrollIndex != num2)
			{
				_scrollIndex = num2;
				_prev.SetActive(num2 > 0);
				_next.SetActive(num2 < num);
				StoryViewNode storyViewNode = _scroll.Nodes.Get<StoryViewNode>(_scrollIndex);
				_detailViewer.Set(storyViewNode.Chapter, storyViewNode.Kind);
			}
		}
	}

	public void Set(Chapters chapters)
	{
		if (chapters == null || KUtility.GetSize(chapters.ChapterList) == 0)
		{
			return;
		}
		_scroll.Nodes.BeginLoad();
		_lowerScroll.Nodes.BeginLoad();
		Vector2 viewSize = _scroll.ViewSize;
		_scroll.Nodes.BaseObject.GetComponent<UIWidget>().SetDimensions((int)viewSize.x, (int)viewSize.y);
		bool flag = false;
		int num = 0;
		Chapter[] chapterList = chapters.ChapterList;
		foreach (Chapter chapter in chapterList)
		{
			StoryViewNode component = _scroll.Nodes.GetNext().GetComponent<StoryViewNode>();
			UIWidget component2 = component.GetComponent<UIWidget>();
			component2.SetDimensions((int)viewSize.x, (int)viewSize.y);
			UIUtility.UpdateAnchors(component2.transform);
			component.Set(chapter, flag);
			if (GameSystem<QuestSystem>.Instance().GetChapterProgress(chapter) < 1f)
			{
				flag = true;
			}
			if (!flag)
			{
				num++;
			}
			_lowerScroll.Nodes.GetNext().GetComponent<StoryViewScrollNode>().Set(component.Kind, chapter.ChapterNum);
		}
		_lowerScroll.Nodes.EndLoad();
		_scroll.Nodes.EndLoad();
		_scroll.ResetPosition();
		_scroll.MoveToNode(num, instant: true);
		_lowerScroll.UpdateLayout();
		_scrollIndex = -1;
		_scrollOffsetRatio = float.MinValue;
	}
}
