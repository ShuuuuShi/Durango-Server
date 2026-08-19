using System;
using System.Collections;
using L10N;
using UnityEngine;

public class MessageBox : UIBase
{
	[SerializeField]
	private GameObject _messageBox;

	[SerializeField]
	private UIWidget _buttonContainer;

	[SerializeField]
	private ListObjectPool _buttons;

	[SerializeField]
	private UIWidget _background;

	[SerializeField]
	private UISpriteLabel _commentLabel;

	[SerializeField]
	private TweenAlpha _mainContainer;

	[SerializeField]
	private int _yMargin;

	[SerializeField]
	private int _minButtonWidth;

	[SerializeField]
	private int _buttonPadding;

	[SerializeField]
	private int _buttonMargin;

	private Action _onOk;

	private Action<bool> _onOkCancel;

	private Action<int> _onSelect;

	private UIWidget _customWidget;

	private int _bgStartHeight;

	private int _nguiOver;

	private bool _isWait;

	private AnimationWidget _animWidget;

	public AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = _messageBox.GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public bool IsShow { get; private set; }

	private void Start()
	{
		NGUITools.SetLayer(((Component)this).gameObject, UIManager.UIOverLayer);
		_bgStartHeight = _background.height;
		_messageBox.gameObject.SetActive(false);
		_buttons.Init(Init_Buttons);
	}

	private void Init_Buttons(GameObject obj)
	{
		UIEventListener.Get(obj).onClick = OnClick_Buttons;
	}

	private void OnClick_Buttons(GameObject obj)
	{
		int num = _buttons.IndexOf(obj);
		if (num != -1)
		{
			if (_onOk != null)
			{
				if (num == 0)
				{
					_onOk();
				}
			}
			else if (_onOkCancel != null)
			{
				switch (num)
				{
				case 0:
					_onOkCancel(obj: true);
					break;
				case 1:
					_onOkCancel(obj: false);
					break;
				}
			}
			else if (_onSelect != null)
			{
				_onSelect(num);
			}
		}
		Hide();
	}

	public void Show(string comment, UIWidget customWidget = null)
	{
		Show(comment, customWidget, (Action)null);
	}

	public void Show(string comment, Action onOk)
	{
		Show(comment, null, onOk);
	}

	public void Show(string comment, UIWidget customWidget, Action onOk)
	{
		_onOk = onOk;
		_onOkCancel = null;
		_onSelect = null;
		Setting(comment, customWidget, T._("확인"));
	}

	public void Show(string comment, Action<bool> onOkCancel)
	{
		Show(comment, null, onOkCancel);
	}

	public void Show(string comment, UIWidget customWidget, Action<bool> onOkCancel)
	{
		_onOk = null;
		_onOkCancel = onOkCancel;
		_onSelect = null;
		Setting(comment, customWidget, T._("확인"), T._("취소"));
	}

	public void Show(string comment, Action<int> onSelect, params string[] items)
	{
		Show(comment, null, onSelect, items);
	}

	public void Show(string comment, UIWidget customWidget, Action<int> onSelect, params string[] items)
	{
		_onOk = null;
		_onOkCancel = null;
		_onSelect = onSelect;
		Setting(comment, customWidget, items);
	}

	private void Setting(string comment, UIWidget customWidget, params string[] buttons)
	{
		_background.width = UIManager.ScreenWidth;
		_background.height = UIManager.ScreenHeight;
		SetComment(comment);
		SetCustomWidget(customWidget);
		SetButtons(buttons);
		LoadingCurtainGroup loadingCurtainGroup = UIManager.FindScript<LoadingCurtainGroup>();
		if ((Object)(object)loadingCurtainGroup != (Object)null && loadingCurtainGroup.IsVisible)
		{
			if (!_isWait)
			{
				((MonoBehaviour)this).StartCoroutine(CoLateShow());
			}
		}
		else
		{
			LateShow();
		}
	}

	private void SetComment(string comment)
	{
		if (string.IsNullOrEmpty(comment))
		{
			((Component)_commentLabel).gameObject.SetActive(false);
			return;
		}
		_commentLabel.Label.width = UIManager.ScreenWidth - 200;
		_commentLabel.text = comment;
		((Component)_commentLabel).gameObject.SetActive(true);
	}

