using UnityEngine;

namespace Durango.UI;

public class SkillTreeDepthNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UILabel _label;

	[SerializeField]
	private UISprite _line;

	private UIWidget _widget;

	public UIWidget Widget => (!(_widget == null)) ? _widget : (_widget = GetComponent<UIWidget>());

	public int Lv { get; private set; }

	public void Set(int level, int width)
	{
		Lv = level;
		_label.text = LocalizeUtil.FormatLevel(level);
		Widget.width = width;
		BgOffset(0f);
		BackgroundEnable(enable: false);
	}

	public void BackgroundEnable(bool enable)
	{
		_background.enabled = enable;
	}

	public void BgOffset(float offset)
	{
		_background.leftAnchor.absolute = (int)offset;
		_background.UpdateAnchors();
	}
}
