using System;
using ItemSystem;
using UnityEngine;

public class SortOptionWidget : MonoBehaviour
{
	[SerializeField]
	private UIScrollView _scrollView;

	[SerializeField]
	private GameObject _defaultSort;

	[SerializeField]
	private GameObject _levelSort;

	[SerializeField]
	private GameObject _weightSort;

	[SerializeField]
	private GameObject _duraSort;

	[SerializeField]
	private GameObject _colorSort;

	private UIWidget _invisibleBox;

	private GameObject _prevClicked;

	public event Action<Util.SortOption, bool> SortOptionSelected;

	private void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_defaultSort);
		uIEventListener.onClick = OnClickSortItem;
		uIEventListener = UIEventListener.Get(_levelSort);
		uIEventListener.onClick = OnClickSortItem;
		uIEventListener = UIEventListener.Get(_weightSort);
		uIEventListener.onClick = OnClickSortItem;
		uIEventListener = UIEventListener.Get(_duraSort);
		uIEventListener.onClick = OnClickSortItem;
		uIEventListener = UIEventListener.Get(_colorSort);
		uIEventListener.onClick = OnClickSortItem;
	}

	private void OnEnable()
	{
		_invisibleBox = UIUtility.SetScrollViewInvisibleBox(_scrollView, _invisibleBox);
		_prevClicked = null;
	}

	private void OnClickSortItem(GameObject go)
	{
		if (this.SortOptionSelected != null)
		{
			bool flag = (Object)(object)_prevClicked != (Object)(object)go;
			if ((Object)(object)go == (Object)(object)_defaultSort)
			{
				this.SortOptionSelected(Util.SortOption.Default, flag);
			}
			else if ((Object)(object)go == (Object)(object)_levelSort)
			{
				this.SortOptionSelected(Util.SortOption.Level, flag);
			}
			else if ((Object)(object)go == (Object)(object)_weightSort)
			{
				this.SortOptionSelected(Util.SortOption.Weight, flag);
			}
			else if ((Object)(object)go == (Object)(object)_duraSort)
			{
				this.SortOptionSelected(Util.SortOption.Durability, flag);
			}
			else if ((Object)(object)go == (Object)(object)_colorSort)
			{
				this.SortOptionSelected(Util.SortOption.Color, flag);
			}
			_prevClicked = ((!flag) ? null : go);
		}
	}

	public void SetDepth(int d)
	{
		((Component)this).GetComponent<UIPanel>().depth = d;
		((Component)_scrollView).GetComponent<UIPanel>().depth = d + 1;
	}
}
