using System;
using System.Collections.Generic;
using CombatData;
using UnityEngine;

public class EquipItemActionWidget : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _actionIcons;

	[SerializeField]
	private UIScrollView _actionIconScrollView;

	[SerializeField]
	private int _actionIconWidth;

	[SerializeField]
	private UILabel _actionTitle;

	[SerializeField]
	private UILabel _actionDescription;

	[SerializeField]
	private UIWidget _actionInfoContainer;

	[SerializeField]
	private GameObject _noAction;

	private UIWidget _actionIconBox;

	private bool _isInit;

	private void OnEnable()
	{
		_actionIconBox = UIUtility.SetScrollViewInvisibleBox(_actionIconScrollView, _actionIconBox);
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_actionIcons.Init(InitActionIcons);
		}
	}

	private void InitActionIcons(GameObject obj)
	{
		UIEventListener uIEventListener = UIEventListener.Get(obj);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClick_ActionInfo));
	}

	public void SetAction(List<CombatData.Action> actions)
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		Init();
		_actionIcons.Clear();
		int i = 0;
		for (int num = actions?.Count ?? 0; i < num; i++)
		{
			CombatData.Action action = actions[i];
			if (action != null && !action.IsAutoAction())
			{
				EquipActionInfoIcon equipActionInfoIcon = ((ListObjectPoolBase<GameObject>)_actionIcons).Add<EquipActionInfoIcon>();
				equipActionInfoIcon.Set(action);
			}
		}
		int count = _actionIcons.Count;
		Vector3 localPosition = _actionIcons.BaseObject.transform.localPosition;
		for (int j = 0; j < count; j++)
		{
			_actionIcons[j].transform.localPosition = localPosition + Vector3.right * (float)j * (float)_actionIconWidth;
		}
		((Component)_actionInfoContainer).gameObject.SetActive(count > 0);
		_noAction.SetActive(count == 0);
		if (count > 0)
		{
			_actionIconScrollView.ResetPosition();
			_actionInfoContainer.alpha = 0f;
			TweenAlpha.Begin(((Component)_actionInfoContainer).gameObject, 0.3f, 1f);
		}
		OnClick_ActionInfo(null);
	}

	private void ShowActionInfo(CombatData.Action action)
	{
		_actionTitle.text = ((action != null) ? action.Name : string.Empty);
		_actionDescription.text = ((action != null) ? action.Description : string.Empty);
	}

	private void OnClick_ActionInfo(GameObject go)
	{
		EquipActionInfoIcon equipActionInfoIcon = null;
		for (int i = 0; i < _actionIcons.Count; i++)
		{
			EquipActionInfoIcon component = _actionIcons[i].GetComponent<EquipActionInfoIcon>();
			bool flag = (Object)(object)((Component)component).gameObject == (Object)(object)go;
			if (flag)
			{
				equipActionInfoIcon = component;
			}
			component.Select = flag;
		}
		CombatData.Action action = ((!((Object)(object)equipActionInfoIcon == (Object)null)) ? equipActionInfoIcon.Action : null);
		ShowActionInfo(action);
	}

	private void OnLayout(Point2 size)
	{
	}
}
