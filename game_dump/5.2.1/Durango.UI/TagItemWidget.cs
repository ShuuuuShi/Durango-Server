using Durango.Logic.Item;
using Durango.UI.Popup;
using JetBrains.Annotations;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class TagItemWidget : UIWidget
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _name;

	private Tag _tag;

	private int _level;

	public UILabel NameLabel => _name;

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying && !(GetComponent<Collider>() == null))
		{
			UIScrollView uIScrollView = UIUtility.FindComponentInParent<UIScrollView>(base.gameObject);
			if (!(uIScrollView == null))
			{
				base.gameObject.AddMissingComponent<UIDragScrollView>().scrollView = uIScrollView;
			}
		}
	}

	public void Set([NotNull] Tag data, int level)
	{
		_tag = data;
		_level = level;
		if (_name != null)
		{
			if (_tag.IsMajor())
			{
				_name.text = _tag.Name;
			}
			else
			{
				_name.text = TagData.GetNameWithLevel(_tag.Name, _level);
			}
			_name.color = TagData.GetGradeColor(_tag.Grade);
		}
		if (_icon != null)
		{
			_icon.spriteName = _tag.Icon;
		}
	}

	[UsedImplicitly]
	private void OnClick()
	{
		if (_tag != null && !string.IsNullOrEmpty(_tag.Description))
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			if (_tag.VisibleLevel)
			{
				widgetTooltipControl.Set(_tag.Name.ToString(), LocalizeUtil.FormatLevel(_level), new SyncString(_tag.Description), 500);
			}
			else
			{
				widgetTooltipControl.Set(_tag.Name, _tag.Description, 500);
			}
			widgetTooltipControl.Show(this, new Vector2(5f, 0f), 60f);
		}
	}
}
