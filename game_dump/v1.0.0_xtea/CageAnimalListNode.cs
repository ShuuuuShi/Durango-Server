using UnityEngine;

public class CageAnimalListNode : SelectableWidget
{
	[SerializeField]
	private UISprite _background;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _gaugeUpper;

	[SerializeField]
	private GameObject _selectWidget;

	[SerializeField]
	private GameObject _inCageIcon;

	[SerializeField]
	private GameObject _selector;

	private Gauge _hungry;

	public UISprite IconSprite => _iconSprite;

	public ulong Id { get; private set; }

	public void Set(ulong id, string animalName, string icon, Gauge hungry, bool inCage)
	{
		Id = id;
		_nameLabel.text = animalName;
		_iconSprite.spriteName = icon;
		_hungry = hungry;
		_inCageIcon.gameObject.SetActive(inCage);
	}

	public void SetColor(Color iconColor, Color bgColor, Color nameColor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		_nameLabel.color = nameColor;
		_iconSprite.color = iconColor;
		_background.color = bgColor;
	}

	private void Update()
	{
		_gaugeUpper.fillAmount = ((_hungry != null) ? _hungry.Ratio() : 0f);
	}

	protected override void OnSelected(bool isSelect)
	{
		base.OnSelected(isSelect);
		_selector.gameObject.SetActive(isSelect);
	}
}
