using System;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class ArtifactInfoManageRightsNode : UIWidget
{
	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private BinaryToggleSlider _toggleButton;

	private bool _originValue;

	private int? _originInventoryAccessCount;

	private bool _isInit;

	public bool Value { get; private set; }

	public bool IsChanged
	{
		get
		{
			int result;
			if (Value == _originValue)
			{
				int? originInventoryAccessCount = _originInventoryAccessCount;
				int valueOrDefault = originInventoryAccessCount.GetValueOrDefault();
				int? inventoryAccessCount = InventoryAccessCount;
				result = ((valueOrDefault != inventoryAccessCount.GetValueOrDefault() || (originInventoryAccessCount.HasValue ^ inventoryAccessCount.HasValue)) ? 1 : 0);
			}
			else
			{
				result = 1;
			}
			return (byte)result != 0;
		}
	}

	public string Text { get; private set; }

	public int? InventoryAccessCount { get; private set; }

	public event Action<ArtifactInfoManageRightsNode> InventoryAccessEditClicked;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_toggleButton.ValueChanged = OnValueChanged;
		}
	}

	private void OnValueChanged(bool on)
	{
		Value = on;
		if (on && InventoryAccessCount.HasValue)
		{
			if (this.InventoryAccessEditClicked != null)
			{
				this.InventoryAccessEditClicked(this);
			}
		}
		else
		{
			TextUpdate();
		}
	}

	public void Set(string text, bool access, int? inventoryAccessCount)
	{
		Init();
		_originValue = access;
		_originInventoryAccessCount = inventoryAccessCount;
		Text = text;
		InventoryAccessCount = inventoryAccessCount;
		Value = access;
		_toggleButton.Set((!access) ? 0f : 1f);
		TextUpdate();
	}

	public void ChangeInventoryAccessCount(int? inventoryAccessCount)
	{
		InventoryAccessCount = inventoryAccessCount;
		TextUpdate();
	}

	private void TextUpdate()
	{
		string text = Text;
		if (InventoryAccessCount.HasValue)
		{
			text = ((!Value) ? string.Format("{0} [FFFFFF4C][icon=img_market_arrowright][-]\n[size=20][BBBBBB]{1}[-][/size]", text, T._("사유지 권한에 따름")) : (InventoryAccessCount.Value switch
			{
				-1 => string.Format("{0} [FFFFFF4C][icon=img_market_arrowright][-]\n[size=20][BBBBBB]<em>{1}</em>[-][/size]", text, T._("무제한")), 
				0 => string.Format("{0} [FFFFFF4C][icon=img_market_arrowright][-]\n[size=20][BBBBBB]{1}[-][/size]", text, T._("꺼내기 불가")), 
				_ => string.Format("{0} [FFFFFF4C][icon=img_market_arrowright][-]\n[size=20][BBBBBB]{1}[-][/size]", text, T._("{0} 마다 <em>{1}개</em> 꺼내기 가능", TimedeltaFormatter.Format(OptionSystem.GetInventoryAccessRefreshPeriod() * 60 * 60, 1, "hour"), InventoryAccessCount.Value)), 
			}));
		}
		_textLabel.text = text;
	}

	private void OnClick()
	{
		if (this.InventoryAccessEditClicked != null)
		{
			this.InventoryAccessEditClicked(this);
		}
	}
}
