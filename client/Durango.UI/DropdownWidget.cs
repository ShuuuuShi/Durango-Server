using System;
using System.Linq;
using Durango.Render.Camera;
using Durango.System.Config;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class DropdownWidget : SelectableWidget
{
	private const float MaxVisibleNodeCount = 10.5f;

	public Action<string> ValueSelected;

	protected bool IsCloseOnClick;

	protected string[] Options;

	[SerializeField]
	private KScrollView _scrollView;

	[SerializeField]
	private UILabel _title;

	[SerializeField]
	private UIWidget _container;

	[SerializeField]
	private Transform _containerAnchorTarget;

	[SerializeField]
	private GameObject _bg;

	[SerializeField]
	private int _titleButtonPadding;

	protected ListObjectPool Pool => _scrollView.Nodes;

	public int Index
	{
		get
		{
			if (Options != null && Setting != null)
			{
				return Options.IndexOf(Setting.Value as string);
			}
			return -1;
		}
	}

	public ValueSetting Setting { get; protected set; }

	public bool IsOpened => _container.gameObject.activeInHierarchy;

	private void Awake()
	{
		Clicked = delegate
		{
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			Open(!IsOpened);
		};
	}

	protected virtual void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouchScreen));
	}

	public virtual void Init(ValueSetting setting, string[] options, bool isCloseOnClick)
	{
		Setting = setting;
		Options = options;
		IsCloseOnClick = isCloseOnClick;
		if (Options != null)
		{
			Pool.Set(Options.Length);
			for (int i = 0; i < Options.Length; i++)
			{
				DropdownButton dropdownButton = Pool.Get<DropdownButton>(i);
				if (!(dropdownButton == null))
				{
					dropdownButton.Set(Localize(Options[i]), i);
					dropdownButton.ButtonClicked = OnClickButton;
				}
			}
		}
		UpdateTitle();
		UIUtility.UpdateAnchors(base.transform);
		Open(isOpen: false);
	}

	public virtual void SetValue(string value)
	{
		if (Setting != null)
		{
			Setting.Value = value;
			UpdateTitle();
		}
	}

	protected void SetTitle(string title)
	{
		_title.text = title;
	}

	private void UpdateTitle()
	{
		SetTitle((Setting != null) ? Localize(Setting.Value as string) : string.Empty);
	}

	private void OnTouchScreen(GameObject go, bool isPressed)
	{
		if (IsOpened && !(go == base.gameObject) && !(go == _bg) && !_scrollView.Nodes.Any((GameObject x) => x == go))
		{
			Open(isOpen: false);
		}
	}

	protected void OnClickButton(int index)
	{
		if (index < Options.Length)
		{
			if (ValueSelected != null)
			{
				ValueSelected(Options[index]);
			}
			SetCurrentButtonSelected();
			if (IsCloseOnClick)
			{
				Open(isOpen: false);
			}
		}
	}

	private string Localize(string text)
	{
		return (Setting != null) ? LocalizeSystem.Get("#config_" + Setting.Key + "_" + text) : null;
	}

	protected void Open(bool isOpen)
	{
		_container.gameObject.SetActive(isOpen);
		base.Selected = isOpen;
		if (isOpen)
		{
			Reposition();
			SetCurrentButtonSelected();
		}
	}

	private void SetCurrentButtonSelected()
	{
		int index = Index;
		for (int i = 0; i < Options.Length; i++)
		{
			DropdownButton dropdownButton = Pool.Get<DropdownButton>(i);
			if (!(dropdownButton == null))
			{
				dropdownButton.Selected = i == index;
			}
		}
		if (index != -1)
		{
			_scrollView.MoveToVisibleArea(index, instant: true);
		}
	}

	private void Reposition()
	{
		if (_scrollView.Nodes.Count != 0)
		{
			if (MainCamera.NGUIPosToScreenPos(UIUtility.ToRootPosition(base.gameObject)).y / (float)Screen.height > 0.3333f)
			{
				_container.pivot = UIWidget.Pivot.Top;
				_container.topAnchor.Set(base.transform, 0f, -_titleButtonPadding);
				_container.bottomAnchor.Set(_containerAnchorTarget, 0f, 0f);
			}
			else
			{
				_container.pivot = UIWidget.Pivot.Bottom;
				_container.topAnchor.Set(_containerAnchorTarget, 1f, 0f);
				_container.bottomAnchor.Set(base.transform, 1f, _titleButtonPadding);
			}
			_container.ResetAndUpdateAnchors();
			int height = _scrollView.Nodes[0].GetComponent<UIWidget>().height;
			int height2 = Mathf.Min(_container.height, Mathf.Min(_scrollView.Nodes.Count * height, (int)(10.5f * (float)height)));
			_container.height = height2;
			UIUtility.UpdateAnchors(_container.transform);
			_scrollView.Reposition();
		}
	}
}
