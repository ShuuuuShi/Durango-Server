using UnityEngine;
using Yaml;

namespace Durango.UI;

public class PetTagPredictItemWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UIWidget _modifierWidget;

	[SerializeField]
	private ListObjectPool _modifierArrows;

	public void Set(Tag tagInfo, int modifierLevel)
	{
		_nameLabel.text = tagInfo.Name;
		GetComponent<UIWidget>().width = (int)_nameLabel.printedSize.x + 10;
		UIUtility.UpdateAnchors(base.transform);
		if (modifierLevel > 0)
		{
			_modifierWidget.gameObject.SetActive(value: true);
			_modifierArrows.Set(modifierLevel);
			UIUtility.WidgetsReposition(_modifierArrows, Vector3.down, Vector3.zero, -4f, 0.5f);
		}
		else
		{
			_modifierWidget.gameObject.SetActive(value: false);
		}
	}
}
