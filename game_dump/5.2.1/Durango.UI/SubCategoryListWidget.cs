using Durango.Logic.LearningGuide;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class SubCategoryListWidget : MonoBehaviour
{
	[SerializeField]
	private KScrollView _kScrollView;

	[SerializeField]
	private UIWidget _imageUpperDotLine;

	private bool _initialized;

	public void Init()
	{
		if (!_initialized)
		{
			_kScrollView.Nodes.Init(delegate(GameObject go)
			{
				go.GetComponent<SubCategoryItemWidget>().Init();
			});
			_initialized = true;
		}
	}

	public void SetCategory(AdviceCategory category)
	{
		AdviceCategory[] adviceCategories = GameSystem<StatisticsSystem>.Instance().AdviceCategories;
		foreach (AdviceCategory adviceCategory in adviceCategories)
		{
			if (adviceCategory != category)
			{
				continue;
			}
			_kScrollView.Nodes.Clear();
			foreach (AdviceSubCategory subCategory in adviceCategory.SubCategories)
			{
				_kScrollView.Nodes.Add<SubCategoryItemWidget>().SetSubCategory(subCategory);
			}
			_kScrollView.ResetPosition();
			break;
		}
		Refresh();
	}

	public void Refresh()
	{
		RefreshSubCategoryItems();
		RefreshUpperDotLine();
	}

	public void RefreshNotification()
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			_kScrollView.Nodes.Get<SubCategoryItemWidget>(i).RefreshNotification();
		}
	}

	private void RefreshSubCategoryItems()
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			_kScrollView.Nodes.Get<SubCategoryItemWidget>(i).Refresh();
		}
	}

	public void SelectSubject(Advice subject, bool moveTo)
	{
		for (int i = 0; i < _kScrollView.Nodes.Count; i++)
		{
			if (_kScrollView.Nodes.Get<SubCategoryItemWidget>(i).RefreshSelectedStates(subject) && moveTo)
			{
				_kScrollView.MoveToNode(i, instant: true);
			}
		}
	}

	private void RefreshUpperDotLine()
	{
		if (_kScrollView.Nodes.Count > 0)
		{
			UIWidget component = _kScrollView.Nodes[0].GetComponent<UIWidget>();
			_imageUpperDotLine.gameObject.SetActive(value: true);
			_imageUpperDotLine.transform.localPosition = component.transform.localPosition + Vector3.up * _imageUpperDotLine.height;
		}
		else
		{
			_imageUpperDotLine.gameObject.SetActive(value: false);
		}
	}
}
