using System;
using Durango.Logic.Social;
using Durango.Utils;
using UnityEngine;

namespace Durango.UI;

public class PinboardLine : MonoBehaviour
{
	public Action<string> NameLabelClicked;

	public Action HeightChanged;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private int _verticalPadding;

	[SerializeField]
	private int _textLeftMargin;

	[SerializeField]
	private int _textRightMargin;

	private bool _initialized;

	private UIWidget _widget;

	public string EntityId { get; private set; }

	public int Height => _widget.height;

	public Vector3 Position
	{
		get
		{
			return base.transform.localPosition;
		}
		set
		{
			base.transform.localPosition = value;
		}
	}

	public void Init()
	{
		if (!_initialized)
		{
			_initialized = true;
			_widget = GetComponent<UIWidget>();
			UIEventListener.Get(_nameLabel.gameObject).onClick = OnClickNameLabel;
		}
	}

	public void Clear(int width, Color colorBackground)
	{
		EntityId = string.Empty;
		_textLabel.text = string.Empty;
		_widget.width = width;
		_background.color = colorBackground;
	}

	public void AddContent(PinboardLineList.PinboardContent content)
	{
		string text = content.content + " [c][888888][size=16]" + Times.Timeago(content.at) + "[/size][-][/c]";
		if (_textLabel.text != string.Empty)
		{
			_textLabel.text = _textLabel.text + "\n" + text;
		}
		else
		{
			EntityId = content.id;
			_textLabel.text = text;
			_nameLabel.text = content.radio_id.name;
			_nameLabel.color = ((!(content.id == GameManager.PlayerId)) ? ChatStruct.ColorNameDefault : ChatStruct.ColorNameLocalPlayer);
			UpdateTextLabelPosition();
		}
		UpdateWidgetHeight();
	}

	private void UpdateWidgetHeight()
	{
		SetHeight(_textLabel.height + _verticalPadding * 2);
		_background.UpdateAnchors();
	}

	private void SetHeight(int value)
	{
		if (_widget.height != value)
		{
			_widget.height = value;
			if (HeightChanged != null)
			{
				HeightChanged();
			}
		}
	}

	private void UpdateTextLabelPosition()
	{
		Vector3 localPosition = _nameLabel.transform.localPosition;
		if (!string.IsNullOrEmpty(_nameLabel.text))
		{
			localPosition = _nameLabel.transform.localPosition + Vector3.right * (_nameLabel.width + _textLeftMargin);
		}
		_textLabel.transform.localPosition = localPosition;
		_textLabel.width = (int)((float)_widget.width - localPosition.x - (float)_textRightMargin);
		_textLabel.ProcessText();
	}

	private void OnClickNameLabel(GameObject obj)
	{
		if (NameLabelClicked != null)
		{
			NameLabelClicked(EntityId);
		}
	}
}
