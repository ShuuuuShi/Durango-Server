using UnityEngine;

public class EquipStatItemWidget : MonoBehaviour
{
	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _nameLabel;

	[SerializeField]
	private UISprite _nameLabelBg;

	[SerializeField]
	private UILabel _valueLabel;

	[SerializeField]
	private UIWidget _splitLine;

	private Vector3 _iconPos;

	private Vector3 _valuePos;

	private Point2 _valueSize;

	private bool _isInit;

	private void Init()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		if (!_isInit)
		{
			_isInit = true;
			_iconPos = ((Component)_iconSprite).transform.localPosition;
			_valuePos = ((Component)_valueLabel).transform.localPosition;
			_valueSize = new Point2(_valueLabel.width, _valueLabel.height);
		}
	}

	public void Set(string icon, string name, bool lineActive)
	{
		Init();
		_iconSprite.spriteName = icon;
		UIUtility.ResizeToSquare(_iconSprite);
		_nameLabel.text = name;
		((Component)_splitLine).gameObject.SetActive(lineActive);
	}

	public void SetValue(string value)
	{
		_valueLabel.text = value;
	}

	public void UpdateLayout()
	{
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		Init();
		bool isPortraitMode = UIManager.IsPortraitMode;
		((Component)_nameLabel).gameObject.SetActive(!isPortraitMode);
		((Component)_nameLabelBg).gameObject.SetActive(!isPortraitMode);
		if (isPortraitMode)
		{
			UIWidget component = ((Component)this).GetComponent<UIWidget>();
			Vector3 val = Vector3.Lerp(component.localCorners[0], component.localCorners[1], 0.5f);
			_iconSprite.SetPosition(val + Vector3.right * 20f, 0f, 0.5f);
			Vector3 position = _iconSprite.GetPosition(1f, 0.5f);
			Vector3 val2 = Vector3.Lerp(component.localCorners[2], component.localCorners[3], 0.5f);
			((Component)_valueLabel).transform.localPosition = Vector3.Lerp(position, val2, 0.5f);
			_valueLabel.width = (int)(val2.x - position.x);
			_valueLabel.height = component.height;
		}
		else
		{
			((Component)_iconSprite).transform.localPosition = _iconPos;
			((Component)_valueLabel).transform.localPosition = _valuePos;
			_valueLabel.width = _valueSize.x;
			_valueLabel.height = _valueSize.y;
		}
		UIUtility.UpdateAnchors(((Component)this).transform);
	}
}
