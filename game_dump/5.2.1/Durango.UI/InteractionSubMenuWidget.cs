using Durango.Logic.Item;
using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class InteractionSubMenuWidget : SelectableWidget
{
	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UILabel _nameLabel;

	private Vector3 _baseNamePos;

	protected override void OnInit()
	{
		base.OnInit();
		_baseNamePos = _nameLabel.transform.localPosition;
	}

	public void Set(ItemIcon icon, string text, int sign)
	{
		Init();
		_iconTexture.SetIcon(icon);
		if (string.IsNullOrEmpty(text))
		{
			_nameLabel.gameObject.SetActive(value: false);
		}
		else
		{
			if (sign > 0)
			{
				_nameLabel.pivot = UIWidget.Pivot.Left;
				_nameLabel.transform.localPosition = _baseNamePos;
			}
			else
			{
				_nameLabel.pivot = UIWidget.Pivot.Right;
				Vector3 baseNamePos = _baseNamePos;
				baseNamePos.x = 0f - baseNamePos.x;
				_nameLabel.transform.localPosition = baseNamePos;
			}
			_nameLabel.gameObject.SetActive(value: true);
			_nameLabel.text = text;
		}
		UIUtility.UpdateAnchors(base.transform);
	}
}
