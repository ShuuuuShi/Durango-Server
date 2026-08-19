using Durango.Logic;
using L10N;
using Messages;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class StoryChapterDetailViewer : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scroll;

	[SerializeField]
	private UILabel _noData;

	[SerializeField]
	private UISprite _circleProgress;

	[SerializeField]
	private UILabel _progressLabel;

	[SerializeField]
	private UISprite _progressBar;

	[SerializeField]
	private UISprite _progressBarBg;

	[SerializeField]
	private ListObjectPool _pool;

	public void Set(Chapter chapter, Chapter.Kind kind)
	{
		if (kind == Chapter.Kind.Locked || KUtility.GetSize(chapter.Quests) == 0)
		{
			_noData.gameObject.SetActive(value: true);
			_scroll.gameObject.SetActive(value: false);
			_noData.text = ((kind != Chapter.Kind.Locked) ? T._("챕터 목표가 없습니다.") : T._("이전 챕터를 완료하면 개방됩니다."));
			return;
		}
		_noData.gameObject.SetActive(value: false);
		_scroll.gameObject.SetActive(value: true);
		float chapterProgress = GameSystem<QuestSystem>.Instance().GetChapterProgress(chapter);
		_circleProgress.fillAmount = chapterProgress;
		_progressLabel.text = $"{chapterProgress:P0}";
		bool flag = false;
		_pool.BeginLoad();
		int num = 100;
		int num2 = num;
		int num3 = num;
		string[] quests = chapter.Quests;
		foreach (string questId in quests)
		{
			QuestToDo epicQuest = GameSystem<QuestSystem>.Instance().GetEpicQuest(questId);
			StoryChapterDetailNode component = _pool.GetNext().GetComponent<StoryChapterDetailNode>();
			component.Set(epicQuest, flag);
			component.transform.localPosition = new Vector3(0f, -num, 0f);
			if (!flag)
			{
				num2 = num;
			}
			num3 = num;
			int num4 = Mathf.Max(component.GetComponent<UIWidget>().height + 50, 100);
			num += num4;
			if (!epicQuest.Finished)
			{
				flag = true;
			}
		}
		_progressBar.height = num2 + (int)_progressBar.transform.localPosition.y;
		_progressBarBg.height = num3 + (int)_progressBarBg.transform.localPosition.y;
		_pool.EndLoad();
		_scroll.ResetPosition();
	}
}
