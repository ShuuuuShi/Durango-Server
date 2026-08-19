using UnityEngine;

public class BoxWidgetNode : MonoBehaviour
{
	[SerializeField]
	public UILabel KeyLabel;

	[SerializeField]
	public UILabel Value;

	private UIWidget _widget;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());
}