	private void SetCustomWidget(UIWidget widget)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_customWidget == (Object)(object)widget))
		{
			if ((Object)(object)_customWidget != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)_customWidget).gameObject);
			}
			_customWidget = widget;
			if ((Object)(object)_customWidget != (Object)null)
			{
				((Component)_customWidget).transform.parent = ((Component)_mainContainer).transform;
				((Component)_customWidget).transform.localScale = Vector3.one;
				NGUITools.SetLayer(((Component)_customWidget).gameObject, UIManager.UIOverLayer);
			}
		}
	}

	private void SetButtons(string[] buttons)
	{
		int size = KUtility.GetSize(buttons);
		_buttons.Set(size);
		for (int i = 0; i < size; i++)
		{
			DefaultSelectableButton component = _buttons[i].GetComponent<DefaultSelectableButton>();
			component.Text = buttons[i];
		}
	}

	private IEnumerator CoLateShow()
	{
		_isWait = true;
		LoadingCurtainGroup loadingCurtain = UIManager.FindScript<LoadingCurtainGroup>();
		while ((Object)(object)loadingCurtain != (Object)null && loadingCurtain.IsVisible)
		{
			yield return null;
		}
		_isWait = false;
		LateShow();
	}

	private void LateShow()
	{
		if (_buttons.Count == 0)
		{
			Show(isShow: false);
			return;
		}
		UpdateLayout();
		Show(isShow: true);
	}

	private void UpdateLayout()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01da: Unknown result type (might be due to invalid IL or missing references)
		//IL_01df: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0302: Unknown result type (might be due to invalid IL or missing references)
		int count = _buttons.Count;
		int num = _minButtonWidth;
		for (int i = 0; i < count; i++)
		{
			DefaultSelectableButton component = _buttons[i].GetComponent<DefaultSelectableButton>();
			num = Mathf.Max((int)component.TextLabel.printedSize.x, num);
		}
		int num2 = _buttonContainer.height;
		if (((Component)_commentLabel).gameObject.activeSelf)
		{
			num2 += _commentLabel.Label.height;
		}
		if ((Object)(object)_customWidget != (Object)null)
		{
			num2 += _customWidget.height;
		}
		float num3 = (float)num2 * 0.5f;
		if (((Component)_commentLabel).gameObject.activeSelf)
		{
			_commentLabel.Label.SetPosition(new Vector3(0f, num3), 0.5f, 1f);
			num3 -= (float)_commentLabel.Label.height;
		}
		if ((Object)(object)_customWidget != (Object)null)
		{
			_customWidget.SetPosition(new Vector3(0f, num3), 0.5f, 1f);
			num3 -= (float)_customWidget.height;
		}
		_buttonContainer.SetPosition(new Vector3(0f, num3), 0.5f, 1f);
		num += _buttonPadding;
		Vector3 val = default(Vector3);
		((Vector3)(ref val))._002Ector((float)(-(num + _buttonMargin) * (count - 1)) / 2f, 0f);
		for (int j = 0; j < count; j++)
		{
			DefaultSelectableButton component2 = _buttons[j].GetComponent<DefaultSelectableButton>();
			component2.Widget.width = num;
			Vector3 localPosition = val + Vector3.right * (float)(num + _buttonMargin) * (float)j;
			((Component)component2).transform.localPosition = localPosition;
		}
		bool flag = !_messageBox.gameObject.activeSelf;
		_messageBox.gameObject.SetActive(true);
		if (flag)
		{
			AnimWidget.Widget.alpha = 0f;
			_mainContainer.tweenFactor = 0f;
			((Component)_mainContainer).GetComponent<UIWidget>().alpha = 0f;
			_mainContainer.PlayForward();
			_background.height = _bgStartHeight;
			TweenHeight component3 = ((Component)_background).GetComponent<TweenHeight>();
			component3.from = _background.height;
			component3.to = num2 + _yMargin;
			component3.tweenFactor = 0f;
			component3.PlayForward();
			Vector3 localPosition2 = ((Component)_buttonContainer).transform.localPosition;
			TweenPosition component4 = ((Component)_buttonContainer).GetComponent<TweenPosition>();
			component4.from = localPosition2 + Vector3.down * 5f;
			component4.to = localPosition2;
			component4.tweenFactor = 0f;
			component4.PlayForward();
			TweenAlpha component5 = ((Component)_buttonContainer).GetComponent<TweenAlpha>();
			component5.from = 0f;
			component5.to = 1f;
			component5.tweenFactor = 0f;
			component5.PlayForward();
			_buttonContainer.alpha = 0f;
		}
	}

	public void SetButtonText(int index, string text)
	{
		if (index >= 0 && index < _buttons.Count)
		{
			DefaultSelectableButton component = _buttons[index].GetComponent<DefaultSelectableButton>();
			component.Text = text;
		}
	}

	public void Hide()
	{
		_onOk = null;
		_onOkCancel = null;
		_onSelect = null;
		Show(isShow: false);
	}

	private void Show(bool isShow)
	{
		if (isShow != IsShow)
		{
			IsShow = isShow;
			if (isShow)
			{
				BlurController.BlurOn("MessageBox", BlurController.Mask.UI);
				AnimWidget.Alpha = 1f;
			}
			else
			{
				BlurController.BlurOff("MessageBox");
				AnimWidget.Alpha = 0f;
			}
		}
	}
}
