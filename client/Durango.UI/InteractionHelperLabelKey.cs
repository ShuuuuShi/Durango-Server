using UnityEngine;

namespace Durango.UI;

public class InteractionHelperLabelKey : MonoBehaviour
{
	[SerializeField]
	private UILabel _keyLabel;

	[SerializeField]
	private UILabel _descLabel;

	[SerializeField]
	private UISprite _descBg;

	[SerializeField]
	private float _paddingHeight;

	[SerializeField]
	private float _defaultPosY;

	[SerializeField]
	private int _descBgWidthPadding;

	public UIWidget Widget => GetComponent<UIWidget>();

	public float DefaultPosY => _defaultPosY;

	public float PosY
	{
		get
		{
			return base.transform.localPosition.y;
		}
		set
		{
			Vector3 localPosition = base.transform.localPosition;
			localPosition.y = value;
			base.transform.localPosition = localPosition;
		}
	}

	public float SecondaryPosY => DefaultPosY + _paddingHeight + (float)Widget.height;

	public int DescBgWidth
	{
		get
		{
			return _descBg.width;
		}
		set
		{
			_descBg.SetDimensions(value, _descBg.height);
			UpdatePosX();
		}
	}

	public void SetShortcut(InputCommand inputCommand, string description)
	{
		_keyLabel.text = $"<shortcut_box>{inputCommand}</shortcut_box>";
		_descLabel.text = description;
	}

	public void Activate(bool enable, bool enableDescription)
	{
		base.gameObject.SetActive(enable);
		_descBg.gameObject.SetActive(enableDescription);
		if (enable)
		{
			SetLayout(enableDescription);
		}
	}

	private void SetLayout(bool enableDescription)
	{
		if (enableDescription)
		{
			_descBg.SetDimensions(_descLabel.width + _descBgWidthPadding, _descBg.height);
		}
		UpdatePosX();
	}

	private void UpdatePosX()
	{
		Vector3 localPosition = base.transform.localPosition;
		localPosition.x = ((!_descBg.gameObject.activeInHierarchy) ? 0f : ((float)(_descBg.width + Widget.width) * -0.25f));
		base.transform.localPosition = localPosition;
	}
}
