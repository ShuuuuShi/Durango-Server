using System;
using UnityEngine;

public class InteractionHelperGroup : UIBase
{
	[SerializeField]
	private Selectable _searchButton;

	[SerializeField]
	private InteractionHelperList _helperList;

	private Vector3 _baseSearchButtonPos;

	public void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(((Component)_searchButton).gameObject);
		uIEventListener.onPress = (UIEventListener.BoolDelegate)Delegate.Combine(uIEventListener.onPress, new UIEventListener.BoolDelegate(OnTouchSearchButton));
		_helperList.ShowStateChanged += OnHelperShow;
		ToDoListGroup toDoListGroup = UIManager.FindScript<ToDoListGroup>();
		toDoListGroup.WidthRatioChanged += OnChangeTodoWidthRatio;
	}

	private void OnPortraitMode(bool isPortrait)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		_baseSearchButtonPos = -Vector3.one;
	}

	private void OnHelperShow()
	{
		_searchButton.Select = _helperList.IsShow;
	}

	private void OnTouchSearchButton(GameObject obj, bool press)
	{
		if (press && !_helperList.IsShow)
		{
			_helperList.Show();
		}
	}

	private void OnChangeTodoWidthRatio(float ratio)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (_baseSearchButtonPos == -Vector3.one)
		{
			_baseSearchButtonPos = ((Component)_searchButton).transform.localPosition;
		}
		Vector3 baseSearchButtonPos = _baseSearchButtonPos;
		baseSearchButtonPos.x -= (1f - ratio) * 100f;
		((Component)_searchButton).transform.localPosition = baseSearchButtonPos;
	}
}
