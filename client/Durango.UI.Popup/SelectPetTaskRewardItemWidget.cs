using System.Collections.Generic;
using Durango.UI.Control;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class SelectPetTaskRewardItemWidget : MonoBehaviour
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private GameObject _bonusSprite;

	private KeyValuePair<string, int> _item;

	public void Set(KeyValuePair<string, int> item, bool isBonus)
	{
		_item = item;
		_iconTexture.SetIcon(item.Key, item.Value);
		_bonusSprite.gameObject.SetActive(isBonus);
	}

	private void OnClick()
	{
		Prototype itemPrototype = PrototypeYaml.GetItemPrototype(_item.Key, _item.Value);
		if (itemPrototype != null)
		{
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, T._("{0} {1:lv:}", itemPrototype.Name, _item.Value));
			widgetTooltipControl.AutoPosition = false;
			widgetTooltipControl.Show(10f);
			widgetTooltipControl.SetPosition(GetComponent<UIWidget>(), new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), Vector2.up * 20f);
		}
	}
}
