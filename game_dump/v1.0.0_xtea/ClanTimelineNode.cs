using UnityEngine;

public class ClanTimelineNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _textLabel;

	[SerializeField]
	private UILabel _atLabel;

	public void Set(string icon, string text, double at)
	{
		_iconSprite.spriteName = icon;
		_textLabel.text = text;
		double num = Connections.Frontend.GetPredictedServerTime() - at;
		_atLabel.text = ((!(num > 0.0)) ? null : TimerSystem.Timeago(num));
	}

	private void UpdateLayout()
	{
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		UIWidget component = ((Component)((Component)_textLabel).transform.parent).GetComponent<UIWidget>();
		component.height = (int)((float)_textLabel.height + (component.localCorners[1].y - _textLabel.GetPosition(0f, 1f).y) * 2f);
		WidgetLayoutController component2 = ((Component)this).GetComponent<WidgetLayoutController>();
		component2.UpdateLayout();
	}
}
