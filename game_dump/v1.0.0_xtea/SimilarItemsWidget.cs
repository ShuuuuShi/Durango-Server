using System;
using System.Collections.Generic;
using MarketData;
using UnityEngine;

public class SimilarItemsWidget : MonoBehaviour
{
	private const int MaxCount = 3;

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private UIWidget _itemsWidget;

	[SerializeField]
	private ListObjectPool _similarItems;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private GameObject _loadingObj;

	private bool _isShow;

	private AnimationWidget _animWidget;

	private bool _isInit;

	private AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			AnimWidget.SetAlpha(0f, useTween: false);
		}
	}

	private void Start()
	{
		if (!_isShow)
		{
			((Component)this).gameObject.SetActive(false);
		}
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_titleWidget).gameObject);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickTitleBar));
	}

	public void Loading()
	{
		Show();
		_loadingObj.SetActive(true);
		_noData.gameObject.SetActive(false);
		_similarItems.Clear();
	}

	public void Show(IList<Commodity> list)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		Show();
		int num = Mathf.Min(list?.Count ?? 0, 3);
		_similarItems.Set(num);
		for (int i = 0; i < num; i++)
		{
			SimilarItemNode component = _similarItems[i].GetComponent<SimilarItemNode>();
			component.Set(list[i]);
			((Component)((Component)component).transform.FindChild("line")).gameObject.SetActive(i < num - 1);
		}
		_similarItems.Reposition(Vector3.right);
		_noData.gameObject.SetActive(num == 0);
		_loadingObj.SetActive(false);
	}

	private void Show()
	{
		Init();
		_isShow = true;
		((Component)this).gameObject.SetActive(true);
		AnimWidget.Alpha = 1f;
	}

	public void Hide()
	{
		AnimWidget.Alpha = 0f;
		_isShow = false;
	}

	private void OnClickTitleBar(GameObject obj)
	{
		if (_isShow)
		{
			Hide();
		}
	}
}
