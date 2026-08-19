using JetBrains.Annotations;
using StatisticsData;
using UnityEngine;

public class AutoGuideTitleNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private Color _selectedColor;

	private Color _labelDefaultColor;

	public Title Title { get; private set; }

	public bool Selected
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return _label.color == _selectedColor;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			if (_labelDefaultColor == default(Color))
			{
				_labelDefaultColor = _label.color;
			}
			_icon.color = ((!value) ? Color.white : _selectedColor);
			_label.color = ((!value) ? _labelDefaultColor : _selectedColor);
		}
	}

	public void Set([NotNull] Title title)
	{
		_label.text = title.Name;
		_icon.spriteName = title.Icon;
		Title = title;
	}
}
