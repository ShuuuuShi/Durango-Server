using Durango.Logic.LearningGuide;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class SubCategoryItemWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _uiWidget;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UIWidget _subjectItemWidget;

	[SerializeField]
	private UILabel _text;

	[SerializeField]
	private ListObjectPool _subjectItems;

	private bool _initialized;

	public AdviceSubCategory SubCategory { get; private set; }

	public void Init()
	{
		if (!_initialized)
		{
			_subjectItems.Init(delegate(GameObject obj)
			{
				UIEventListener.Get(obj).onClick = OnClickSubjectItem;
			});
			_initialized = true;
		}
	}

	public void SetSubCategory(AdviceSubCategory subCategory)
	{
		SubCategory = subCategory;
		_text.text = SubCategory.Name;
		_subjectItems.Clear();
		int num = 0;
		Advice[] advices = GameSystem<StatisticsSystem>.Instance().Advices;
		foreach (Advice advice in advices)
		{
			if (advice.SubCategory == SubCategory.Id)
			{
				_subjectItems.Add<SubjectItemWidget>().SetSubject(advice);
				num++;
			}
		}
		_uiWidget.height = _titleWidget.height + num * _subjectItemWidget.height;
		_subjectItems.Reposition(Vector3.down);
	}

	public void Refresh()
	{
		for (int i = 0; i < _subjectItems.Count; i++)
		{
			_subjectItems.Get<SubjectItemWidget>(i).RefreshAchievedInfo();
		}
	}

	public void RefreshNotification()
	{
		for (int i = 0; i < _subjectItems.Count; i++)
		{
			_subjectItems.Get<SubjectItemWidget>(i).RefreshNotification();
		}
	}

	private void OnClickSubjectItem(GameObject obj)
	{
		SubjectItemWidget component = obj.GetComponent<SubjectItemWidget>();
		if (!(component == null))
		{
			Advice subject = ((!component.Selected) ? component.Subject : null);
			UIManager.FindScript<LearningGuideGroup>().SelectSubject(subject);
		}
	}

	public bool RefreshSelectedStates([CanBeNull] Advice subject)
	{
		bool result = false;
		for (int i = 0; i < _subjectItems.Count; i++)
		{
			SubjectItemWidget subjectItemWidget = _subjectItems.Get<SubjectItemWidget>(i);
			subjectItemWidget.Selected = subjectItemWidget.Subject == subject;
			if (subjectItemWidget.Selected)
			{
				result = true;
			}
		}
		return result;
	}
}
