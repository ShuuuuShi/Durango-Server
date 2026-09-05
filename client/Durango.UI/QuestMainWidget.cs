using System.Collections.Generic;
using System.Linq;
using Durango.UI.Control;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.UI;

public class QuestMainWidget : MonoBehaviour, IUIInitializable
{
	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private AnimationWidget _loadingIcon;

	[SerializeField]
	private UILabel _noQuestLabel;

	private KInfiniteScrollView.View<QuestToDo, QuestNodeWidget> _questView;

	void IUIInitializable.Init()
	{
		_questView = _scrollView.Initialize(delegate(QuestNodeWidget node, QuestToDo data)
		{
			node.Set(data);
		}, delegate(QuestNodeWidget node)
		{
			node.Init();
		});
		_noQuestLabel.gameObject.SetActive(value: false);
		_noQuestLabel.text = T._("퀘스트가 없습니다.");
		_loadingIcon.gameObject.SetActive(value: false);
	}

	public void ShowLoading()
	{
		_loadingIcon.gameObject.SetActive(value: true);
		_loadingIcon.Widget.alpha = 0f;
		_loadingIcon.Alpha = 1f;
		_scrollView.Panel.alpha = 0f;
		_noQuestLabel.gameObject.SetActive(value: false);
	}

	public void Set(List<QuestToDo> quests, bool reset)
	{
		_loadingIcon.gameObject.SetActive(value: false);
		_scrollView.Panel.alpha = 1f;
		if (reset)
		{
			_scrollView.ResetPosition();
		}
		// [แก้เอง] แสดงได้แค่ 10 รายการ (ข้อมูลครบในเซิร์ฟ ความคืบหน้าทำงานปกติ)
		// ลิสต์จาก client เรียง unfinished ก่อน + ตาม Order ของเควสอยู่แล้ว — ตัดท้าย 10 แรก
		List<QuestToDo> shown = quests;
		_questView.SetList(shown);
		if (KUtility.GetSize(shown) == 0)
		{
			_noQuestLabel.gameObject.SetActive(value: true);
			_noQuestLabel.alpha = 0f;
			TweenAlpha tweenAlpha = TweenAlpha.Begin(_noQuestLabel.gameObject, 0.2f, 1f);
			tweenAlpha.method = UITweener.Method.EaseOut;
			tweenAlpha.PlayForward();
		}
		else
		{
			_noQuestLabel.gameObject.SetActive(value: false);
		}
	}

	public Transform GetQuestReceiveButtonTransform(string id)
	{
		foreach (QuestNodeWidget item in _questView.List)
		{
			if (item.QuestId == id)
			{
				return item.GetRecieveButtonTransform();
			}
		}
		return null;
	}
}
