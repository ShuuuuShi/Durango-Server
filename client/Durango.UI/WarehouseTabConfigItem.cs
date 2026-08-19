using System;
using System.Collections.Generic;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

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
	private RectLayout _layout;

	public string Text { get; private set; }

	public event Action<WarehouseTabConfigItem> OnChangeName;

	public event Action<WarehouseTabConfigItem> OnUp;

	public event Action<WarehouseTabConfigItem> OnDown;

	public event Action<WarehouseTabConfigItem> OnRemove;

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
		_layout.UpdateOnSizeChange();
	}

	public void Set(KeyValuePair<string, int> category)
	{
		Text = category.Key;
		_textLabel.text = Text;
	}
}
