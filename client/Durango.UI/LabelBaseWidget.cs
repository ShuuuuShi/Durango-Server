using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class LabelBaseWidget : MonoBehaviour
{
	[SerializeField]
	private UISpriteLabel _label;

	[SerializeField]
	private UISprite _line;

	[SerializeField]
	private UISprite _bg;

	public UISpriteLabel Label => _label;

	public GameObject BgLine => _line.gameObject;

	private void Awake()
	{
		ShowBg(show: false);
	}

	public void ShowBg(bool show)
	{
		_bg.gameObject.SetActive(show);
	}
}
