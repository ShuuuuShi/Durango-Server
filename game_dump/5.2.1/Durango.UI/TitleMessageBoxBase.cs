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
	private UILabel _title;

	[SerializeField]
	private UILabel _message;

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
		_message.text = message;
		base.gameObject.SetActive(value: true);
	}

	public virtual void Close()
	{
		base.gameObject.SetActive(value: false);
	}
}
