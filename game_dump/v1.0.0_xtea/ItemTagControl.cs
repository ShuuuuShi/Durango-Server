using L10N;
using UnityEngine;

public class ItemTagControl : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _dotted;

	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _name;

	[SerializeField]
	private UILabel _level;

	private UIWidget _widget;

	public UIWidget Widget
	{
		get
		{
			if ((Object)(object)_widget == (Object)null)
			{
				_widget = ((Component)this).GetComponent<UIWidget>();
			}
			return _widget;
		}
	}

	public string Name
	{
		get
		{
			return UIUtility.GetLabelText(_name);
		}
		set
		{
			UIUtility.SetLabelText(_name, value);
		}
	}

	public UILabel NameLabel => _name;

	public int Level
	{
		set
		{
			UIUtility.SetLabelText(_level, T.Format("{0:lv:}", value));
		}
	}

	public string Icon
	{
		get
		{
			return UIUtility.GetSpriteName(_icon);
		}
		set
		{
			UIUtility.SetSpriteName(_icon, value);
		}
	}

	public Color IconColor
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return (!((Object)(object)_icon != (Object)null)) ? Color.clear : _icon.color;
		}
		set
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_icon != (Object)null)
			{
				_icon.color = value;
			}
		}
	}

	public Color FontColor
	{
		get
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			return (!((Object)(object)_name != (Object)null)) ? Color.clear : _name.color;
		}
		set
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_name != (Object)null)
			{
				_name.color = value;
			}
		}
	}

	public Color BackgroundColor
	{
		get
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			return ((Object)(object)_background != (Object)null) ? _background.color : ((!((Object)(object)_dotted != (Object)null)) ? Color.clear : _dotted.color);
		}
		set
		{
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_background != (Object)null)
			{
				_background.color = value;
			}
			if ((Object)(object)_dotted != (Object)null)
			{
				_dotted.color = value;
			}
		}
	}

	public bool DottedLine
	{
		get
		{
			return (Object)(object)_dotted != (Object)null && ((Component)_dotted).gameObject.activeSelf;
		}
		set
		{
			if ((Object)(object)_dotted != (Object)null)
			{
				((Component)_dotted).gameObject.SetActive(value);
			}
			if ((Object)(object)_background != (Object)null)
			{
				((Component)_background).gameObject.SetActive(!value);
			}
		}
	}
}
