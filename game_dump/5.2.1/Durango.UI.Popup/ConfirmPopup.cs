using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI.Popup;

public class ConfirmPopup : TooltipBase
{
	[SerializeField]
	protected UILabel TextLabel;

	[SerializeField]
	protected SelectableButton ButtonBase;

	protected ListObjectPool<SelectableButton> Buttons = new ListObjectPool<SelectableButton>();

	private readonly List<MessageBox.Button> _buttonTexts = new List<MessageBox.Button>();

	private readonly List<Action> _onActions = new List<Action>();

	private Action _onCancel;

	private bool _isConfirm;

	public override bool DragLock
	{
		get
		{
			return true;
		}
		set
		{
		}
	}

	protected override void OnAwake()
	{
		base.OnAwake();
		if (base.ModalBox != null)
		{
			UIEventListener uIEventListener = UIEventListener.Get(base.ModalBox);
			uIEventListener.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener.onDrag, new UIEventListener.VectorDelegate(UIManager.IgnoreUIDrag));
		}
		Buttons.BaseObject = ButtonBase;
		Buttons.UseBase = true;
		Buttons.Init(delegate(SelectableButton btn)
		{
			btn.Clicked = OnClickButton;
		});
	}

	private void OnClickButton()
	{
		int num = Buttons.IndexOf((SelectableButton)Selectable.Current);
		if (num != -1)
		{
			_onActions.Get(num)?.Invoke();
			_isConfirm = true;
			Hide();
		}
	}

	protected override void OnShow()
	{
		base.OnShow();
		_isConfirm = false;
	}

	protected override void OnHide()
	{
		base.OnHide();
		if (!_isConfirm && _onCancel != null)
		{
			_onCancel();
		}
		Clear();
	}

	protected override void FillData()
	{
		Buttons.BeginLoad();
		for (int i = 0; i < _buttonTexts.Count; i++)
		{
			MessageBox.Button button = _buttonTexts[i];
			if (!string.IsNullOrEmpty(button.Text))
			{
				SelectableButton next = Buttons.GetNext();
				next.SetStyle(button.Style);
				next.Text = button.Text;
			}
		}
		Buttons.EndLoad();
	}

	protected override void UpdateLayout()
	{
		Vector3 vector = new Vector3(30f, -32f);
		TextLabel.SetPosition(vector, 0f, 1f);
		vector.y -= TextLabel.height;
		vector.y -= 26f;
		UIUtility.WidgetsReposition(Buttons, Vector3.right, vector + Vector3.down * ButtonBase.Widget.height * 0.5f, 5f);
		float num = 0f - vector.y + (float)ButtonBase.Widget.height;
		base.Widget.height = (int)num;
		UIUtility.UpdateAnchors(base.transform);
		Vector3 position = UIRootAnchor.GetRootAnchor(UIBase.AnchorType.Base).GetPosition(0f, 0.5f);
		base.Widget.SetPosition(position, 0f, 0.5f);
	}

	public void Clear()
	{
		_buttonTexts.Clear();
		_onActions.Clear();
		_onCancel = null;
	}

	public ConfirmPopup AddButton(MessageBox.Button text, Action action)
	{
		_buttonTexts.Add(text);
		_onActions.Add(action);
		return this;
	}

	public ConfirmPopup OnCancel(Action action)
	{
		_onCancel = action;
		return this;
	}

	public bool Show(string comment, float duration)
	{
		VisibleState state = base.State;
		if (state == VisibleState.FadeIn || state == VisibleState.Show)
		{
			if (_onCancel != null)
			{
				_onCancel();
			}
			return false;
		}
		TextLabel.text = comment;
		Show(duration);
		return true;
	}
}
