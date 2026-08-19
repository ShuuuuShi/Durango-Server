using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI.Popup;

public class PvpIslandGuideItem : MonoBehaviour
{
	public UILabel Title;

	public UILabel Description;

	public UIWidget DescriptionWidget;

	private const int Padding = 24;

	public void Set(string title, string description)
	{
		Title.text = title;
		Description.text = description;
		DescriptionWidget.height = (int)Description.printedSize.y + 48;
		GetComponent<RectLayoutComponent>().UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}
}
