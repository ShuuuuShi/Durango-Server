using FatigueData;
using UnityEngine;
using Yaml;

public class WorldMapEnvNode : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private UISpriteLabel _title;

	[SerializeField]
	private UISpriteLabel _description;

	[SerializeField]
	private float _bottomMargin;

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

	public void Set(FatigueVelocity fatigueVelocity)
	{
		if (fatigueVelocity.CategoryData != null)
		{
			FatigueCategory categoryData = fatigueVelocity.CategoryData;
			_icon.spriteName = categoryData.icon;
			_title.text = categoryData.name;
			_description.text = categoryData.description;
			RefreshContainerWidgetSize();
		}
	}

	private void RefreshContainerWidgetSize()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		float num = _description.Label.printedSize.y + (0f - ((Component)_description).transform.localPosition.y) + _bottomMargin;
		Widget.height = (int)num;
	}
}
