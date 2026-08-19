using System;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationList : MonoBehaviour
{
	[SerializeField]
	private GridContainer _motions;

	[SerializeField]
	private ListObjectPool _emotions;

	private UIWidget _widget;

	private AnimationWidget _animWidget;

	[NonSerialized]
	public KeyValuePair<string, string[]> SelectedMotion;

	[NonSerialized]
	public int SelectedEmotion = 1;

	private List<KeyValuePair<string, string[]>> _motionList;

	private bool _isShow;

	private bool _isInit;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	private AnimationWidget AnimWidget => (!((Object)(object)_animWidget == (Object)null)) ? _animWidget : (_animWidget = ((Component)this).GetComponent<AnimationWidget>());

	public event Action CommunicationSelected;

	public void Init()
	{
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		Dictionary<string, string[]> dictionary = KUtility.ParseJsonFile<Dictionary<string, string[]>>("MotionInfos/emotion_motion_map");
		ListObjectPool nodes = _motions.Nodes;
		nodes.Set(dictionary.Count);
		int num = 0;
		_motionList = new List<KeyValuePair<string, string[]>>();
		foreach (KeyValuePair<string, string[]> item in dictionary)
		{
			string text = $"#emotion_{item.Key}";
			GameObject val = nodes[num];
			((Object)val).name = item.Key;
			UIEventListener uIEventListener = UIEventListener.Get(val);
			uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, new UIEventListener.VoidDelegate(OnClickMotionButton));
			UILabel component = ((Component)val.transform.FindChild("text")).GetComponent<UILabel>();
			UISprite component2 = ((Component)val.transform.FindChild("icon")).GetComponent<UISprite>();
			component.text = LocalizeSystem.Get(text);
			component2.spriteName = IconMap.Get(text);
			_motionList.Add(item);
			num++;
		}
		_motions.Refresh();
		_emotions.Set(7);
		for (int i = 0; i < 7; i++)
		{
			GameObject val2 = _emotions[i];
			UISprite component3 = ((Component)val2.transform.FindChild("icon")).GetComponent<UISprite>();
			if (i == 0)
			{
				component3.spriteName = "button_hud_pen";
			}
			else
			{
				component3.spriteName = $"icon_emoticon_{i}";
			}
			UIUtility.ResizeToSquare(component3);
			UIEventListener uIEventListener2 = UIEventListener.Get(val2);
			uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, new UIEventListener.VoidDelegate(OnClickEmotionButton));
		}
		_emotions.Reposition(Vector3.right);
		WidgetLayoutController component4 = ((Component)this).GetComponent<WidgetLayoutController>();
		component4.UpdateLayout();
		NGUITools.UpdateWidgetCollider(((Component)this).gameObject);
		AnimWidget.SetAlpha(0f, useTween: false);
	}

	private void OnClickMotionButton(GameObject obj)
	{
		int index = _motions.Nodes.IndexOf(obj);
		SelectedMotion = _motionList[index];
		SelectedEmotion = -1;
		Hide();
		if (this.CommunicationSelected != null)
		{
			this.CommunicationSelected();
		}
	}

	private void OnClickEmotionButton(GameObject obj)
	{
		int selectedEmotion = _emotions.IndexOf(obj);
		SelectedEmotion = selectedEmotion;
		Hide();
		if (this.CommunicationSelected != null)
		{
			this.CommunicationSelected();
		}
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
