using System;
using System.Collections.Generic;
using UnityEngine;

public class QuickChatList : MonoBehaviour
{
	[SerializeField]
	private ListObjectPool _buttons;

	[SerializeField]
	private int _minWidth;

	private UIWidget _widget;

	private AnimationWidget _animWidget;

	private List<string> _chatList;

	private bool _isShow;

	private bool _isInit;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	private AnimationWidget AnimWidget => (!((Object)(object)_animWidget == (Object)null)) ? _animWidget : (_animWidget = ((Component)this).GetComponent<AnimationWidget>());

	public event Action<string> QuickChatClicked;

	private void Init()
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_chatList = LocalizeSystem.GetSequenceKeys("#quick_chat_", numberOnly: true);
		_buttons.Set(_chatList.Count);
		int num = _minWidth;
		for (int i = 0; i < _chatList.Count; i++)
		{
			GameObject val = _buttons[i];
			((Object)val).name = _chatList[i];
			UIEventListener uIEventListener = UIEventListener.Get(val);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickQuickChatButton));
			UILabel component = ((Component)val.transform.FindChild("text")).GetComponent<UILabel>();
			component.text = LocalizeSystem.Format(_chatList[i], "(X Y)");
			UIWidget component2 = val.GetComponent<UIWidget>();
			component2.width = component.width + 22;
			NGUITools.UpdateWidgetCollider(val);
			num = Mathf.Max(num, component2.width);
		}
		Vector3 val2 = Widget.localCorners[0] + new Vector3(10f, 10f);
		int height = _buttons.BaseObject.GetComponent<UIWidget>().height;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		for (int j = 0; j < _buttons.Count; j++)
		{
			UIWidget component3 = _buttons[j].GetComponent<UIWidget>();
			if (num3 + component3.width > num)
			{
				num2 += height + 5;
				num4 = Mathf.Max(num4, num3);
				num3 = component3.width + 5;
				component3.SetPosition(val2 + Vector3.up * (float)num2, 0f, 0f);
			}
			else
			{
				component3.SetPosition(val2 + Vector3.right * (float)num3 + Vector3.up * (float)num2, 0f, 0f);
				num3 += component3.width + 5;
			}
		}
		Widget.width = num4 + 20;
		Widget.height = num2 + height + 20;
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
		UIUtility.UpdateAnchors(((Component)this).transform);
		AnimWidget.SetAlpha(0f, useTween: false);
	}

	private void Start()
	{
		if (!_isShow)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnTouch(GameObject obj, bool press)
	{
		if (press && _isShow)
		{
			Transform child = ((!((Object)(object)obj == (Object)null)) ? obj.transform : null);
			if (!NGUITools.IsChild(((Component)this).transform, child))
			{
				Hide();
			}
		}
	}

	private void OnClickQuickChatButton(GameObject obj)
	{
		int num = _buttons.IndexOf(obj);
		if (num != -1 && this.QuickChatClicked != null)
		{
			this.QuickChatClicked(_chatList[num]);
		}
		Hide();
	}

	public void Show()
	{
		Init();
		_isShow = true;
		((Component)this).gameObject.SetActive(true);
		AnimWidget.Alpha = 1f;
	}

	public void Hide()
	{
		_isShow = false;
		AnimWidget.Alpha = 0f;
	}
}
