using System;
using System.Collections.Generic;
using L10N;
using UnityEngine;

public class WarehouseTabConfigItem : SelectableWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private GameObject _textButton;

	[SerializeField]
	private GameObject _upButton;

	[SerializeField]
	private GameObject _downButton;

	[SerializeField]
	private GameObject _removeButton;

	[SerializeField]
	private DefaultSelectableButton _addButton;

	private int _size;

	public string Text { get; private set; }

	public event Action<WarehouseTabConfigItem> OnChangeName;

	public event Action<WarehouseTabConfigItem> OnUp;

	public event Action<WarehouseTabConfigItem> OnDown;

	public event Action<WarehouseTabConfigItem> OnRemove;

	public event Action OnAdd;

	public void Start()
	{
		UIEventListener uIEventListener = UIEventListener.Get(_textButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.OnChangeName != null)
			{
				this.OnChangeName(this);
			}
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(_upButton);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.OnUp != null)
			{
				this.OnUp(this);
			}
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(_downButton);
		uIEventListener3.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener3.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.OnDown != null)
			{
				this.OnDown(this);
			}
		});
		UIEventListener uIEventListener4 = UIEventListener.Get(_removeButton);
		uIEventListener4.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener4.onClick, (UIEventListener.VoidDelegate)delegate
		{
			if (this.OnRemove != null)
			{
				this.OnRemove(this);
			}
		});
		DefaultSelectableButton addButton = _addButton;
		addButton.Clicked = (Action)Delegate.Combine(addButton.Clicked, (Action)delegate
		{
			if (this.OnAdd != null)
			{
				this.OnAdd();
			}
		});
	}

	public void Set(KeyValuePair<string, int> category)
	{
		Text = category.Key;
		_size = category.Value;
		_textLabel.text = Text;
		((Component)_addButton).gameObject.SetActive(false);
	}

	public void SetAddButton()
	{
		Text = null;
		((Component)_addButton).gameObject.SetActive(true);
		_addButton.Text = string.Format("[icon=img_plus_small] {0}", T._("탭 추가"));
	}
}
