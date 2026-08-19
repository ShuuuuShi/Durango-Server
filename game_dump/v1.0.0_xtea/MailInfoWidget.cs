using UnityEngine;

public class MailInfoWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _icon;

	[SerializeField]
	private ItemIconTex _itemIcon;

	[SerializeField]
	private UISprite _iconBG;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private UISprite _descriptionBG;

	public int Width => _iconBG.width + _descriptionBG.width + _descriptionBG.leftAnchor.absolute;

	public int Height => _iconBG.height;

	public void Set(string icon, string description)
	{
		_icon.spriteName = icon;
		UIUtility.ResizeToSquare(_icon);
		((Component)_itemIcon).gameObject.SetActive(false);
		SetDescription(description);
	}

	public void Set(string icon, ItemColor cols, string description)
	{
		_itemIcon.SetIcon(icon, cols);
		((Component)_icon).gameObject.SetActive(false);
		SetDescription(description);
	}

	private void SetDescription(string text)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		_description.text = text;
		_descriptionBG.width = Mathf.Max(150, _description.width + (int)Mathf.Abs(((Component)_description).transform.localPosition.x - ((Component)_descriptionBG).transform.localPosition.x) * 2);
	}
}
