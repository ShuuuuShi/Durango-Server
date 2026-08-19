using System;
using L10N;
using UnityEngine;

public class PrologueLeftMenuListGroup : UIBase
{
	public UITweener _tweener;

	public GameObject _menuBtn;

	public float _width;

	public UIWidget _backGround;

	public GameObject _hambergerBG;

	public MenuListControl _skipButton;

	public MenuListControl _configButton;

	public MenuListControl _exitGameButton;

	private bool _isShow;

	private int _labelMaxWidth;

	public bool Show
	{
		get
		{
			return _isShow;
		}
		set
		{
			if (_isShow != value)
			{
				_isShow = value;
				if (_isShow)
				{
					((Component)_tweener).gameObject.SetActive(true);
					_tweener.tweenFactor = 0f;
					_tweener.PlayForward();
				}
				else
				{
					((Component)_tweener).gameObject.SetActive(false);
				}
				UIBase.HideUI(UIFlag.CoveredByClosable, _isShow);
			}
		}
	}

	private bool MenuBGVisible
	{
		set
		{
			if (value)
			{
				if ((Object)(object)_hambergerBG != (Object)null)
				{
					_hambergerBG.SetActive(true);
				}
			}
			else if ((Object)(object)_hambergerBG != (Object)null)
			{
				_hambergerBG.SetActive(false);
			}
		}
	}

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Main_Open_01.wav", "Sound/Effect/UI/UI_Menu_Main_Close_01.wav");
		UIBase.OnCloseCloseableUI += delegate
		{
			if (!UIBase.HasCloseable)
			{
				MenuBGVisible = true;
			}
		};
		UIBase.OnOpenCloseableUI += delegate
		{
			Close();
			MenuBGVisible = false;
		};
	}

	private void Start()
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		AddMenuList(_skipButton, T._("프롤로그 건너뛰기"), delegate
		{
			Close();
			UIManager.MessageBox.Show(T._("프롤로그를 정말로 건너뛰시겠습니까?"), delegate(bool ok)
			{
				if (ok)
				{
					KSingleton<PrologueManager>.Instance().SkipPrologue();
				}
			});
		});
		AddMenuList(_configButton, T._("설정"), delegate
		{
			Close();
			UIManager.Open<ConfigGroup>();
		});
		AddMenuList(_exitGameButton, T._("종료"), delegate
		{
			Close();
			UIBase.CloseUI();
		});
		_backGround.width = _labelMaxWidth + 150;
		UIEventListener uIEventListener = UIEventListener.Get(_menuBtn);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			HambergerClick();
		});
		((Component)_tweener).transform.localPosition = Vector3.right * _width / 2f;
		((Component)_tweener).gameObject.SetActive(false);
		MenuBGVisible = true;
	}

	private void AddMenuList(MenuListControl menu, string text, Action func)
	{
		menu.MenuLabel = text;
		menu.Clicked = (Action)Delegate.Combine(menu.Clicked, func);
		_labelMaxWidth = Mathf.Max(menu.GetLabelWidth(), _labelMaxWidth);
	}

	private void HambergerClick()
	{
		if (UIBase.HasCloseable)
		{
			UIBase.CloseUI(forceClose: true);
		}
		else if (base.IsOpen)
		{
			Close();
		}
		else
		{
			Open();
		}
	}

	protected override bool OnOpen()
	{
		if (!Show)
		{
			Show = true;
			MenuBGVisible = false;
			return true;
		}
		return false;
	}

	protected override bool OnClose()
	{
		Show = false;
		if (!UIBase.HasCloseable)
		{
			MenuBGVisible = true;
		}
		return true;
	}
}
