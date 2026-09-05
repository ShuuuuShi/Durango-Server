using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class TitleMessageBoxBase : MonoBehaviour
{
	[SerializeField]
	protected SelectableButton _okButton;

	[SerializeField]
	protected UILabel _title;

	[SerializeField]
	protected UILabel _message;

	[SerializeField]
	private SelectableButton _cancelButton;

	private Action _onOk;

	private Action _onCancel;

	protected virtual void Awake()
	{
		if (_okButton != null)
		{
			_okButton.Clicked = OnOk;
		}
		if (_cancelButton != null)
		{
			_cancelButton.Clicked = OnCancel;
		}
	}

	private void OnOk()
	{
		if (_onOk != null)
		{
			_onOk();
		}
		else
		{
			Close();
		}
	}

	private void OnCancel()
	{
		if (_onCancel != null)
		{
			_onCancel();
		}
		else
		{
			Close();
		}
	}

	private void OnEnable()
	{
		GameSystem<InputSystem>.Instance().On(InputCommand.Back, OnReceivedBackInputCommand);
	}

	private void OnDisable()
	{
		GameSystem<InputSystem>.Instance().Off(InputCommand.Back, OnReceivedBackInputCommand);
	}

	private void OnReceivedBackInputCommand(InputCommandMessage msg)
	{
		OnCancel();
	}

	public virtual void Show(string title, string message, Action onClick, Action onCancel = null, string okButtonLabel = null, string cancelButtonLabel = null)
	{
		_okButton.Text = (string.IsNullOrEmpty(okButtonLabel) ? ManualTranslator.Confirm : okButtonLabel);
		_cancelButton.gameObject.SetActive(onCancel != null);
		_cancelButton.Text = (string.IsNullOrEmpty(cancelButtonLabel) ? ManualTranslator.Cancel : cancelButtonLabel);
		_onOk = onClick;
		_onCancel = onCancel;
		_title.text = title;
		if (message.Length > 2500)
		{
			message = message.Substring(0, 2500);
		}
		if (_message != null)
		{
			_message.multiLine = true;
			_message.overflowMethod = UILabel.Overflow.ClampContent;
			_message.text = message;
		}
		LayoutDialog(onCancel == null);
		base.gameObject.SetActive(value: true);
	}

	/// <summary>ปุ่มเดียวชิดล่างของกล่อง ข้อความอยู่เหนือปุ่ม ไม่ทับกัน</summary>
	private void LayoutDialog(bool singleOk)
	{
		if (_okButton == null || _okButton.Widget == null)
		{
			return;
		}
		UIWidget ok = _okButton.Widget;
		if (singleOk)
		{
			ok.bottomAnchor.Set(0f, 16f);
			ok.topAnchor.Set(0f, 80f);
			if (_message != null)
			{
				_message.bottomAnchor.Set(0f, 96f);
				_message.UpdateAnchors();
			}
		}
		else
		{
			ok.bottomAnchor.absolute = 140;
			ok.topAnchor.absolute = 206;
		}
		ok.UpdateAnchors();
	}

	public virtual void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
