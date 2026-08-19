using System;
using System.Linq;
using Durango.Logic.Market;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MarketSubCatecoriesWidget : MonoBehaviour
{
	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleText;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private GameObject _noData;

	private UIWidget _widget;

	private bool _isInit;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	public event Action<Category.Sub> SubCategorySelected;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
		}
	}

	private void Start()
	{
		Init();
	}

	private void Deactivate()
	{
		_titleWidget.gameObject.SetActive(value: false);
		_scrollView.gameObject.SetActive(value: false);
		_noData.gameObject.SetActive(value: true);
	}

	public void SetCategory([CanBeNull] Category.Main main)
	{
		SetCategory(main, isSelected: false, null);
	}

	public void SetCategory([CanBeNull] Category.Main main, [CanBeNull] Category.Sub current)
	{
		SetCategory(main, isSelected: true, current);
	}

	private void SetCategory([CanBeNull] Category.Main main, bool isSelected, [CanBeNull] Category.Sub current)
	{
		Init();
		if (main == null)
		{
			Deactivate();
			return;
		}
		Category category = GameSystem<MarketSystem>.Instance().CategoryYamlData.FirstOrDefault((Category elem) => elem.MainCategory.Id == main.Id);
		if (category == null)
		{
			Deactivate();
			return;
		}
		_titleWidget.gameObject.SetActive(value: true);
		_scrollView.gameObject.SetActive(value: true);
		_noData.gameObject.SetActive(value: false);
		_titleText.text = category.MainCategory.Name;
		ListObjectPool nodes = _scrollView.Nodes;
		nodes.BeginLoad();
		GameObject firstNode = nodes.GetNext();
		firstNode.transform.Find("Text").GetComponent<UILabel>().text = T._("전체보기");
		SelectableWidget component = firstNode.GetComponent<SelectableWidget>();
		component.Selected = isSelected && current == null;
		component.Clicked = delegate
		{
			for (int k = 0; k < nodes.Count; k++)
			{
				nodes[k].GetComponent<SelectableWidget>().Selected = nodes[k] == firstNode;
			}
			if (this.SubCategorySelected != null)
			{
				this.SubCategorySelected(null);
			}
		};
		int i = 0;
		for (int size = KUtility.GetSize(category.Subs); i < size; i++)
		{
			Category.Sub targetData = category.Subs[i];
			GameObject targetNode = nodes.GetNext();
			UILabel component2 = targetNode.transform.Find("Text").GetComponent<UILabel>();
			component2.text = targetData.Name;
			SelectableWidget component3 = targetNode.GetComponent<SelectableWidget>();
			component3.Selected = isSelected && current != null && current.Id == targetData.Id;
			component3.Clicked = delegate
			{
				for (int j = 0; j < nodes.Count; j++)
				{
					nodes[j].GetComponent<SelectableWidget>().Selected = nodes[j] == targetNode;
				}
				if (this.SubCategorySelected != null)
				{
					this.SubCategorySelected(targetData);
				}
			};
		}
		nodes.EndLoad();
		_scrollView.Reposition();
	}
}
